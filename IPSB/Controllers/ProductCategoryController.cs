using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
    public class ProductCategoryController : Controller
    {
        private readonly IProductCategoryService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<ProductCategory> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        // private readonly IAuthorizationService _authorizationService;
        public ProductCategoryController(IProductCategoryService service, IMapper mapper, 
            IPagingSupport<ProductCategory> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
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
        public ActionResult<ProductCategoryRefModel> GetProductCategoryById(int id)
        {
            //IQueryable<ProductCategory> proList = _service.GetAll(_ => _.Products);
            //var proCate = proList.FirstOrDefault(_ => _.Id == id);

            var proCate = _service.GetByIdAsync(_ => _.Id == id).Result;

            if (proCate == null)
            {
                return NotFound();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, proCate, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to access product category with id: {id}");
            }*/

            var rtnProCate = _mapper.Map<ProductCategoryRefModel>(proCate);

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
        /// <returns>All product categories</returns>
        /// <response code="200">Returns all product categories</response>
        /// <response code="404">No product categories found</response>
        [HttpGet]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductCategoryRefModel>> GetAllProductCategories([FromQuery] ProductCategorySM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var list = _service.GetAll();

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }

                else
                {
                    if (model.Status == Constants.Status.ACTIVE)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.ACTIVE);
                    }

                    if (model.Status == Constants.Status.INACTIVE)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.INACTIVE);
                    }
                }
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<ProductCategoryRefModel>();

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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductCategoryCM>> CreateProductCategory([FromBody] ProductCategoryCM proCateModel)
        {

            ProductCategory crtProCate = _mapper.Map<ProductCategory>(proCateModel);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, crtProCateType, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to create product category: {proCateModel.Name}");
            }*/

            string imageURL = await _uploadFileService.UploadFile("123456798", proCateModel.ImageUrl, "building", "building-detail");
            crtProCate.ImageUrl = imageURL;

            // Default status when creating is ACTIVE
            crtProCate.Status = Constants.Status.ACTIVE;

            try
            {
                await _service.AddAsync(crtProCate);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetProductCategoryById", new { id = crtProCate.Id }, crtProCate);
        }

        /// <summary>
        /// Update product category with specified id
        /// </summary>
        /// <param name="id">Product category's id</param>
        /// <param name="model">Information applied to updated service type</param>
        /// <response code="204">Update product category successfully</response>
        /// <response code="400">Product category's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutProductCategory(int id, [FromBody] ProductCategoryUM model)
        {
            ProductCategory updProCate = await _service.GetByIdAsync(_ => _.Id == id);

            if (updProCate == null || id != model.Id)
            {
                return BadRequest();
            }

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updProCate, Operations.Create);
            // if (!authorizedResult.Succeeded)
            // {
            //     return Forbid($"Not authorized to update product category: {updProCate.Name}");
            // }

            string imageURL = updProCate.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "product-category", "product-category-detail");
            }

            try
            {
                updProCate.Id = model.Id;
                updProCate.Name = model.Name;
                updProCate.ImageUrl = imageURL;
                _service.Update(updProCate);
                await _service.Save();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return NoContent();
        }

        /// <summary>
        /// Change the status of product category to inactive
        /// </summary>
        /// <param name="id">Product category's id</param>
        /// <response code="204">Change product category's status successfully</response>
        /// <response code="400">Product category's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            ProductCategory proCate = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (proCate is null)
            {
                return BadRequest();
            }

            if (proCate.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }

            proCate.Status = Constants.Status.INACTIVE;

            try
            {
                _service.Update(proCate);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();

        }

    }
}
