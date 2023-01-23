using Core;
using Core.Entities;
using Core.Interfaces;
using Core.ValueObjects;

namespace Application.Services
{
    public class TravelService : ITravelService
    {
        private readonly IUniverseRepository _universeRepository;
        private readonly EngineRouteConfiguration _engineRouteConfiguration;

        public TravelService(IUniverseRepository universeRepository, EngineRouteConfiguration engineRouteConfiguration)
        {
            _universeRepository = universeRepository;
            _engineRouteConfiguration = engineRouteConfiguration;
        }
        
        public async Task<ITravelComputingReport> EvaluateTravelOddsAsync(EmpirePlan empirePlan)
        {
            var routes = await _universeRepository.ListAsync();

            // -> STEP 1 : RESOLVING ALL POSSIBLE PATHS
            ////////////////////////////////////////////////////////////////
            var routePaths = FindAllPossibleRoutePaths(routes);

            //Debug.WriteLine("\n-> Possible route paths :");
            //Debug.WriteLine("------------------------------");
            //foreach (var routeList in routePaths)
            //    Debug.WriteLine($"{string.Join(" -> ", routeList.Select(m => m.ORIGIN))} -> {routeList.Last().DESTINATION}");

            // -> STEP 1 : RESOLVING ALL POSSIBLE PATHS
            ////////////////////////////////////////////////////////////////
            var travelEvaluations = BuildRoutePathsTravelEvaluations(empirePlan, routePaths);

            //Debug.WriteLine("\n-> Travel positions :");
            //Debug.WriteLine("------------------------------");
            //foreach (var evaluation in travelEvaluations)
            //    Debug.WriteLine($"{evaluation.Odds}% ==> {string.Join(" -> ", evaluation.TravelPositions.Select(m =>
            //    {
            //        if (m.BountyHunters && m.Refuel)
            //            return $"{m.Planet} ({m.Day}:BountyHunters:Refuel)";

            //        if (m.Refuel)
            //            return $"{m.Planet} ({m.Day}:Refuel)";

            //        if (m.BountyHunters)
            //            return $"{m.Planet} ({m.Day}:BountyHunters)";

            //        if (m.Waiting)
            //            return $"{m.Planet} ({m.Day}:waiting)";

            //        return $"{m.Planet} ({m.Day}) ";
            //    }))}");

            var minTravelTime = travelEvaluations.Min(m => m.TravelDays);
            var maxTravelTime = travelEvaluations.Max(m => m.TravelDays);
            var bestOdds = travelEvaluations.Max(m => m.Odds);
            var bestTravelTime = travelEvaluations.Where(m => m.Odds == bestOdds).Max(m => m.TravelDays);
            var doable = bestTravelTime <= empirePlan.countdown;
            
            return new TravelComputingReport
            {
                Doable = doable,
                BestOdds = bestOdds,
                MinTravelTime = minTravelTime,
                MaxTravelTime = maxTravelTime,
                BestTravelTime = bestTravelTime,
                
                BestEvaluations = doable 
                    ? travelEvaluations.Where(m => m.Odds == bestOdds).ToList()
                    : null,
                OtherPossibleEvaluations = doable
                    ? travelEvaluations.Where(m => m.Odds != bestOdds && m.Odds > 0).ToList()
                    : null,
                ImpossibleEvaluations = doable
                    ? travelEvaluations.Where(m => m.Odds == 0).ToList()
                    : travelEvaluations
            };
        }

        private IList<ITravelEvaluation> BuildRoutePathsTravelEvaluations(
            EmpirePlan empirePlan, 
            IList<IList<RouteEntity>> routePaths)
        {
            var travelEvaluations = new List<ITravelEvaluation>();

            foreach (var routePath in routePaths)
            {
                // -> STEP 1 -> Evaluating most obvious path steps (no waiting)
                /////////////////////////////////////////////////////////////////////////////////////////
                var travelEvaluation = Evaluate(empirePlan, routePath);
                travelEvaluations.Add(travelEvaluation);

                // -> STEP 2 -> Evaluating route path with waiting strategy
                /////////////////////////////////////////////////////////////////////////////////////////
                if (travelEvaluation.TravelDays < empirePlan.countdown)
                {
                    // -> 
                    for (var spareDays = empirePlan.countdown - travelEvaluation.TravelDays; spareDays > 0; spareDays--)
                    {

                        for (var i = 0; i < routePath.Count; i++)
                        {
                            List<TravelWaitingStep> waitingStrategy;

                            if (i == 0)
                            {
                                waitingStrategy = new List<TravelWaitingStep> { new(routePath[i].Origin, spareDays) };
                                travelEvaluation = Evaluate(empirePlan, routePath, waitingStrategy);

                                travelEvaluations.Add(travelEvaluation);
                            }

                            waitingStrategy = new List<TravelWaitingStep> { new(routePath[i].Destination, spareDays) };
                            travelEvaluation = Evaluate(empirePlan, routePath, waitingStrategy);

                            travelEvaluations.Add(travelEvaluation);
                        }
                    }
                }
            }

            return travelEvaluations.OrderByDescending(m => m.Odds).ToList();
        }

        /// <summary>
        /// Evaluate a route path base on initial configuration and intercepted empire plan
        /// </summary>
        /// <param name="empirePlan">Intercepted empire plan</param>
        /// <param name="routePath">Route path to evaluate</param>
        /// <param name="waitingStrategy">Optional waiting strategy</param>
        /// <returns>Route path evaluation</returns>
        private TravelEvaluation Evaluate(
            EmpirePlan empirePlan, 
            IList<RouteEntity> routePath, 
            IList<TravelWaitingStep> waitingStrategy = null)
        {
            var currentDay = 0;
            var currentPlanet = Planet.From(routePath.First().Origin);
            var currentAutonomy = _engineRouteConfiguration.autonomy;
            var travelEvaluation = new TravelEvaluation(empirePlan, currentDay, currentPlanet);

            ManageWaitingStep(ref currentDay, currentPlanet, travelEvaluation, waitingStrategy);

            for (var i = 0; i < routePath.Count; i++)
            {
                var route = routePath[i];
                currentPlanet = Planet.From(route.Destination);
                currentDay += route.TravelTime;

                if (currentAutonomy - route.TravelTime == 0 || (i + 1 < routePath.Count && currentAutonomy - routePath[i + 1].TravelTime <= 0))
                {
                    travelEvaluation.AddPosition(currentDay, currentPlanet);

                    // -> refuel required to reach next step
                    currentDay++;
                    currentAutonomy = _engineRouteConfiguration.autonomy;

                    travelEvaluation.AddPosition(currentDay, currentPlanet, Core.TravelPosition.Refuel);
                }
                else
                {
                    currentAutonomy -= route.TravelTime;
                    travelEvaluation.AddPosition(currentDay, currentPlanet);
                }

                ManageWaitingStep(ref currentDay, currentPlanet, travelEvaluation, waitingStrategy);
            }

            return travelEvaluation;
        }

        /// <summary>
        /// Manage the condition to wait on a planet based on a given strategy
        /// </summary>
        /// <param name="currentDay">Reference current day</param>
        /// <param name="currentPlanet">Current planet to handle</param>
        /// <param name="travelEvaluation">Current travel evaluation to apply on</param>
        /// <param name="waitingStrategy">Waiting strategy</param>
        private void ManageWaitingStep(ref int currentDay, Planet currentPlanet, TravelEvaluation travelEvaluation, IList<TravelWaitingStep> waitingStrategy)
        {
            var rest = waitingStrategy?.FirstOrDefault(m => m.Planet.Equals(currentPlanet));
            if (rest != null)
            {
                currentDay += rest.Day;
                travelEvaluation.AddPosition(currentDay, currentPlanet, Core.TravelPosition.Waiting);
            }
        }

        /// <summary>
        /// Find all possible paths based on configuration
        /// </summary>
        /// <param name="allRoutes">All universe routes</param>
        /// <returns>All possible routes as a list of route</returns>
        private IList<IList<RouteEntity>> FindAllPossibleRoutePaths(IList<RouteEntity> allRoutes)
        {
            var originPlanet = Planet.From(_engineRouteConfiguration.departure);
            var possibleRoutesByOrigin = allRoutes.GroupBy(m => m.Origin).ToDictionary(m => m.Key, m => m.ToList());
            var nodeRoutePaths = possibleRoutesByOrigin[originPlanet].Select(m => new List<RouteEntity> { m }).ToList();

            var possibleRoutePaths = SeekRoutePath(nodeRoutePaths, ref possibleRoutesByOrigin);

            return possibleRoutePaths.Cast<IList<RouteEntity>>().ToList();
        }

        /// <summary>
        /// Recursive route path look up
        /// </summary>
        /// <param name="parentRoutePaths">Current path node parent possible route paths (can be more than one)</param>
        /// <param name="possibleRoutesByOrigin">Global dictionary of routes by origin</param>
        /// <returns>Returns current node route path</returns>
        private List<List<RouteEntity>> SeekRoutePath(List<List<RouteEntity>> parentRoutePaths, ref Dictionary<string, List<RouteEntity>> possibleRoutesByOrigin)
        {
            var routePaths = new List<List<RouteEntity>>();

            foreach (var parentRoutePath in parentRoutePaths)
            {
                var origin = parentRoutePath.Last().Destination;

                if (possibleRoutesByOrigin.TryGetValue(origin, out var routes))
                {
                    var nodeRoutePaths = routes.Select(m => new List<RouteEntity>(parentRoutePath) { m }).ToList();

                    routePaths.AddRange(SeekRoutePath(nodeRoutePaths, ref possibleRoutesByOrigin));
                    continue;
                }

                routePaths.Add(parentRoutePath);
            }

            return routePaths;
        }

        #region -> Service classes /////////////////////////////////////////////////////////////////////////////////

        private class TravelComputingReport : ITravelComputingReport
        {
            public bool Doable { get; set; }
            public int MinTravelTime { get; set; }
            public int MaxTravelTime { get; set; }
            public int BestTravelTime { get; set; }
            public int BestOdds { get; set; }
            public IList<ITravelEvaluation>? BestEvaluations { get; set; }
            public IList<ITravelEvaluation>? OtherPossibleEvaluations { get; set; }
            public IList<ITravelEvaluation>? ImpossibleEvaluations { get; set; }
        }

        private class TravelEvaluation : ITravelEvaluation
        {
            private int _possibleCaptures = 0;
            private readonly IList<ITravelPosition> _travelPositions = new List<ITravelPosition>();
            private readonly EmpirePlan _empirePlan;

            public TravelEvaluation(EmpirePlan empirePlan, int day, Planet planet)
            {
                _empirePlan = empirePlan;
                this.AddPosition(day, planet);
            }

            public void AddPosition(int day, Planet planet, Core.TravelPosition type = Core.TravelPosition.Stepping)
            {
                var bountyHunters = _empirePlan.bounty_hunters.Any(m => m.planet.Equals(planet) && m.day == day); ;
                var position = new TravelPosition(day, planet, bountyHunters, type);

                _possibleCaptures += (position.BountyHunters && position.Refuel)
                    ? 1
                    : (position.BountyHunters ? 1 : 0);

                _travelPositions.Add(position);
            }

            public int TravelDays => _travelPositions.Last()?.Day ?? 0;

            public IList<ITravelPosition> TravelPositions => _travelPositions;

            public int Odds
            {
                get
                {
                    var total = 0m;
                    for (var k = 0; k < _possibleCaptures; k++)
                    {
                        if (k == 0)
                            total += 1m / 10m;
                        else
                            total += Convert.ToDecimal(Math.Pow(9, k)) / Convert.ToDecimal(Math.Pow(10, k + 1));
                    }

                    return TravelDays <= _empirePlan.countdown ? Convert.ToInt16((1 - total) * 100) : 0;
                }
            }
        }

        private class TravelPosition : ITravelPosition
        {
            public TravelPosition(int day, Planet planet, bool bountyHunters, Core.TravelPosition type)
            {
                Day = day;
                Planet = planet;
                Refuel = type == Core.TravelPosition.Refuel;
                BountyHunters = bountyHunters;
                Waiting = type == Core.TravelPosition.Waiting;
            }

            public int Day { get; private set; }

            public Planet Planet { get; private set; }

            public bool BountyHunters { get; private set; }

            public bool Refuel { get; private set; }

            public bool Waiting { get; private set; }
        }

        private class TravelWaitingStep
        {
            public TravelWaitingStep(string planet, int day)
            {
                Day = day;
                Planet = Planet.From(planet);
            }
            public int Day { get; private set; }

            public Planet Planet { get; private set; }
        }
        #endregion
    }
}