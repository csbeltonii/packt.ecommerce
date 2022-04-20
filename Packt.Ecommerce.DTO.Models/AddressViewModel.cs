using System.ComponentModel.DataAnnotations;

namespace Packt.Ecommerce.DTO.Models
{
    public class AddressViewModel
    {
        public string Address1 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
