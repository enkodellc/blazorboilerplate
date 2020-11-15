using Humanizer;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace BlazorBoilerplate.Infrastructure.Server.Models
{
    public static class ResponseMessage
    {
        public static string GetDescription(int httpStatusCode)
        {
            foreach (var field in typeof(StatusCodes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(f => f.FieldType == typeof(int)))
            {
                if (httpStatusCode == (int)field.GetValue(null))
                {
                    return field.Name.Humanize(LetterCasing.Title);
                }
            }

            return "Unknown error";
        }
    }
}
