using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace TestCore.ViewModels
{
    // A simplified version of the GroceryListItemsViewModel for testing the search functionality
    public class TestViewModel
    {
        private readonly IProductService _productService;
        
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];
        
        public string MyMessage { get; set; } = string.Empty;
        
        public string ProductSearchText { get; set; } = string.Empty;

        public TestViewModel(IProductService productService)
        {
            _productService = productService;
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            foreach (Product p in _productService.GetAll())
            {
                AvailableProducts.Add(p);
            }
        }

        public void SearchProducts(string searchParameter)
        {
            var q = (searchParameter ?? ProductSearchText)?.Trim();

            // if the searchbar is empty, return the full list.
            if (string.IsNullOrWhiteSpace(q))
            {
                GetAvailableProducts();
                return;
            }

            // copy the original AvailableList.
            ObservableCollection<Product> products = [.. AvailableProducts];

            AvailableProducts.Clear();
            foreach (var product in products)
            {
                // Hier wordt ook gecheckt of de searchQuery zit in de string.
                if (!string.IsNullOrEmpty(product.Name) && product.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    AvailableProducts.Add(product);
                }
            }
            if (AvailableProducts.Count == 0 && !string.IsNullOrWhiteSpace(q))
            {
                MyMessage = $"Geen producten gevonden met \"{q}\"";
            }
        }
    }
}