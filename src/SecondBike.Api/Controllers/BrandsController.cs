using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Brands;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Brand management endpoints — CRUD for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class BrandsController : BaseApiController
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => ToResponse(await _brandService.GetAllAsync(ct));

    [HttpGet("{brandId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int brandId, CancellationToken ct)
        => ToResponse(await _brandService.GetByIdAsync(brandId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto, CancellationToken ct)
        => ToResponse(await _brandService.CreateAsync(dto, ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBrandDto dto, CancellationToken ct)
        => ToResponse(await _brandService.UpdateAsync(dto, ct));

    [HttpDelete("{brandId:int}")]
    public async Task<IActionResult> Delete(int brandId, CancellationToken ct)
        => ToResponse(await _brandService.DeleteAsync(brandId, ct));
}
