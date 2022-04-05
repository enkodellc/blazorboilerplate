using System.IO;
using System.Linq;
namespace BlazorBoilerplate.SourceGenerator
{
    public static class CrossPlatform
    {
        public static string PathCombine(string basePath, params string[] additional)
        {
            var splits = additional.Select(s => s.Split(pathSplitCharacters)).ToArray();
            var totalLength = splits.Sum(arr => arr.Length);
            var segments = new string[totalLength + 1];
            segments[0] = basePath;
            var i = 0;
            foreach (var split in splits)
            {
                foreach (var value in split)
                {
                    i++;
                    segments[i] = value;
                }
            }
            return Path.Combine(segments);
        }

        static char[] pathSplitCharacters = new char[] { '/', '\\' };
    }
}