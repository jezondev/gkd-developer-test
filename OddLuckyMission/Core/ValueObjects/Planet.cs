using Core.Common;
using Core.Exceptions;

namespace Core.ValueObjects;

public class Planet : ValueObject
{
    static Planet()
    {
    }

    private Planet()
    {
    }

    private Planet(string name)
    {
        Name = name;
    }

    public static Planet From(string name)
    {
        var planet = new Planet { Name = name };

        if (!SupportedPlanets.Contains(planet))
        {
            throw new UnExistingPlanetException(name);
        }

        return planet;
    }

    public static Planet Tatooine => new(nameof(Tatooine));

    public static Planet Dagobah => new(nameof(Dagobah));

    public static Planet Endor => new(nameof(Endor));

    public static Planet Hoth => new(nameof(Hoth));

    public string Name { get; private set; }

    public static implicit operator string(Planet planet)
    {
        return planet.ToString();
    }

    public static explicit operator Planet(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Name;
    }

    protected static IEnumerable<Planet> SupportedPlanets
    {
        get
        {
            yield return Tatooine;
            yield return Dagobah;
            yield return Endor;
            yield return Hoth;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
