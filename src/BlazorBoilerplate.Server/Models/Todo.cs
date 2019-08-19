using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Models
{
    public class Todo : BaseModel
    {
        [Key]
        public new long Id { get; set; }

        [Required]
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}
