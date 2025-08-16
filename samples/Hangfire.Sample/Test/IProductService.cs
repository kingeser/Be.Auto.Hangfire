namespace Sample.Test
{
    public interface IProductService
    {
        Product? GetProductById(int id);
        List<Product?> GetAllProducts();
        Task AddProduct(Product? product);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
    }
}