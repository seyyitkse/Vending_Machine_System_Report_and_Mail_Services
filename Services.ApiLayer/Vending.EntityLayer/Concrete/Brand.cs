using System.Text.Json.Serialization;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class Brand
    {
        public int BrandId { get; set; }
        public string Name { get; set; }
        // Navigation property
        [JsonIgnore] // Döngüyü kýrmak için ekledik

        public ICollection<Product> Products { get; set; }
        public Brand()
        {
            Products = new HashSet<Product>();
        }
    }
}
