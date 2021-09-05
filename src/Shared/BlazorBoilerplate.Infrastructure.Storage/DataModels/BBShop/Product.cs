using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    public partial class Product : IAuditable, ISoftDelete
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "The product title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The product desc is required")]
        public string Description { get; set; }

        public string Image { get; set; }

        public long CategoryId { get; set; }

        public long ViewCount { get; set; }
    }
}
