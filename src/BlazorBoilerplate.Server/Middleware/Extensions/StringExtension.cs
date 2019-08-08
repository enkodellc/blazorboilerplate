using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazorBoilerplate.Server.Middleware.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidJson(this string text)
        {
            text = text.Trim();
            if ((text.StartsWith("{") && text.EndsWith("}")) || //For object
                (text.StartsWith("[") && text.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(text);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
