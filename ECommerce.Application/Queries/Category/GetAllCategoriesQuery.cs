using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Category;

public class GetAllCategoriesQuery(int pageSize, int page) : IRequest<Result<List<CategoryResponseDto>>>
{
    public readonly int PageSize = pageSize;
    public readonly int Page = page;
}

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository, ILogService logService) : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryResponseDto>>>
{
    public async Task<Result<List<CategoryResponseDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await categoryRepository.Read(request.Page, request.PageSize, cancellationToken);
            if(categories.Count == 0)
                return Result<List<CategoryResponseDto>>.Failure(ErrorMessages.CategoryNotFound);

            var categoryResponse = categories.Select(MapToResponseDto).ToList();

            return Result<List<CategoryResponseDto>>.Success(categoryResponse);
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ex.Message);
            return Result<List<CategoryResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private CategoryResponseDto MapToResponseDto(Domain.Model.Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description
    };
}
