using Moq;
using Xunit;
using System;

public class OrderProcessorTests
{
    [Fact]
    public void ProcessOrder_WhenOrderIsNull_ThrowsArgumentNullException()
    {
        var mockDatabase = new Mock<IDatabase>();
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);

        Assert.Throws<ArgumentNullException>(() => processor.ProcessOrder(null));
    }

    [Fact]
    public void ProcessOrder_WhenTotalAmountIsZero_ReturnsFalse()
    {
        var mockDatabase = new Mock<IDatabase>();
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 0, CustomerEmail = "test@test.com" };

        var result = processor.ProcessOrder(order);

        Assert.False(result);
        mockDatabase.Verify(x => x.Save(It.IsAny<Order>()), Times.Never);
        mockEmailService.Verify(x => x.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void ProcessOrder_WhenTotalAmountIsNegative_ReturnsFalse()
    {
        var mockDatabase = new Mock<IDatabase>();
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = -10, CustomerEmail = "test@test.com" };

        var result = processor.ProcessOrder(order);

        Assert.False(result);
        mockDatabase.Verify(x => x.Save(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public void ProcessOrder_WhenDatabaseIsNotConnected_CallsConnectMethod()
    {
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(x => x.IsConnected).Returns(false);
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 50, CustomerEmail = "test@test.com", Id = 1 };

        var result = processor.ProcessOrder(order);

        mockDatabase.Verify(x => x.Connect(), Times.Once);
        mockDatabase.Verify(x => x.Save(order), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void ProcessOrder_WhenTotalAmountIsLessThan100_DoesNotSendEmail()
    {
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(x => x.IsConnected).Returns(true);
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 50, CustomerEmail = "test@test.com", Id = 1 };

        var result = processor.ProcessOrder(order);

        mockEmailService.Verify(x => x.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        Assert.True(order.IsProcessed);
        Assert.True(result);
    }

    [Fact]
    public void ProcessOrder_WhenTotalAmountIsGreaterThan100_SendsEmail()
    {
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(x => x.IsConnected).Returns(true);
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 150, CustomerEmail = "test@test.com", Id = 1 };

        var result = processor.ProcessOrder(order);

        mockEmailService.Verify(x => x.SendOrderConfirmation("test@test.com", 1), Times.Once);
        Assert.True(order.IsProcessed);
        Assert.True(result);
    }

    [Fact]
    public void ProcessOrder_SavesOrderToDatabase()
    {
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(x => x.IsConnected).Returns(true);
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 75, CustomerEmail = "test@test.com", Id = 1 };

        var result = processor.ProcessOrder(order);

        mockDatabase.Verify(x => x.Save(order), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void ProcessOrder_WhenDatabaseSaveThrowsException_ReturnsFalse()
    {
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(x => x.IsConnected).Returns(true);
        mockDatabase.Setup(x => x.Save(It.IsAny<Order>())).Throws(new Exception("Database error"));
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 75, CustomerEmail = "test@test.com", Id = 1 };

        var result = processor.ProcessOrder(order);

        Assert.False(result);
        mockEmailService.Verify(x => x.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void ProcessOrder_WhenTotalAmountIsExactly100_DoesNotSendEmail()
    {
        var mockDatabase = new Mock<IDatabase>();
        mockDatabase.Setup(x => x.IsConnected).Returns(true);
        var mockEmailService = new Mock<IEmailService>();
        var processor = new OrderProcessor(mockDatabase.Object, mockEmailService.Object);
        var order = new Order { TotalAmount = 100, CustomerEmail = "test@test.com", Id = 1 };

        var result = processor.ProcessOrder(order);

        mockEmailService.Verify(x => x.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        Assert.True(result);
        Assert.True(order.IsProcessed);
    }
}