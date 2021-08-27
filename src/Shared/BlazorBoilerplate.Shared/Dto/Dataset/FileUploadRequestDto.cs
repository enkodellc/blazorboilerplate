using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Dto.Dataset
{
    public class FileUploadRequestDto
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public FileUploadRequestDto(string filename, string content)
        {
            FileName = filename;
            Content = content;
        }
    }
}
