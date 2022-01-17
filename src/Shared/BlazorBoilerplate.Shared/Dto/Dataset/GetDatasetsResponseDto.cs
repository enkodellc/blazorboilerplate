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
        public int Columns { get; set; }
        public int Rows { get; set; }
        public DateTime Creation_date { get; set; }
        public GetDatasetsResponseDto(string name, string type,int columns,int rows, DateTime time)
        {
            Name = name;
            Type = type;
            Columns = columns;
            Rows = rows;
            Creation_date = time;
        }
    }
}
