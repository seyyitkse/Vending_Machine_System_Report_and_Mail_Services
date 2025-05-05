using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class VendProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VendId { get; set; }

        [ForeignKey("VendId")]
        public Vend Vend { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int Stock { get; set; } // Otomattaki ürün stoğu
    }
}