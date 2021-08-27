using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class AutoMLStatusDto
    {
        public string Name { get; set; }
        public int Status { get; set; }
        public List<string> Messages { get; set; }
        public AutoMLStatusDto()
        {
            Messages = new List<string>();
        }
    }
}
