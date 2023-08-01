using Microsoft.EntityFrameworkCore;
using Products.Data;
using Products.Interface;
using System.Net;
using System.Web.Http;

namespace Products.Interface
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductsContext _context;

        public ProductRepository(ProductsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _context.Product.ToListAsync();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _context.Product.FindAsync(id);
        }

        public async Task AddProduct(Product product)
        {
            _context.Product.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

}
