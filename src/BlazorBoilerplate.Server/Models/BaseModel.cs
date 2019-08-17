using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Server.Models
{
    public class BaseModel
    {
        [Key]
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public Guid CreatedBy { get; set; }

        public Guid ModifiedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
