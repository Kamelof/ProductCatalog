using System.Net.Mime;
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
public class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>Returns all products ordered by id.</summary>
    /// <response code="200">List of products.</response>
    [HttpGet]
    [MapToApiVersion(1.0)]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>Returns one page of products ordered by id.</summary>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Items per page, from 1 to 100.</param>
    /// <response code="200">Requested page. A page past the end has no items.</response>
    /// <response code="400">Page or page size is out of range.</response>
    [HttpGet]
    [MapToApiVersion(2.0)]
    [ProducesResponseType<PagedResult<ProductDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int pageSize = Paging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await productService.GetPagedAsync(page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>Returns a product by id.</summary>
    /// <param name="id">Product id.</param>
    /// <response code="200">The product.</response>
    /// <response code="404">Product does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return ToProblem(Error.NotFound($"Product with id {id} was not found."));
        }

        return Ok(product);
    }

    /// <summary>Updates the product description. Other product fields are not changed.</summary>
    /// <param name="id">Product id.</param>
    /// <param name="request">New description, required, at most 4000 characters.</param>
    /// <response code="200">The updated product.</response>
    /// <response code="400">Description is missing, blank, too long or contains control characters.</response>
    /// <response code="404">Product does not exist.</response>
    [HttpPatch("{id:int}/description")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
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
