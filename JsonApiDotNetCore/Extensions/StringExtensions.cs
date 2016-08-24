using System;
using System.Text;

namespace JsonApiDotNetCore.Extensions
{
  public static class StringExtensions
  {
    public static string ToCamelCase(this string str)
    {
      var splittedPhrase = str.Split(' ', '-', '.');
      var sb = new StringBuilder();
      foreach (var s in splittedPhrase)
      {
        var splittedPhraseChars = s.ToCharArray();
        if (splittedPhraseChars.Length > 0)
        {
          splittedPhraseChars[0] = new string(splittedPhraseChars[0], 1).ToUpper().ToCharArray()[0];
        }
        sb.Append(new string(splittedPhraseChars));
      }
      return sb.ToString();
    }
  }
}
