using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public long UserCode { get; set; }

        [ForeignKey("UserCode")]
        public AppUser AppUser { get; set; }

        [Required]
        [JsonIgnore]
        public int VendId { get; set; }
        public Vend Vend { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } // Yeni eklenen alan
        [Required]
        public decimal TotalPrice { get; set; } // Siparişin toplam tutarı
        public bool IsInvoiceSent { get; set; } // Yeni alan
    }
}
