using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class AutoMLTarget
    {
        // TODO: new issue. duplicate attribute name, target.target ? consider to refactor it
        public string Target { get; set; }
        public BlazorBoilerplate.Server.DataType Type { get; set; }
    }
}
