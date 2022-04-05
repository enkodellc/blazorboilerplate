using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    public partial class DbLog
    {
        [Key]
        public int Id { get; set; }

        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Exception { get; set; }

        public string Properties { get; set; }
    }
}
