namespace Sample.Library
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }

        // Constructor for initializing Product
        public Product(int id, string name, decimal price, int stock, string description)
        {
            Id = id;
            Name = name;
            Price = price;
            Stock = stock;
            Description = description;
        }
    }
}