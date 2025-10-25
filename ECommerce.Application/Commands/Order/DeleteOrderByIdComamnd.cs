using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class DeleteOrderByIdCommand() : IRequest<Result>
{
    public required Guid Id { get; init; }
}

public class DeleteOrderByIdCommandHandler(IOrderRepository orderRepository, ILoggingService logger, IUnitOfWork unitOfWork) : IRequestHandler<DeleteOrderByIdCommand, Result>
{
    public async Task<Result> Handle(DeleteOrderByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var orderResult = await ValidateAndGetOrder(request);
            if (orderResult is { IsFailure: true, Message: not null })
                return Result.Failure(orderResult.Message);

            if (orderResult.Data is null)
                return Result.Failure(ErrorMessages.OrderNotFound);

            await DeleteOrder(orderResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<Domain.Model.Order>> ValidateAndGetOrder(DeleteOrderByIdCommand request)
    {
        var order = await orderRepository.GetById(request.Id);
        if (order == null)
        {
            logger.LogWarning(ErrorMessages.OrderNotFound, request.Id);
            return Result<Domain.Model.Order>.Failure(ErrorMessages.OrderNotFound);
        }

        return Result<Domain.Model.Order>.Success(order);
    }

    private async Task DeleteOrder(Domain.Model.Order order)
    {
        orderRepository.Delete(order);
        await unitOfWork.Commit();

        logger.LogInformation(ErrorMessages.OrderDeleted, order.Id, order.Status);
    }
}