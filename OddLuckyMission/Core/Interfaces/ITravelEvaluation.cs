using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITravelEvaluation
    {
        int TravelDays { get; }

        int Odds { get; }

        IList<ITravelPosition> TravelPositions { get; }
    }
}
