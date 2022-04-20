namespace Packt.Ecommerce.Data.Models.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Address Address { get; set; }
        public int PhoneNumber { get; set; }
        public string Etag { get; set; }
    }
}
