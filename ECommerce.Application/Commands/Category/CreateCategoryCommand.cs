using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class CreateCategoryCommand(CreateCategoryRequestDto request) : IRequest<Result>
{
    public readonly CreateCategoryRequestDto Model = request;
}

public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, ILogService logger, IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Result>
{
    public async Task<Result> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryExists = await categoryRepository.CheckNameExists(request.Model.Name, cancellationToken);
            if (categoryExists)
                return Result.Failure(ErrorMessages.CategoryExists);

            var category = new Domain.Model.Category
            {
                Name = request.Model.Name,
                Description = request.Model.Description
            };

            await categoryRepository.Create(category, cancellationToken);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorCreatingCategory);
            return Result.Failure(ErrorMessages.ErrorCreatingCategory);
        }
    }
}