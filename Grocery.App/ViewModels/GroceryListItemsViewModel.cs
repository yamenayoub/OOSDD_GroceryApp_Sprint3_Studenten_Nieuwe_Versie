﻿using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        private readonly IFileSaverService _fileSaverService;
        
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];

        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);
        
        [ObservableProperty]
        string myMessage = "Er zijn geen producten meer om toe te voegen";

        [ObservableProperty]
        string productSearchText;

        [RelayCommand]
        void SearchProducts(string searchParameter)
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

        partial void OnProductSearchTextChanged(string searchParam)
        {
            SearchProducts(searchParam);
        }
        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService, IFileSaverService fileSaverService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            _fileSaverService = fileSaverService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            foreach (Product p in _productService.GetAll())
                if (MyGroceryListItems.FirstOrDefault(g => g.ProductId == p.Id) == null  && p.Stock > 0)
                    AvailableProducts.Add(p);
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null) return;
            GroceryListItem item = new(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(item);
            product.Stock--;
            _productService.Update(product);
            AvailableProducts.Remove(product);
            OnGroceryListChanged(GroceryList);
        }

        [RelayCommand]
        public async Task ShareGroceryList(CancellationToken cancellationToken)
        {
            if (GroceryList == null || MyGroceryListItems == null) return;
            string jsonString = JsonSerializer.Serialize(MyGroceryListItems);
            try
            {
                await _fileSaverService.SaveFileAsync("Boodschappen.json", jsonString, cancellationToken);
                await Toast.Make("Boodschappenlijst is opgeslagen.").Show(cancellationToken);
            }
            catch (Exception ex)
            {
                await Toast.Make($"Opslaan mislukt: {ex.Message}").Show(cancellationToken);
            }
        }

    }
}
