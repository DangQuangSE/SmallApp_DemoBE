using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Categories;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Category (BikeType) management endpoints — CRUD for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => ToResponse(await _categoryService.GetAllAsync(ct));

    [HttpGet("{typeId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int typeId, CancellationToken ct)
        => ToResponse(await _categoryService.GetByIdAsync(typeId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
        => ToResponse(await _categoryService.CreateAsync(dto, ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryDto dto, CancellationToken ct)
        => ToResponse(await _categoryService.UpdateAsync(dto, ct));

    [HttpDelete("{typeId:int}")]
    public async Task<IActionResult> Delete(int typeId, CancellationToken ct)
        => ToResponse(await _categoryService.DeleteAsync(typeId, ct));
}
