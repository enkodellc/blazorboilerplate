using BlazorBoilerplate.Shared.Helpers;
using System.Collections.Generic;

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
