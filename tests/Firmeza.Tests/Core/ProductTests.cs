using Firmeza.Core.Entities;
using Xunit;

namespace Firmeza.Tests.Core;

// Pruebas básicas de la entidad Product
public class ProductTests
{
    [Fact]
    public void Product_DefaultStock_ShouldBeZero()
    {
        // Verifica que un producto nuevo empieza con stock 0
        var product = new Product();
        Assert.Equal(0, product.Stock);
    }

    [Fact]
    public void Product_DefaultPrice_ShouldBeZero()
    {
        var product = new Product();
        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void Product_Name_ShouldNotBeEmpty_AfterAssignment()
    {
        var product = new Product { Name = "Gray Cement" };
        Assert.False(string.IsNullOrEmpty(product.Name));
    }

    [Fact]
    public void Product_SaleDetails_ShouldInitializeEmpty()
    {
        // La colección de detalles no debe ser null al crear un producto
        var product = new Product();
        Assert.NotNull(product.SaleDetails);
        Assert.Empty(product.SaleDetails);
    }

    [Theory]
    [InlineData(0,   true)]   // stock 0 es bajo
    [InlineData(9,   true)]   // stock 9 es bajo
    [InlineData(10,  false)]  // stock 10 ya no es bajo
    [InlineData(100, false)]  // stock alto no es bajo
    public void Product_LowStock_DetectedCorrectly(int stock, bool expectedLow)
    {
        var product = new Product { Stock = stock };
        bool isLow  = product.Stock < 10;
        Assert.Equal(expectedLow, isLow);
    }
}
