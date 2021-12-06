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
        public AutoMLTabularDataConfiguration Configuration { get; set; }
        public List<String> RequiredAutoMLs { get; set; }
        public AutoMLRuntimeConstraints RuntimeConstraints { get; set; }
        public StartAutoMLRequestDto()
        {

        }
    }
}
