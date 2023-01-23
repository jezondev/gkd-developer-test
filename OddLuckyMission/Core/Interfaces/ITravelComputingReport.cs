using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITravelComputingReport
    {
        bool Doable { get; }

        int MinTravelTime { get; }
        int MaxTravelTime { get; }
        int BestTravelTime { get; }

        int BestOdds { get; }

        IList<ITravelEvaluation> BestEvaluations { get; }
        
        IList<ITravelEvaluation>? OtherPossibleEvaluations{ get; }

        IList<ITravelEvaluation>? ImpossibleEvaluations{ get; }
    }
}
