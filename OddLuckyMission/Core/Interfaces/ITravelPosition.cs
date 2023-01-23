using Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITravelPosition
    {
        int Day { get; }

        Planet Planet { get; }

        bool BountyHunters { get; }

        bool Refuel { get; }

        bool Waiting { get; }
    }
}
