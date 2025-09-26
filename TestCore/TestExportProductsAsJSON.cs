using System.Text.Json;
using TestCore.ViewModels;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using Moq;

namespace TestCore
{
    [TestFixture]
    public class TestExportProductsAsJSON
    {
        private Mock<IProductService> _productService;
        private Mock<IFileSaverService> _fileSaver;

        [SetUp]
        public void SetUp()
        {
            _productService = new Mock<IProductService>();
            _fileSaver = new Mock<IFileSaverService>();
        }

        [Test]
        public async Task ExportProductsListAsJson_SavesSerializedProducts_WithExpectedFileName()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product(1, "Appel", 10),
                new Product(2, "Banaan", 0)
            };
            _productService.Setup(s => s.GetAll()).Returns(products);

                var vm = new TestExportProductsToJSONViewModel(_productService.Object, _fileSaver.Object);
                var token = CancellationToken.None;
                var expectedJson = JsonSerializer.Serialize(vm.Products);

            // Act
            await vm.ExportProductsListAsJson(token);

            // Assert
            _fileSaver.Verify(s => s.SaveFileAsync(
                "Producten.json",
                expectedJson,
                token),
                Times.Once);
        }

        [Test]
        public async Task ExportProductsListAsJson_WhenProductsIsNull_DoesNotCallFileSaver()
        {
            // Arrange
            _productService.Setup(s => s.GetAll()).Returns(new List<Product>());
            var vm = new TestExportProductsToJSONViewModel(_productService.Object, _fileSaver.Object)
            {
                Products = null!
            };

            // Act
            await vm.ExportProductsListAsJson(CancellationToken.None);

            // Assert
            _fileSaver.Verify(s => s.SaveFileAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task ExportProductsListAsJson_WhenSaveThrows_MethodDoesNotThrow()
        {
            // Arrange
            var products = new List<Product> { new Product(1, "Appel", 10) };
            _productService.Setup(s => s.GetAll()).Returns(products);

            _fileSaver
                .Setup(s => s.SaveFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("IO error"));

            var vm = new TestExportProductsToJSONViewModel(_productService.Object, _fileSaver.Object);

            // Act & Assert (no exception should escape)
            Assert.DoesNotThrowAsync(async () => await vm.ExportProductsListAsJson(CancellationToken.None));
            _fileSaver.Verify(s => s.SaveFileAsync(
                "Producten.json",
                It.Is<string>(json => !string.IsNullOrWhiteSpace(json)),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}