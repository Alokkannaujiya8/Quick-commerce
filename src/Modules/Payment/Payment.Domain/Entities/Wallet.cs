namespace Payment.Domain.Entities;

public class Wallet
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = "INR";
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Wallet() { }

    public static Wallet Create(Guid userId, string currency = "INR")
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        return new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0m,
            Currency = currency.Trim().ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be positive.");

        Balance = decimal.Round(Balance + amount, 2);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Debit amount must be positive.");
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient wallet balance.");

        Balance = decimal.Round(Balance - amount, 2);
        UpdatedAt = DateTime.UtcNow;
    }
}
