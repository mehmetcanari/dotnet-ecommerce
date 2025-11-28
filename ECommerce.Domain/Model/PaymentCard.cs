namespace ECommerce.Domain.Model;

public class PaymentCard
{
    public required string CardHolderName { get; init; }
    public required string CardNumber { get; init; }
    public required int ExpirationMonth { get; init; }
    public required int ExpirationYear { get; init; }
    public required string Cvc { get; init; }
    public required int RegisterCard { get; init; }
}
