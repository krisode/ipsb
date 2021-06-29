using AutoMapper;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.Controllers
{
    [Route("api/v1.0/product-groups")]
    [ApiController]
    public class ProductGroupController : AuthorizeController
    {
        private readonly IProductGroupService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<ProductGroup> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;

        public ProductGroupController(IProductGroupService service, IMapper mapper, IPagingSupport<ProductGroup> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
        }

        /// <summary>
        /// Get a specific product group by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the product group with the corresponding id</returns>
        /// <response code="200">Returns the product group with the specified id</response>
        /// <response code="404">No product groups found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<ProductGroupVM> GetProductGroupById(int id)
        {
            var productGroup = _service.GetByIdAsync(_ => _.Id == id, _ => _.Store, _ => _.Products).Result;

            if (productGroup == null)
            {
                return NotFound();
            }

            var rtnEdge = _mapper.Map<ProductGroupVM>(productGroup);

            return Ok(rtnEdge);
        }

        /// <summary>
        /// Get all product groups
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
        /// <returns>All product groups</returns>
        /// <response code="200">Returns all product groups</response>
        /// <response code="404">No product groups found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductGroupVM>> GetAllProductGroups([FromQuery] ProductGroupSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<ProductGroup> list = _service.GetAll(_ => _.Store, _ => _.Products);

            if (model.StoreId != 0)
            {
                list = list.Where(_ => _.StoreId == model.StoreId);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                list = list.Where(_ => _.Description.Contains(model.Description));
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
                .Paginate<ProductGroupVM>();

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
        ///         "Name": "Name of the product group",     
        ///         "Description": "General description of the product group",     
        ///         "ImageUrl": "Image of the product group", 
        ///         "StoreId": "Id of the store which the product group belongs to",
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new product group</response>
        /// <response code="409">Product group already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductGroupCM>> CreateProductGroup([FromForm] ProductGroupCM model)
        {
            ProductGroup productGroup = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
            if (productGroup is not null)
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

            ProductGroup crtProductGroup = _mapper.Map<ProductGroup>(model);
            string imageURL = await _uploadFileService.UploadFile("123456798", model.Image, "product-group", "product-group-detail");
            crtProductGroup.Image = imageURL;

            // Default POST Status = "New"
            crtProductGroup.Status = Constants.Status.NEW;

            try
            {
                await _service.AddAsync(crtProductGroup);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetProductGroupById", new { id = crtProductGroup.Id }, crtProductGroup);
        }

        /// <summary>
        /// Update product group with specified id
        /// </summary>
        /// <param name="id">Product group's id</param>
        /// <param name="model">Information applied to updated product group</param>
        /// <response code="204">Update product group successfully</response>
        /// <response code="400">Product group's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Product group already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutProductGroup(int id, [FromForm] ProductGroupUM model)
        {

            ProductGroup updProductGroup = await _service.GetByIdAsync(_ => _.Id == id);

            if (!updProductGroup.Name.ToUpper().Equals(model.Name.ToUpper()))
            {
                ProductGroup productGroup = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
                if (productGroup is not null)
                {
                    return Conflict();
                }
            }

            if (updProductGroup == null || id != model.Id)
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

            string imageURL = updProductGroup.Image;

            if (model.Image is not null && model.Image.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.Image, "product-group", "product-group-detail");
            }

            try
            {
                updProductGroup.Id = model.Id;
                updProductGroup.Name = model.Name;
                updProductGroup.Description = model.Description;
                updProductGroup.StoreId = model.StoreId;
                updProductGroup.Status = model.Status;

                _service.Update(updProductGroup);
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
