using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class GetAutoMlModelRequestDto
    {
        public string SessionId { get; set; }
        public string AutoMl { get; set; }
    }
}
