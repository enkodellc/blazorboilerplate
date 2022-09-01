using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    public partial class Person : IAuditable
    {
        public ApplicationUser User { get; set; }

        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public Guid Id { get; set; }

        [MaxLength(64)]
        public string FirstName { get; set; }

        [MaxLength(64)]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [MaxLength(16)]
        public string TIN { get; set; } //https://en.wikipedia.org/wiki/Taxpayer_Identification_Number

        [MaxLength(16)]
        public string IdentityCard { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime? ExpirationDate { get; set; }
        public DateTime? ExpirationReminderSentOn { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }

        public bool? Confirmed { get; set; } //accepted terms and conditions

        public DateTime? DeleteDate { get; set; }

        [MaxLength(64)]
        public string Address { get; set; }

        [MaxLength(64)]
        public string City { get; set; }

        [MaxLength(64)]
        public string Province { get; set; }

        [MaxLength(12)]
        public string ZipCode { get; set; }

        [MaxLength(2)]
        public string CountryCode { get; set; }

        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [JsonIgnore]
        public Point Location { get; set; }

        [NotMapped]
        public double? Longitude
        {
            get => Location?.X;
            set
            {
                if (value != null)
                {
                    if (Location == null)
                        Location = Utils.CreatePoint(value.Value, 0);
                    else
                        Location.X = value.Value;
                }
            }
        }

        [NotMapped]
        public double? Latitude
        {
            get => Location?.Y;
            set
            {
                if (value != null)
                {
                    if (Location == null)
                        Location = Utils.CreatePoint(0, value.Value);
                    else
                        Location.Y = value.Value;
                }
            }
        }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public override string ToString()
        {
            return BirthDate.HasValue ? $"{LastName} {FirstName} ({BirthDate.Value.ToShortDateString()})" : $"{LastName} {FirstName}";
        }
    }
}
