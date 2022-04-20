using System;

public class ProductListViewModel
{
    [JsonProperty(ProperyName = "id")] 
    public string Id { get; set; }

    public string Name { get; set; }
    public int Price { get; set; }
    public Uri ImageUrl { get; set; }
    public double AverageRating NAME { get; set; }
}
