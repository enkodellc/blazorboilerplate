using BlazorBoilerplate.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.Dataset
{
    public class GetDatasetResponseDto
    {
        public string Name { get; set; }
        public Server.DataType Type { get; set; }
        public List<string> FirstEntries { get; set; }
    }
}
