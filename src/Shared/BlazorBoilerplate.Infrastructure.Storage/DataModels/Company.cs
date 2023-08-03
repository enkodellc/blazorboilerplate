using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Name { get; set; }

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

        [Required]
        [MaxLength(64)]
        public string Address { get; set; }

        [Required]
        [MaxLength(64)]
        public string City { get; set; }

        [MaxLength(64)]
        public string Province { get; set; }

        [Required]
        [MaxLength(12)]
        public string ZipCode { get; set; }

        [Required]
        [MaxLength(2)]
        public string CountryCode { get; set; }

        [Required]
        [MaxLength(32)]
        public string VatIn { get; set; } //https://en.wikipedia.org/wiki/VAT_identification_number

        [MaxLength(15)]
        public string PhoneNumber { get; set; }
    }
}
