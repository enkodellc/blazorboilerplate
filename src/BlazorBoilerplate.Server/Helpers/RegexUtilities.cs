using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Helpers
{
    public static class RegexUtilities
    {
        public static Regex StringInterpolationHelper = new Regex("(?<=[\\ \"]{0,1}{)(.+?)(?=})(?:[\\ \\.\"]{0,2}|$)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        internal static Regex DirtyXMLParserRegex = new Regex("(?<=<property key=\')(?<key>[a-zA-Z0-9\\- :]+?)(?:'>)(?<value>[a-zA-Z0-9. \\/-:]+?)(?=<\\/property>)", RegexOptions.Compiled);

        public static Dictionary<string, string> DirtyXMLParser(string input)
        {
            var propertyList = new Dictionary<string, string>();
            var xmlMatch = DirtyXMLParserRegex.Match(input);
            while (xmlMatch.Success)
            {
                propertyList.Add(xmlMatch.Groups["key"].Value, xmlMatch.Groups["value"].Value);
                xmlMatch = xmlMatch.NextMatch();
            }
            return propertyList;
        }
    }
}
