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
        public GetSessionResponseDto()
        {
            AutoMls = new List<AutoMLStatusDto>();
        }
    }
}
