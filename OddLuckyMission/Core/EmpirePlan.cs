using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ValueObjects;

namespace Core
{
    public class EmpirePlan
    {
        public int countdown { get; set; }

        public List<BountyHunterDestinationPlan> bounty_hunters { get; set; } = new List<BountyHunterDestinationPlan>();
    }

    public class BountyHunterDestinationPlan
    {
        public string planet { get; set; }

        public int day { get; set; }
    }
}
