using BlazorBoilerplate.Shared.Dto.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.AutoML
{
    public class StartAutoMLRequestDto
    {
        public string DatasetName { get; set; }
        public string DatasetType { get; set; }
        public string Task { get; set; }
        public object Configuration { get; set; }

        public int Time { get; set; }
        public StartAutoMLRequestDto()
        {

        }
    }
}
