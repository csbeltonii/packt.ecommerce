using System;
using Newtonsoft.Json;

#nullable disable

namespace Packt.Ecommerce.DTO.Models
{
    public class ProductListViewModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Name { get; set; }
        public int Price { get; set; }
        public Uri ImageUrl { get; set; }
        public double AverageRating { get; set; }

    }
}
