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
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/shopping-items")]
    [ApiController]
    // [Authorize(Roles = "Visitor")]
    public class ShoppingItemController : ControllerBase
    {
        private readonly IShoppingItemService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<ShoppingItem> _pagingSupport;

        public ShoppingItemController(IShoppingItemService service, IMapper mapper, IPagingSupport<ShoppingItem> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }



        /// <summary>
        /// Get a specific shopping item by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the shopping item with the corresponding id</returns>
        /// <response code="200">Returns the shopping item with the specified id</response>
        /// <response code="404">No shopping items found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingItemVM>> GetShoppingItemById(int id)
        {
            ResponseModel responseModel = new();

            var result = await _service.GetByIdAsync(_ => _.Id == id, _ => _.Product);
            if (result == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingItem));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }
            var rtnShoppingItem = _mapper.Map<ShoppingItemVM>(result);
            return Ok(rtnShoppingItem);
        }

        /// <summary>
        /// Get all shopping items
        /// </summary>
        /// <returns>All shopping items</returns>
        /// <response code="200">Returns all shopping items</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ShoppingItemVM>> GetAllShoppingItems([FromQuery] ShoppingItemSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var result = _service.GetAll(_ => _.Product);
            if (model.ShoppingListId > 0)
            {
                result = result.Where(_ => _.ShoppingListId == model.ShoppingListId);
            }
            if (model.Note != null)
            {
                result = result.Where(_ => _.Note.Contains(model.Note));
            }

            var pagedModel = _pagingSupport.From(result)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<ShoppingItemVM>();

            return Ok(pagedModel);
        }
        
        /// <summary>
        /// Count shopping items
        /// </summary>
        /// <returns>Number of shopping items</returns>
        /// <response code="200">Returns number of shopping items</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ShoppingItemVM>> CountShoppingItems([FromQuery] ShoppingItemSM model)
        {
            var result = _service.GetAll(_ => _.Product);
            if (model.ShoppingListId > 0)
            {
                result = result.Where(_ => _.ShoppingListId == model.ShoppingListId);
            }
            if (model.Note != null)
            {
                result = result.Where(_ => _.Note.Contains(model.Note));
            }

            return Ok(result.Count());
        }

        /// <summary>
        /// Create a new shopping items
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST    
        ///     [
        ///        {
        ///           "shoppingListId": 0,
        ///           "productId": 0,
        ///           "note": "string"
        ///        }
        ///     ]
        ///
        /// </remarks>
        /// <param name="model">Information applied to create shopping items</param>
        /// <response code="201">Created a new shopping items</response>
        /// <response code="400">Bad request body when create items</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateShoppingItem([FromBody] ShoppingItemCM model)
        {
            ResponseModel responseModel = new();

            var item = _mapper.Map<ShoppingItem>(model);
            try
            {
                await _service.AddAsync(item);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return CreatedAtAction("CreateShoppingItem", new { Id = item.Id }, item);
        }

        /// <summary>
        /// Update shopping item with specified id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT 
        ///     {
        ///        "productId": 0,
        ///        "note": "string"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Shopping item's id</param>
        /// <param name="model">Information applied to updated shopping item</param>
        /// <response code="204">Update shopping item successfully</response>
        /// <response code="400">Shopping item's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutShoppingItem(int id, [FromBody] ShoppingItemUM model)
        {
            ResponseModel responseModel = new();

            var dataToUpdate = await _service.GetByIdAsync(_ => _.Id == id);
            if (dataToUpdate == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingItem));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            try
            {
                dataToUpdate.Note = model.Note;
                _service.Update(dataToUpdate);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return NoContent();
        }


        /// <summary>
        /// Delete shopping item with specified id
        /// </summary>
        /// <param name="id">Shopping item's id</param>
        /// <response code="204">Delete shopping item successfully</response>
        /// <response code="404">Shopping item's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            var dataToDelete = await _service.GetByIdAsync(_ => _.Id == id);
            if (dataToDelete == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingItem));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            try
            {
                _service.Delete(dataToDelete);
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
