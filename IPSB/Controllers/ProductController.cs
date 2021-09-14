using AutoMapper;
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
using System.Threading.Tasks;

namespace IPSB.Controllers
{
    [Route("api/v1.0/products")]
    [ApiController]
    [Authorize(Roles = "Visitor, Store Owner")]
    public class ProductController : AuthorizeController
    {
        private readonly IProductService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Product> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;

        public ProductController(IProductService service, IMapper mapper, IPagingSupport<Product> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
        }

        /// <summary>
        /// Get a specific product by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the product with the corresponding id</returns>
        /// <response code="200">Returns the product with the specified id</response>
        /// <response code="404">No products found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<ProductVM> GetProductById(int id)
        {
            var product = _service.GetByIdAsync(_ => _.Id == id, _ => _.ProductCategory, _ => _.ProductGroup, _ => _.Store).Result;

            if (product == null)
            {
                return NotFound();
            }

            var rtnEdge = _mapper.Map<ProductVM>(product);

            return Ok(rtnEdge);
        }

        /// <summary>
        /// Get all products
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
        /// <returns>All products</returns>
        /// <response code="200">Returns all products</response>
        /// <response code="404">No products found</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductVM>> GetAllProducts([FromQuery] ProductSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Product> list = _service.GetAll(_ => _.ProductCategory, _ => _.ProductGroup, _ => _.Store);

            if (model.StoreId != 0)
            {
                list = list.Where(_ => _.StoreId == model.StoreId);
            }
            
            if (model.ProductCategoryId != 0)
            {
                list = list.Where(_ => _.ProductCategoryId == model.ProductCategoryId);
            }
            
            if (model.ProductGroupId != 0)
            {
                list = list.Where(_ => _.ProductGroupId == model.ProductGroupId);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                list = list.Where(_ => _.Description.Contains(model.Description));
            }

            if (model.LowerPrice != 0)
            {
                list = list.Where(_ => _.Price >= model.LowerPrice);
            }

            if (model.UpperPrice != 0)
            {
                list = list.Where(_ => _.Price <= model.UpperPrice);
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
                .Paginate<ProductVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Name": "Name of the product",   
        ///         "StoreId": "Id of the store which the product belongs to",   
        ///         "ProductGroupId": "Id of the product group which the product belongs to",   
        ///         "ImageUrl": "Image of the product",   
        ///         "Description": "General description of the product",   
        ///         "ProductCategoryId": "Id of the product category which the product belongs to",   
        ///         "Price": "Price of the product",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new product</response>
        /// <response code="409">Product already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductCM>> CreateProduct([FromForm] ProductCM model)
        {
            Product product = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
            if (product is not null)
            {
                return Conflict();
            }

            //if (!string.IsNullOrEmpty(model.Status))
            //{
            //    if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
            //    {
            //        return BadRequest();
            //    }
            //}

            Product crtProduct = _mapper.Map<Product>(model);
            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "product", "product-detail");
            crtProduct.ImageUrl = imageURL;

            // Default POST Status = "New"
            crtProduct.Status = Constants.Status.NEW;

            try
            {
                await _service.AddAsync(crtProduct);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetProductById", new { id = crtProduct.Id }, crtProduct);
        }

        /// <summary>
        /// Update product with specified id
        /// </summary>
        /// <param name="id">Product's id</param>
        /// <param name="model">Information applied to updated product</param>
        /// <response code="204">Update product successfully</response>
        /// <response code="400">Product's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Product already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutProduct(int id, [FromForm] ProductUM model)
        {

            Product updProduct = await _service.GetByIdAsync(_ => _.Id == id);

            if (!updProduct.Name.ToUpper().Equals(model.Name.ToUpper()))
            {
                Product product = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
                if (product is not null)
                {
                    return Conflict();
                }
            }

            if (updProduct == null || id != model.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.NEW && model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

            string imageURL = updProduct.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "product", "product-detail");
            }

            try
            {
                updProduct.Id = model.Id;
                updProduct.StoreId = model.StoreId;
                updProduct.ProductGroupId = model.ProductGroupId;
                updProduct.Name = model.Name;
                updProduct.ImageUrl = imageURL;
                updProduct.Description = model.Description;
                updProduct.ProductCategoryId = model.ProductCategoryId;
                updProduct.Price = model.Price;
                updProduct.Status = model.Status;

                _service.Update(updProduct);
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
