using Azure.Data.Tables;
using ABCRetail.Models;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly TableClient _customerTableClient;
        private readonly TableClient _productTableClient;

        public TableStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _customerTableClient =
                new TableClient(connectionString, "Customers");

            _productTableClient =
                new TableClient(connectionString, "Products");
        }

        // -------------------------
        // Customers
        // -------------------------

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var customers = new List<Customer>();

            await foreach (Customer customer in
                _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }

            return customers;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _customerTableClient.AddEntityAsync(customer);
        }

        // -------------------------
        // Products
        // -------------------------

        public async Task<List<Product>> GetProductsAsync()
        {
            var products = new List<Product>();

            await foreach (Product product in
                _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }

            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            await _productTableClient.AddEntityAsync(product);
        }
    }
}