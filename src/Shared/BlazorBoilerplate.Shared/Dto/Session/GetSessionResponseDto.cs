using BlazorBoilerplate.Shared.Dto.AutoML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.Session
{
    public class GetSessionResponseDto
    {
        public int Status { get; set; }
        public List<AutoMLStatusDto> AutoMls { get; set; }
        public string Dataset { get; set; }
        public BlazorBoilerplate.Server.MachineLearningTask Task { get; set; }
        public AutoMLTabularDataConfiguration Configuration { get; set; }
        public List<String> RequiredMlLibraries { get; set; }
        public List<String> RequiredAutoMLs { get; set; }
        public AutoMLRuntimeConstraints RuntimeConstraints { get; set; }

        public GetSessionResponseDto()
        {
            AutoMls = new List<AutoMLStatusDto>();
            RequiredMlLibraries = new List<String>();
            RequiredAutoMLs = new List<String>();
        }
    }
}
