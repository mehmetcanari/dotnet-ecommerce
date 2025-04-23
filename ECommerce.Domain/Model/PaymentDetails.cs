namespace ECommerce.Domain.Model;

public class PaymentDetails
{
    public required string CardHolderName {get;set;}
    public required string CardNumber {get;set;}
    public required string ExpirationDate {get;set;}
    public required string CVV {get;set;}
}
