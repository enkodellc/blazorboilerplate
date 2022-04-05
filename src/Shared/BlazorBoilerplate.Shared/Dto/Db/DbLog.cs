using BlazorBoilerplate.Shared.Helpers;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class DbLog
    {
        public IDictionary<string, string> LogProperties
        {
            get
            {
                return RegexUtilities.DirtyXmlPropertyParser(Properties);
            }
        }

    }
}
