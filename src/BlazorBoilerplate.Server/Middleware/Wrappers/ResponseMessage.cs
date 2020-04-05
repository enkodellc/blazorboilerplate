using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlazorBoilerplate.Server.Middleware.Wrappers
{
    public static class ResponseMessage
    {
        private static Regex regEx = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) |
(?<=[^A-Z])(?=[A-Z]) |
(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
        public static string GetDescription(int httpStatusCode)
        {
            foreach (var field in typeof(StatusCodes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(f => f.FieldType == typeof(int)))
            {
                if (httpStatusCode == (int)field.GetValue(null))
                {
                    return regEx.Replace(field.Name, " ");
                }
            }

            return "Unknown error";
        }
    }
}
