using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Products;

namespace Products.Data
{
    public class ProductsContext : DbContext
    {
        public ProductsContext (DbContextOptions<ProductsContext> options)
            : base(options)
        {
        }

        public DbSet<Products.Product> Product { get; set; } = default!;
    }
}
