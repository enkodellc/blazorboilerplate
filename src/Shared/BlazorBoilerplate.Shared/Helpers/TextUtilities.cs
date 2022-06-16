using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Helpers
{
    public static class TextUtilities
    {
        public static string ToYesNo(bool? value, IStringLocalizer<object> L)
        {
            return value == true ? L["Yes"].ToString() : value == false ? L["No"].ToString() : string.Empty;
        }
    }
}
