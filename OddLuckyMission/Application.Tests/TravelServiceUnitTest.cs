using Application.Services;
using Castle.Components.DictionaryAdapter;
using Core;
using Core.Entities;
using Core.Interfaces;
using Core.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests
{
    public class Tests
    {
        private readonly Mock<IUniverseRepository> _universeRepositoryMock = new();
        private readonly EngineRouteConfiguration _engineRouteConfiguration = new EngineRouteConfiguration
        {
            departure = Planet.Tatooine,
            arrival = Planet.Endor,
            autonomy = 6
        };

        [SetUp]
        public void Setup()
        {
            _universeRepositoryMock.Setup(m => m.ListAsync(CancellationToken.None)).ReturnsAsync(new List<RouteEntity>
            {
                new() { Origin = Planet.Tatooine,  Destination = Planet.Dagobah, TravelTime = 6 },
                new() { Origin = Planet.Dagobah,  Destination = Planet.Endor, TravelTime = 4 },
                new() { Origin = Planet.Dagobah,  Destination = Planet.Hoth, TravelTime = 1 },
                new() { Origin = Planet.Hoth,  Destination = Planet.Endor, TravelTime = 1 },
                new() { Origin = Planet.Tatooine,  Destination = Planet.Hoth, TravelTime = 6 },
            });
        }


        [TearDown]
        public void DeInitialize()
        {
            Console.WriteLine("Inside TearDown");
        }

        [Test]
        public async Task Example1()
        {
            var service = new TravelService(_universeRepositoryMock.Object, _engineRouteConfiguration);

            Func<Task<ITravelComputingReport>> func = async () => await service.EvaluateTravelOddsAsync(new EmpirePlan
            {
                countdown = 7,
                bounty_hunters = new EditableList<BountyHunterDestinationPlan>
                {
                    new() { planet = Planet.Hoth, day = 6 },
                    new() { planet = Planet.Hoth, day = 7 },
                    new() { planet = Planet.Hoth, day = 8 },
                }
            });

            var result = await func();

            result.Doable.Should().BeFalse();
            result.MinTravelTime.Should().Be(8);
            result.BestOdds.Should().Be(0);

            result.BestEvaluations.Should().BeNull();
            result.OtherPossibleEvaluations.Should().BeNull();
            result.ImpossibleEvaluations.Should().HaveCount(3);
        }

        [Test]
        public async Task Example2()
        {
            var service = new TravelService(_universeRepositoryMock.Object, _engineRouteConfiguration);
            var engineConfiguration = new EngineRouteConfiguration
            {
                departure = Planet.Tatooine,
                arrival = Planet.Endor,
                autonomy = 6
            };

            Func<Task<ITravelComputingReport>> func = async () => await service.EvaluateTravelOddsAsync(new EmpirePlan
            {
                countdown = 8,
                bounty_hunters = new EditableList<BountyHunterDestinationPlan>
                {
                    new() { planet = Planet.Hoth, day = 6 },
                    new() { planet = Planet.Hoth, day = 7 },
                    new() { planet = Planet.Hoth, day = 8 },
                }
            });

            var result = await func();

            result.Doable.Should().BeTrue();
            result.BestTravelTime.Should().Be(8);
            result.BestOdds.Should().Be(81);

            result.BestEvaluations.Should().HaveCount(1);

            // -> ONLY BEST VALID EVALUATION
            ///////////////////////////////////////////////////////////////////
            result.BestEvaluations[0].TravelPositions.Should().HaveCount(4);
            // -> departure from tatooine
            result.BestEvaluations[0].TravelPositions[0].Planet.Should().Be(Planet.Tatooine);
            result.BestEvaluations[0].TravelPositions[0].Day.Should().Be(0);
            // -> Travel from Tatooine to Hoth, with 10% chance of being captured on day 6 on Hoth
            result.BestEvaluations[0].TravelPositions[1].Planet.Should().Be(Planet.Hoth);
            result.BestEvaluations[0].TravelPositions[1].BountyHunters.Should().Be(true);
            result.BestEvaluations[0].TravelPositions[1].Day.Should().Be(6);
            // -> Refuel on Hoth with 10 % chance of being captured on day 7 on Hoth.
            result.BestEvaluations[0].TravelPositions[2].Planet.Should().Be(Planet.Hoth);
            result.BestEvaluations[0].TravelPositions[2].BountyHunters.Should().Be(true);
            result.BestEvaluations[0].TravelPositions[2].Refuel.Should().Be(true);
            result.BestEvaluations[0].TravelPositions[2].Day.Should().Be(7);
            // -> Travel from Hoth to Endor
            result.BestEvaluations[0].TravelPositions[3].Planet.Should().Be(Planet.Endor);

            // -> ALTERNATIVE POSSIBLE EVALUATED ROUTE PATHS
            ///////////////////////////////////////////////////////////////////
            result.OtherPossibleEvaluations.Should().HaveCount(0);

            // -> NO ODD EVALUATED ROUTE PATHS
            ///////////////////////////////////////////////////////////////////
            result.ImpossibleEvaluations.Should().HaveCount(2);
        }
        
        [Test]
        public async Task Example3()
        {
            var service = new TravelService(_universeRepositoryMock.Object, _engineRouteConfiguration);
            var engineConfiguration = new EngineRouteConfiguration
            {
                departure = Planet.Tatooine,
                arrival = Planet.Endor,
                autonomy = 6
            };

            Func<Task<ITravelComputingReport>> func = async () => await service.EvaluateTravelOddsAsync(new EmpirePlan
            {
                countdown = 9,
                bounty_hunters = new EditableList<BountyHunterDestinationPlan>
                {
                    new() { planet = Planet.Hoth, day = 6 },
                    new() { planet = Planet.Hoth, day = 7 },
                    new() { planet = Planet.Hoth, day = 8 },
                }
            });
            var result = await func();

            result.Doable.Should().BeTrue();
            result.BestTravelTime.Should().Be(9);
            result.BestOdds.Should().Be(90);

            result.BestEvaluations.Should().HaveCount(1);

            // -> ONLY BEST VALID EVALUATION
            ///////////////////////////////////////////////////////////////////
            result.BestEvaluations[0].TravelPositions.Should().HaveCount(5);
            // -> departure from tatooine
            result.BestEvaluations[0].TravelPositions[0].Planet.Should().Be(Planet.Tatooine);
            result.BestEvaluations[0].TravelPositions[0].Day.Should().Be(0);
            // -> Travel from Tatooine to Dagobath
            result.BestEvaluations[0].TravelPositions[1].Planet.Should().Be(Planet.Dagobah);
            result.BestEvaluations[0].TravelPositions[1].Day.Should().Be(6);
            // -> Refuel on Dagobah
            result.BestEvaluations[0].TravelPositions[2].Planet.Should().Be(Planet.Dagobah);
            result.BestEvaluations[0].TravelPositions[2].Refuel.Should().Be(true);
            result.BestEvaluations[0].TravelPositions[2].Day.Should().Be(7);
            // -> Travel from Dagobah to Hoth, with 10% chance of being captured on day 8 on Hoth.
            result.BestEvaluations[0].TravelPositions[3].Planet.Should().Be(Planet.Hoth);
            result.BestEvaluations[0].TravelPositions[3].BountyHunters.Should().Be(true);
            result.BestEvaluations[0].TravelPositions[3].Day.Should().Be(8);
            // -> Travel from Hoth to Endor
            result.BestEvaluations[0].TravelPositions[4].Planet.Should().Be(Planet.Endor);
            result.BestEvaluations[0].TravelPositions[4].Day.Should().Be(9);

            // -> ALTERNATIVE POSSIBLE EVALUATED ROUTE PATHS
            ///////////////////////////////////////////////////////////////////
            result.OtherPossibleEvaluations.Should().HaveCount(4);
            result.OtherPossibleEvaluations[0].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[1].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[2].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[3].Odds.Should().Be(73);

            // -> NO ODD EVALUATED ROUTE PATHS
            ///////////////////////////////////////////////////////////////////
            result.ImpossibleEvaluations.Should().HaveCount(1);
        }
        
        [Test]
        public async Task Example4()
        {
            var service = new TravelService(_universeRepositoryMock.Object, _engineRouteConfiguration);
            var engineConfiguration = new EngineRouteConfiguration
            {
                departure = Planet.Tatooine,
                arrival = Planet.Endor,
                autonomy = 6
            };

            Func<Task<ITravelComputingReport>> func = async () => await service.EvaluateTravelOddsAsync(new EmpirePlan
            {
                countdown = 10,
                bounty_hunters = new EditableList<BountyHunterDestinationPlan>
                {
                    new() { planet = Planet.Hoth, day = 6 },
                    new() { planet = Planet.Hoth, day = 7 },
                    new() { planet = Planet.Hoth, day = 8 },
                }
            });
            var result = await func();

            result.Doable.Should().BeTrue();
            result.BestTravelTime.Should().Be(10);
            result.BestOdds.Should().Be(100);

            result.BestEvaluations.Should().HaveCount(2);

            // -> FIRST BEST VALID EVALUATION
            ///////////////////////////////////////////////////////////////////
            var evaluation = result.BestEvaluations[0];
            evaluation.TravelPositions.Should().HaveCount(6);
            // -> departure from tatooine
            evaluation.TravelPositions[0].Planet.Should().Be(Planet.Tatooine);
            evaluation.TravelPositions[0].Day.Should().Be(0);
            // -> Wait for 1 day on Tatooine
            evaluation.TravelPositions[1].Planet.Should().Be(Planet.Tatooine);
            evaluation.TravelPositions[1].Waiting.Should().Be(true);
            evaluation.TravelPositions[1].Day.Should().Be(1);
            // -> Travel from Tatooine to Dagobah
            evaluation.TravelPositions[2].Planet.Should().Be(Planet.Dagobah);
            evaluation.TravelPositions[2].Day.Should().Be(7);
            // -> refuel on Dagobah
            evaluation.TravelPositions[3].Planet.Should().Be(Planet.Dagobah);
            evaluation.TravelPositions[3].Refuel.Should().Be(true);
            evaluation.TravelPositions[3].Day.Should().Be(8);
            // -> Travel from Dagobah to Hoth
            evaluation.TravelPositions[4].Planet.Should().Be(Planet.Hoth);
            evaluation.TravelPositions[4].Day.Should().Be(9);
            // -> Travel from Hoth to Endor
            evaluation.TravelPositions[5].Planet.Should().Be(Planet.Endor);
            evaluation.TravelPositions[5].Day.Should().Be(10);

            // -> SECOND BEST VALID EVALUATION
            ///////////////////////////////////////////////////////////////////
            evaluation = result.BestEvaluations[1];
            evaluation.TravelPositions.Should().HaveCount(6);
            // -> departure from tatooine
            evaluation.TravelPositions[0].Planet.Should().Be(Planet.Tatooine);
            evaluation.TravelPositions[0].Day.Should().Be(0);
            // -> Travel from Tatooine to Dagobah
            evaluation.TravelPositions[1].Planet.Should().Be(Planet.Dagobah);
            evaluation.TravelPositions[1].Day.Should().Be(6);
            // -> refuel on Dagobah
            evaluation.TravelPositions[2].Planet.Should().Be(Planet.Dagobah);
            evaluation.TravelPositions[2].Refuel.Should().Be(true);
            evaluation.TravelPositions[2].Day.Should().Be(7);
            // -> Wait for 1 day on Dagobah
            evaluation.TravelPositions[3].Planet.Should().Be(Planet.Dagobah);
            evaluation.TravelPositions[3].Waiting.Should().Be(true);
            evaluation.TravelPositions[3].Day.Should().Be(8);
            // -> Travel from Dagobah to Hoth
            evaluation.TravelPositions[4].Planet.Should().Be(Planet.Hoth);
            evaluation.TravelPositions[4].Day.Should().Be(9);
            // -> Travel from Hoth to Endor
            evaluation.TravelPositions[5].Planet.Should().Be(Planet.Endor);
            evaluation.TravelPositions[5].Day.Should().Be(10);
            
            // -> ALTERNATIVE POSSIBLE EVALUATED ROUTE PATHS
            ///////////////////////////////////////////////////////////////////
            result.OtherPossibleEvaluations.Should().HaveCount(10);
            result.OtherPossibleEvaluations[0].Odds.Should().Be(90);
            result.OtherPossibleEvaluations[1].Odds.Should().Be(90);
            result.OtherPossibleEvaluations[2].Odds.Should().Be(90);
            result.OtherPossibleEvaluations[3].Odds.Should().Be(90);
            result.OtherPossibleEvaluations[4].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[5].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[6].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[7].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[8].Odds.Should().Be(81);
            result.OtherPossibleEvaluations[9].Odds.Should().Be(73);

            // -> NO ODD EVALUATED ROUTE PATHS
            ///////////////////////////////////////////////////////////////////
            result.ImpossibleEvaluations.Should().HaveCount(1);
        }
    }
}