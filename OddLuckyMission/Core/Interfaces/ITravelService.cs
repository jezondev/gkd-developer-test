namespace Core.Interfaces
{
    public interface ITravelService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="empirePlan"></param>
        /// <returns></returns>
        public Task<ITravelComputingReport> EvaluateTravelOddsAsync(EmpirePlan empirePlan);
    }
}
