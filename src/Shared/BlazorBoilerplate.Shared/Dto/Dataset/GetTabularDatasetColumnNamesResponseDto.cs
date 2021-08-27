using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.Dataset
{
    public class GetTabularDatasetColumnNamesResponseDto
    {
        public List<string> ColumnNames { get; set; }
        public GetTabularDatasetColumnNamesResponseDto()
        {
            ColumnNames = new List<string>();
        }
    }
}
