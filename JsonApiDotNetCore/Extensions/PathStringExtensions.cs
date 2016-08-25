using System.Linq;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Extensions
{
  public static class PathStringExtensions
  {
    public static string ExtractFirstSegment(this PathString path, out PathString remainingSegments)
    {
      remainingSegments = new PathString();

      if (!path.HasValue) return string.Empty;

      var splitPath = SplitPath(path);
      remainingSegments = new PathString(RemoveFirstSegmentFromPath(splitPath));
      return splitPath[0];
    }

    private static string[] SplitPath(PathString pathString)
    {
      return pathString.ToString().TrimStart('/').Split('/');;
    }

    private static string RemoveFirstSegmentFromPath(string[] pathArray)
    {
      return string.Join("/", pathArray.Skip(1).ToArray());
    }
  }
}
