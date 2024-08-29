namespace Sample.Test
{
    public class Product(int id, string name, decimal price, int stock, string description)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public decimal Price { get; set; } = price;
        public int Stock { get; set; } = stock;
        public string Description { get; set; } = description;

        // Constructor for initializing Product
    }
}