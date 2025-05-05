using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [JsonIgnore] // Döngüyü kırmak için ekledik
        public Category? Category { get; set; }

        [Required]
        public int BrandId { get; set; }

        [JsonIgnore] // Döngüyü kırmak için ekledik
        public Brand? Brand { get; set; }

        [JsonIgnore] // Döngüyü kırmak için ekledik
        public ICollection<Order> Orders { get; set; }

        public bool IsCriticalStock { get; set; }
        [JsonIgnore]
        public ICollection<VendProduct> VendProducts { get; set; } // Vend ile ilişki
    }
}
