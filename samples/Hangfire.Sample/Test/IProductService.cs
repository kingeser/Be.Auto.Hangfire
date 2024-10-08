﻿namespace Sample.Test
{
    public interface IProductService
    {
        Product? GetProductById(int id);
        List<Product?> GetAllProducts();
        void AddProduct(Product? product);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
    }
}