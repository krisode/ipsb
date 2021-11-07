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
using static IPSB.Utils.Constants;


namespace IPSB.Controllers
{
    [Route("api/v1.0/product-categories")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductCategoryController : ControllerBase
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
            ResponseModel responseModel = new();

            var proCate = _service.GetByIdAsync(_ => _.Id == id).Result;

            if (proCate == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ProductCategory));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
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
        [HttpGet]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ProductCategoryRefModel>> GetAllProductCategories([FromQuery] ProductCategorySM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            var list = _service.GetAll();

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Status.ACTIVE && model.Status != Status.INACTIVE)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }

                else
                {
                    if (model.Status == Status.ACTIVE)
                    {
                        list = list.Where(_ => _.Status == Status.ACTIVE);
                    }

                    if (model.Status == Status.INACTIVE)
                    {
                        list = list.Where(_ => _.Status == Status.INACTIVE);
                    }
                }
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<ProductCategoryRefModel>();

            return Ok(pagedModel);
        }
        
        /// <summary>
        /// Count product categories
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
        /// <returns>Number of product categories</returns>
        /// <response code="200">Returns number of product categories</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ProductCategoryRefModel>> CountProductCategories([FromQuery] ProductCategorySM model)
        {
            ResponseModel responseModel = new();

            var list = _service.GetAll();

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Status.ACTIVE && model.Status != Status.INACTIVE)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }

                else
                {
                    if (model.Status == Status.ACTIVE)
                    {
                        list = list.Where(_ => _.Status == Status.ACTIVE);
                    }

                    if (model.Status == Status.INACTIVE)
                    {
                        list = list.Where(_ => _.Status == Status.INACTIVE);
                    }
                }
            }

            return Ok(list.Count());
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
            ResponseModel responseModel = new();

            ProductCategory crtProCate = _mapper.Map<ProductCategory>(proCateModel);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, crtProCateType, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to create product category: {proCateModel.Name}");
            }*/

            string imageURL = await _uploadFileService.UploadFile("123456798", proCateModel.ImageUrl, "building", "building-detail");
            crtProCate.ImageUrl = imageURL;

            // Default status when creating is ACTIVE
            crtProCate.Status = Status.ACTIVE;

            try
            {
                await _service.AddAsync(crtProCate);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
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
            ResponseModel responseModel = new();
            if (id != model.Id)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Id));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }
            ProductCategory updProCate = await _service.GetByIdAsync(_ => _.Id == id);

            if (updProCate == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ProductCategory));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
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
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
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
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            ProductCategory proCate = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (proCate is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ProductCategory));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (proCate.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(ProductCategory));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            proCate.Status = Status.INACTIVE;

            try
            {
                _service.Update(proCate);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_DELETE;
                responseModel.Type = ResponseType.CAN_NOT_DELETE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return NoContent();

        }

    }
}
