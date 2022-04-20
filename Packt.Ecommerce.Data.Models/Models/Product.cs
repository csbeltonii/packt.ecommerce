using System.Collections.Generic;
using Newtonsoft.Json;

namespace Packt.Ecommerce.Data.Models.Models
{
    public class Product
    {
        [JsonProperty(PropertyName ="id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<Rating> Rating { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string CreatedDate { get; set; }
        public List<string> Format { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Size { get; set; }
        public List<string> Color { get; set; }
        public string Etag { get; set; }
    }
}
