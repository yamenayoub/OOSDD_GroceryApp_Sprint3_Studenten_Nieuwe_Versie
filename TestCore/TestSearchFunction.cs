using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using TestCore.ViewModels;
using Moq;

namespace TestCore
{
    [TestFixture]
    public class TestSearchFunction
    {
        private TestViewModel _viewModel;
        private List<Product> _testProducts;
        private Mock<IProductService> _mockProductService;

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockProductService = new Mock<IProductService>();
            
            // Create test data
            _testProducts =
            [
                new(1, "Appel", 10),
                new (2, "Banaan", 15),
                new (3, "Sinaasappel", 20),
                new (4, "Druif", 25),
                new (5, "Aardbei", 30)
            ];
            
            // Configure product service to return our test products
            _mockProductService.Setup(s => s.GetAll()).Returns(_testProducts);
                
            // Initialize the view model with mock services
            _viewModel = new TestViewModel(_mockProductService.Object);
        }

        [Test]
        public void SearchProducts_EmptySearchTerm_ReturnsAllProducts()
        {
            // Arrange
            _viewModel.ProductSearchText = "";
            
            // Act
            _viewModel.SearchProducts("");

            // Assert
            Assert.That(_viewModel.AvailableProducts.Count, Is.EqualTo(5));
            Assert.That(_viewModel.MyMessage, Is.Empty);
        }

        [Test]
        public void SearchProducts_MatchingTerm_ReturnsFilteredProducts()
        {
            // Arrange
            _viewModel.ProductSearchText = "";
            
            // Act
            _viewModel.SearchProducts("aa");
            
            // Assert
            Assert.That(_viewModel.AvailableProducts.Count, Is.EqualTo(3)); // Should find "Banaan" and "Sinaasappel"
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Banaan"), Is.True);
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Sinaasappel"), Is.True);
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Aardbei"), Is.True);
            Assert.That(_viewModel.MyMessage, Is.Empty);
        }

        [Test]
        public void SearchProducts_NoMatchingProducts_ReturnsEmptyList()
        {
            // Arrange
            _viewModel.ProductSearchText = "";
            
            // Act
            _viewModel.SearchProducts("xyz");
            
            // Assert
            Assert.That(_viewModel.AvailableProducts.Count, Is.EqualTo(0));
            Assert.That(_viewModel.MyMessage, Is.EqualTo("Geen producten gevonden"));
        }

        [Test]
        public void SearchProducts_CaseInsensitiveSearch_ReturnsMatchingProducts()
        {
            // Arrange
            _viewModel.ProductSearchText = "";
            
            // Act
            _viewModel.SearchProducts("DRUIF");
            
            // Assert
            Assert.That(_viewModel.AvailableProducts.Count, Is.EqualTo(1));
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Druif"), Is.True);
            Assert.That(_viewModel.MyMessage, Is.Empty);
        }

        [Test]
        public void SearchProducts_NullParameter_UseProductSearchText()
        {
            // Arrange
            _viewModel.ProductSearchText = "druif";
            
            // Act
            _viewModel.SearchProducts(null);
            
            // Assert
            Assert.That(_viewModel.AvailableProducts.Count, Is.EqualTo(1));
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Druif"), Is.True);
            Assert.That(_viewModel.MyMessage, Is.Empty);
        }

        [Test]
        public void SearchProducts_TrimmedSearchTerm_MatchesCorrectly()
        {
            // Arrange
            _viewModel.ProductSearchText = "";
            
            // Act
            _viewModel.SearchProducts("  appel  ");

            // Assert, hij moet twee producten teruggeven: appel en sinaasappel
            Assert.That(_viewModel.AvailableProducts.Count, Is.EqualTo(2));
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Appel"), Is.True);
            Assert.That(_viewModel.AvailableProducts.Any(p => p.Name == "Sinaasappel"), Is.True);
            Assert.That(_viewModel.MyMessage, Is.Empty);
        }
    }
}