using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProductCatalog.Api.Contracts;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Products;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [MapToApiVersion(1.0)]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet]
    [MapToApiVersion(2.0)]
    [ProducesResponseType<PagedResult<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int pageSize = Paging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await productService.GetPagedAsync(page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return ToProblem(Error.NotFound($"Product with id {id} was not found."));
        }

        return Ok(product);
    }

    [HttpPatch("{id:int}/description")]
    [Consumes("application/json")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateDescription(
        int id,
        UpdateProductDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await productService.UpdateDescriptionAsync(id, request.Description, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    private ActionResult ToProblem(Error error)
    {
        if (error.Type == ErrorType.Validation && error.ValidationErrors is not null)
        {
            var modelState = new ModelStateDictionary();
            foreach (var (key, messages) in error.ValidationErrors)
            {
                foreach (var message in messages)
                {
                    modelState.AddModelError(key, message);
                }
            }

            return ValidationProblem(modelState);
        }

        return error.Type switch
        {
            ErrorType.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: error.Message),
            _ => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad Request", detail: error.Message)
        };
    }
}
