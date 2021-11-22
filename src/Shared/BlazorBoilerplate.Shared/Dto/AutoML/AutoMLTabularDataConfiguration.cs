using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class AutoMLTabularDataConfiguration : IAutoMLConfiguration
    {
        public string Target { get; set; }
        public Dictionary<string, string> Feartures { get; set; }
        public string GetSummary()
        {
            throw new NotImplementedException();
        }
    }
}
