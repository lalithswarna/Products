using System.ComponentModel.DataAnnotations;

namespace Products
{
    public class Product
    {
        public int ID { get; set; }
        public string? Description { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}