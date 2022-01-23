using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class TestAutoMLRequestDto
    {
        public string TestData { get; set; }
        public string SessionId { get; set; }
        public string AutoMlName { get; set; }
    }
}