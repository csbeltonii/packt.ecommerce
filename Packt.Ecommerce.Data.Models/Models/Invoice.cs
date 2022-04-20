using System.Collections.Generic;
using Newtonsoft.Json;

namespace Packt.Ecommerce.Data.Models.Models
{
    public class Invoice
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string PaymentMode { get; set; }
        public Address ShippingAddress { get; set; }
        public SoldBy SoldBy { get; set; }
        public List<Product> Products { get; set; }
        public string Etag { get; set; }
    }
}
