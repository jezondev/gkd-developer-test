using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class RouteEntity
    {
        public string Origin { get; set; }

        public string Destination { get; set; }

        public int TravelTime { get; set; }
    }
}
