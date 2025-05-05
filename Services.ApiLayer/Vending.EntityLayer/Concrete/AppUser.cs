using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Services.ApiLayer.Vending.EntityLayer.Concrete
{
    public class AppUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public long UserCode { get; set; }
        public int? DepartmentID { get; set; }
        public Department? Department { get; set; }
        [JsonIgnore] // Döngüyü kırmak için ekledik
        public ICollection<Order> Orders { get; set; } // Add Orders navigation property

        public decimal MonthlyLimit { get; set; }  // Örneğin: 500₺
        public decimal CurrentBalance { get; set; } // Örneğin: 200₺ kaldı
        public DateTime LastResetDate { get; set; } // En son ne zaman sıfırlandı?
        public bool IsAdmin { get; set; } // Admin kullanıcıları belirlemek için
    }
}
