namespace Sample.Test
{
  
    public class ProductService(string serviceUrl) :  IProductService
    {
        // This would be a mock in-memory list acting as a simple database for demonstration
        private readonly List<Product?> _productList = new List<Product?>();

        // Get a product by its ID
        public Product? GetProductById(int id)
        {
            return _productList.Find(p => p.Id == id);
        }

        // Get a list of all products
        public List<Product?> GetAllProducts()
        {
            return _productList;
        }

        // Add a new product
        public async Task AddProduct(Product? product)
        {
           await  Task.Delay(TimeSpan.FromSeconds(10));

           
            _productList.Add(product);
        }

        // Update an existing product
        public void UpdateProduct(Product product)
        {
            var existingProduct = GetProductById(product.Id);
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.Description = product.Description;
        }

        // Delete a product by its ID
        public void DeleteProduct(int id)
        {
            var product = GetProductById(id);
            _productList.Remove(product);
        }

        public void GetTest()
        {
            
        }
    }
}