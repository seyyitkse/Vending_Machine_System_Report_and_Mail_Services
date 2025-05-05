using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class Vend
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationDescription { get; set; }

        [JsonIgnore] // Döngüyü kırmak için ekledik
        public ICollection<Order> Orders { get; set; }
        [JsonIgnore] // Döngüyü kırmak için ekledik
        public ICollection<VendProduct> VendProducts { get; set; } // Vend ile ilişki
    }
}
