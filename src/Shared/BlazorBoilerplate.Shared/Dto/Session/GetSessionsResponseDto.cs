using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.Session
{
    public class GetSessionsResponseDto
    {
        public List<string> SessionIds { get; set; }
        public GetSessionsResponseDto()
        {
            SessionIds = new List<string>();
        }
    }
}
