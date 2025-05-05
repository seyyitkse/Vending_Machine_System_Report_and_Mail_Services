using System.Text.Json.Serialization;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        [JsonIgnore] // Döngüyü kırmak için ekledik

        public ICollection<Product> Products { get; set; }

        public Category()
        {
            Products = new HashSet<Product>();
        }
    }
}
