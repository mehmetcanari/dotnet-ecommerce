namespace ECommerce.Domain.Model;

public class PaymentCard
{
    public required string CardHolderName {get;set;}
    public required string CardNumber {get;set;}
    public required int ExpirationMonth {get;set;}
    public required int ExpirationYear {get;set;}
    public required string CVC {get;set;}
    public required int RegisterCard {get;set;}
}
