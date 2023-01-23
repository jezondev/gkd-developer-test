namespace Core.Exceptions;

public class UnExistingPlanetException : Exception
{
    public UnExistingPlanetException(string name)
        : base($"Planet \"{name}\" does not exist")
    {
    }
}
