using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace TestCore.ViewModels
{
    public partial class TestExportProductsToJSONViewModel
    {
        private readonly IFileSaverService _fileSaverService;

        public ObservableCollection<Product> Products { get; set; }

        public TestExportProductsToJSONViewModel(IProductService productService, IFileSaverService fileSaverService)
        {
            Products = new(productService.GetAll());
            _fileSaverService = fileSaverService;
        }

        [RelayCommand]
        public async Task ExportProductsListAsJson(CancellationToken cancellationToken)
        {
            if (Products == null) return;
            string jsonString = JsonSerializer.Serialize(Products);
            try
            {
                await _fileSaverService.SaveFileAsync("Producten.json", jsonString, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Opslaan mislukt: {ex.Message}");
            }
        }
    }
}
