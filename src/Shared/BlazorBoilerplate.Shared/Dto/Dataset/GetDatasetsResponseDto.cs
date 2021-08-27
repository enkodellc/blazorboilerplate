using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.Dataset
{
    public class GetDatasetsResponseDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public GetDatasetsResponseDto(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
