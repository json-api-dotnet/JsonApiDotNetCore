using System.Globalization;

namespace TestBuildingBlocks;

public static class HalfExtensions
{
    public static float AsFloat(this Half half)
    {
        // Caution: Simply casting to float returns a higher precision (more digits).
        return float.Parse(half.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
