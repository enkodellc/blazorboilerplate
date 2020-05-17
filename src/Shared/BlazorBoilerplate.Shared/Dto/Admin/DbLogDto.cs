using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.Admin
{
    public class DbLogDto
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Exception { get; set; }

        public string Properties { get; set; }

        public IDictionary<string, string> LogProperties { get; set; }
    }
}
