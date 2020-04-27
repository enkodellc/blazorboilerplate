using BlazorBoilerplate.Localization;
using System;
using System.Globalization;

namespace BlazorBoilerplate.Infrastrcture.Extensions
{
    public static class StringEnumExtension
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(LocalizedDescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                            description = ((LocalizedDescriptionAttribute)descriptionAttributes[0]).Description;

                        break;
                    }
                }
            }

            return description;
        }
    }
}
