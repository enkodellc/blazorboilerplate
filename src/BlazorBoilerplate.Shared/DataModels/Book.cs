using BlazorBoilerplate.Shared.DataInterfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Shared.DataModels
{
    public class Book : ITenant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [NotMapped]
        public string BookStoreTitle { get; set; }
    }
}