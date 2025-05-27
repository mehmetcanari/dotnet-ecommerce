using ECommerce.Application.Abstract.Service;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;

namespace ECommerce.Application.Services.Payment;

public class IyzicoPaymentProvider : IPaymentProvider
{
    public async Task<Iyzipay.Model.Payment> CreateAsync(CreatePaymentRequest request, Options options)
    {
        return await Task.Run(() => Iyzipay.Model.Payment.Create(request, options));
    }
} 