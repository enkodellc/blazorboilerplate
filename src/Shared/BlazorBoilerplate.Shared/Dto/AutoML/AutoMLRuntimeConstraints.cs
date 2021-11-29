using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class AutoMLRuntimeConstraints
    {
        public int Runtime_limit { get; set; }
        public int Max_iter { get; set; }
    }
}
