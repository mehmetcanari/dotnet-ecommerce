namespace ECommerce.Application.Abstract.Service;

using Iyzipay.Model;
using Iyzipay.Request;
using Iyzipay;
using System.Threading.Tasks;

public interface IPaymentProvider
{
    Task<Payment> CreateAsync(CreatePaymentRequest request, Options options);
} 