using AutoMapper;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IPSB.Controllers
{
    [Route("api/v1.0/product-categories")]
    [ApiController]
    public class ProductCategoryController : AuthorizeController
    {
        private readonly IProductCategoryService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<ProductCategory> _pagingSupport;

        public ProductCategoryController(IProductCategoryService service, IMapper mapper, IPagingSupport<ProductCategory> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }


        /// <summary>
        /// Get a specific product category by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the product category with the corresponding id</returns>
        /// <response code="200">Returns the product category with the specified id</response>
        /// <response code="404">No product categories found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<ProductCategoryVM> GetProductCategoryById(int id)
        {
            //IQueryable<ProductCategory> proList = _service.GetAll(_ => _.Products);
            //var proCate = proList.FirstOrDefault(_ => _.Id == id);

            var proCate = _service.GetByIdAsync(_ => _.Id == id, _ => _.Products);
        
            if (proCate == null)
            {
                return NotFound();
            }

            var rtnProCate = _mapper.Map<ProductCategoryVM>(proCate.Result);

            return Ok(rtnProCate);
        }

        /// <summary>
        /// Get all product categories
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET 
        ///     {
        ///         
        ///     }
        ///
        /// </remarks>
        /// <returns>All service types</returns>
        /// <response code="200">Returns all product categories</response>
        /// <response code="404">No product categories found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductCategoryVM>> GetAllProductCategories([FromQuery] ProductCategorySM model, int pageSize, int pageIndex)
        {
            IQueryable<ProductCategory> serviceTypeList = _service.GetAll(s => s.Products);

            if (!string.IsNullOrEmpty(model.Name))
            {
                serviceTypeList = serviceTypeList.Where(s => s.Name.Contains(model.Name));
            }

            if (pageSize == 0)
            {
                pageSize = 20;
            }

            if (pageIndex == 0)
            {
                pageIndex = 1;
            }

            var pagedModel = _pagingSupport.From(serviceTypeList)
                .GetRange(pageIndex, pageSize, s => s.Id)
                .Paginate<ProductCategoryVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new product category
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "name": "Product Category's name",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created new product category</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductCategoryCM>> CreateProductCategory([FromBody] ProductCategoryCM proCateModel)
        {
            /*DateTime crtDate = DateTime.Now;
            DateTime updDate = DateTime.Now;*/

            ProductCategory crtProCateType = _mapper.Map<ProductCategory>(proCateModel);

            try
            {
                /*crtService.CreatedDate = crtDate;
                crtService.UpdatedDate = updDate;*/

                await _service.AddAsync(crtProCateType);
                await _service.Save();

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetProductCategoryById", new { id = crtProCateType.Id }, crtProCateType);
        }

        /// <summary>
        /// Update product category with specified id
        /// </summary>
        /// <param name="id">Product category's id</param>
        /// <param name="productCategory">Information applied to updated service type</param>
        /// <response code="204">Update product category successfully</response>
        /// <response code="400">>Product category's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutProductCategory(int id, [FromBody] ProductCategoryUM productCategory)
        {
            ProductCategory updProCate = await _service.GetByIdAsync(_ => _.Id == id);
            if (updProCate == null || id != productCategory.Id)
            {
                return BadRequest();
            }

            try
            {
                updProCate.Id = productCategory.Id;
                updProCate.Name = productCategory.Name;
                _service.Update(updProCate);
                await _service.Save();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return NoContent();
        }

        // DELETE api/<ProductCategoryController>/5
        // Change Status to Inactive
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }

        protected override bool IsAuthorize()
        {
            throw new NotImplementedException();
        }
    }
}
