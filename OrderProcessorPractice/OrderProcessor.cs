using System;

public interface IDatabase
{
    bool IsConnected { get; }
    void Connect();
    void Save(Order order);
    Order GetOrder(int id);
}

public interface IEmailService
{
    void SendOrderConfirmation(string customerEmail, int orderId);
}

public class Order
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsProcessed { get; set; }
}

public class OrderProcessor
{
    private readonly IDatabase _database;
    private readonly IEmailService _emailService;
    private const decimal EmailThreshold = 100;

    public OrderProcessor(IDatabase database, IEmailService emailService)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public bool ProcessOrder(Order order)
    {
        ValidateOrder(order);

        if (!IsOrderAmountValid(order))
            return false;

        EnsureDatabaseConnection();

        return TryProcessOrder(order);
    }

    private void ValidateOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
    }

    private bool IsOrderAmountValid(Order order)
    {
        return order.TotalAmount > 0;
    }

    private void EnsureDatabaseConnection()
    {
        if (!_database.IsConnected)
            _database.Connect();
    }

    private bool TryProcessOrder(Order order)
    {
        try
        {
            _database.Save(order);
            MarkOrderAsProcessed(order);
            SendConfirmationEmailIfNeeded(order);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void MarkOrderAsProcessed(Order order)
    {
        order.IsProcessed = true;
    }

    private void SendConfirmationEmailIfNeeded(Order order)
    {
        if (order.TotalAmount > EmailThreshold)
        {
            _emailService.SendOrderConfirmation(order.CustomerEmail, order.Id);
        }
    }
}