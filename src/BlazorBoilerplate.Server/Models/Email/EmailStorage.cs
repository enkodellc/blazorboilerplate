using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.SqlServer;

namespace BlazorBoilerplate.Server.Models
{
    public class ETemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public int Version { get; set; }
        public string Body { get; set; }


        public List<ETemplate_Field> ETemplate_Fields { get; set; }

    }

    public class ETemplate_Field
    {
        public int Id { get; set; }
        public int ETemplateId { get; set; }
        public string Field_Name { get; set; }

        public List<EField_Entry> Field_Entries { get; set; }
    }

    public class EField_Entry
    {
        public int Id { get; set; }
        public int SentEmailId { get; set; }
        public int ETemplate_FieldId { get; set; }
        public string Field_Content { get; set; }

    }

    public class SentEmail
    {
        public int Id { get; set; }
        public string ToAddresses { get; set; }
        public string CCAddresses { get; set; }
        public string BccAddresses { get; set; }
        public string Subject { get; set; }
        public DateTime TimeStamp_UTC { get; set; }

        public List<EField_Entry> Sent_Field_Entry { get; set; }

    }
}
