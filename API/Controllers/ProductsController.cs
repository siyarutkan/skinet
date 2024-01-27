using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController : BaseApiController
	{
		public readonly IGenericRepository<Product> _productsRepo;
		public readonly IGenericRepository<ProductBrand> _productBrandsRepo;
		public readonly IGenericRepository<ProductType> _productTypesRepo;
		private readonly IMapper _mapper;

		public ProductsController(IGenericRepository<Product> productsRepo, 
		IGenericRepository<ProductBrand> productBrandsRepo, 
		IGenericRepository<ProductType> productTypesRepo, IMapper mapper)
		{
			_mapper = mapper;
			_productsRepo = productsRepo;
			_productBrandsRepo = productBrandsRepo;
			_productTypesRepo = productTypesRepo;			
		}

		[HttpGet]
		public async Task<ActionResult<List<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams productParams)
		{
			var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
			var countSpec = new ProductsWithFiltersForCountSpecification(productParams);
			var totalItems = await _productsRepo.CountAsync(countSpec);
			
			var products = await _productsRepo.ListAsync(spec);
			var data = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);
			
			return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, productParams.PageSize, totalItems, data));
		}
		
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
		{
			var spec = new ProductsWithTypesAndBrandsSpecification(id);
			var product = await _productsRepo.GetEntityWithSpec(spec);
			 if (product == null) return NotFound(new ApiResponse(404));
			return _mapper.Map<Product, ProductToReturnDto>(product);
		}
		
		[HttpGet("brands")]
		public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
		{
			return Ok(await _productBrandsRepo.ListAllAsync());
		}
		
		[HttpGet("types")]
		public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductTypes()
		{
			return Ok(await _productTypesRepo.ListAllAsync());
		}
	}
}