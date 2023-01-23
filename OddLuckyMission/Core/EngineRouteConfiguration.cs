using Core.ValueObjects;

namespace Core
{
    public class EngineRouteConfiguration
    {
        public int autonomy { get; set; }
        public string departure { get; set; }
        public string arrival { get; set; }
        public string routes_db { get; set; }
    }
}