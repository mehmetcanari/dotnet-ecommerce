using ECommerce.Application.Abstract.Service;
using Iyzipay.Request;
using Iyzipay;
using System.Threading.Tasks;

namespace ECommerce.Application.Services.Payment;

public class IyzipayPaymentProvider : IPaymentProvider
{
    public async Task<Iyzipay.Model.Payment> CreateAsync(CreatePaymentRequest request, Options options)
    {
        return await Iyzipay.Model.Payment.Create(request, options);
    }
} 