using System.Collections.Generic;

namespace Hangfire.Sample.Library
{
    public class ProductService : IProductService
    {
        private readonly string _serviceUrl;

        public ProductService(string serviceUrl)
        {
            _serviceUrl = serviceUrl;
        }
        // This would be a mock in-memory list acting as a simple database for demonstration
        private readonly List<Product> _productList = new List<Product>();

        // Get a product by its ID
        public Product GetProductById(int id)
        {
            return _productList.Find(p => p.Id == id);
        }

        // Get a list of all products
        public List<Product> GetAllProducts()
        {
            return _productList;
        }

        // Add a new product
        public void AddProduct(Product product)
        {
            _productList.Add(product);
        }

        // Update an existing product
        public void UpdateProduct(Product product)
        {
            var existingProduct = GetProductById(product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Description = product.Description;
            }
        }

        // Delete a product by its ID
        public void DeleteProduct(int id)
        {
            var product = GetProductById(id);
            if (product != null)
            {
                _productList.Remove(product);
            }
        }
    }
}