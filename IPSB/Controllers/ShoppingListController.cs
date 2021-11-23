using AutoMapper;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/v1.0/shopping-lists")]
    [ApiController]
    // [Authorize(Roles = "Visitor")]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListService _service;
        private readonly IShoppingItemService _shoppingItemService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<ShoppingList> _pagingSupport;

        public ShoppingListController(IShoppingListService service, IShoppingItemService shoppingItemService, IMapper mapper, IPagingSupport<ShoppingList> pagingSupport)
        {
            _service = service;
            _shoppingItemService = shoppingItemService;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }


        /// <summary>
        /// Get a specific shopping list by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the shopping list with the corresponding id</returns>
        /// <response code="200">Returns the shopping list with the specified id</response>
        /// <response code="404">No shopping lists found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult GetShoppingListById(int id)
        {
            ResponseModel responseModel = new();

            var result = _service.GetAll(_ => _.Building)
                                .Include(_ => _.ShoppingItems)
                                .ThenInclude(_ => _.Product)
                                .ThenInclude(_ => _.Store)
                                .ThenInclude(_ => _.Location)
                                .AsSplitQuery()
                                .FirstOrDefault(_ => _.Id == id);
            if (result == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingList));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }

            var rtnShoppingList = _mapper.Map<ShoppingListVM>(result);
            return Ok(rtnShoppingList);
        }

        /// <summary>
        /// Get all shopping lists
        /// </summary>
        /// <returns>All shopping lists</returns>
        /// <response code="200">Returns all shopping lists</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ShoppingListVM>> GetAllShoppingLists([FromQuery] ShoppingListSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var result = _service.GetAll(_ => _.Building);
            if (model.BuildingId > 0)
            {
                result = result.Where(_ => _.BuildingId == model.BuildingId);
            }
            if (model.AccountId > 0)
            {
                result = result.Where(_ => _.AccountId == model.AccountId);
            }
            if (model.Name != null)
            {
                result = result.Where(_ => _.Name.Contains(model.Name));
            }
            if (model.StartShoppingDate != null)
            {
                result = result.Where(_ => _.ShoppingDate >= model.StartShoppingDate);
            }
            if (model.EndShoppingDate != null)
            {
                result = result.Where(_ => _.ShoppingDate <= model.EndShoppingDate);
            }
            if (model.Status != null)
            {
                result = result.Where(_ => _.Status.Equals(model.Status));
            }
            var pagedModel = _pagingSupport.From(result)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<ShoppingListVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Count shopping lists
        /// </summary>
        /// <returns>Number of shopping lists</returns>
        /// <response code="200">Returns number of shopping lists</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ShoppingListVM>> CountShoppingLists([FromQuery] ShoppingListSM model)
        {
            var result = _service.GetAll(_ => _.Building);
            if (model.BuildingId > 0)
            {
                result = result.Where(_ => _.BuildingId == model.BuildingId);
            }
            if (model.AccountId > 0)
            {
                result = result.Where(_ => _.AccountId == model.AccountId);
            }
            if (model.Name != null)
            {
                result = result.Where(_ => _.Name.Contains(model.Name));
            }
            if (model.StartShoppingDate != null)
            {
                result = result.Where(_ => _.ShoppingDate >= model.StartShoppingDate);
            }
            if (model.EndShoppingDate != null)
            {
                result = result.Where(_ => _.ShoppingDate <= model.EndShoppingDate);
            }
            if (model.Status != null)
            {
                result = result.Where(_ => _.Status.Equals(model.Status));
            }

            return Ok(result.Count());
        }

        /// <summary>
        /// Create a new shopping list
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "name": "string",
        ///         "buildingId": 0,
        ///         "shoppingDate": "2021-09-19T16:20:10.084Z
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new shopping list</response>
        /// <response code="409">Shopping list already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateShoppingList([FromBody] ShoppingListCM model)
        {
            ResponseModel responseModel = new();

            var dataToInsert = _mapper.Map<ShoppingList>(model);
            try
            {
                dataToInsert.Status = Status.ACTIVE;
                await _service.AddAsync(dataToInsert);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return CreatedAtAction("CreateShoppingList", new { id = dataToInsert.Id }, dataToInsert);
        }

        /// <summary>
        /// Update shopping list with specified id
        /// </summary>
        /// <remarks>
        ///     PUT 
        ///     {
        ///         "name": "string",
        ///         "buildingId": 0,
        ///         "shoppingDate": "2021-09-19T16:20:10.084Z
        ///     }
        /// </remarks>
        /// <param name="id">Shopping list's id</param>
        /// <param name="model">Information applied to updated store</param>
        /// <response code="204">Update shopping list successfully</response>
        /// <response code="400">Shopping list's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Shopping list already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutShoppingList(int id, [FromBody] ShoppingListUM model)
        {
            ResponseModel responseModel = new();

            var dataToUpdate = await _service.GetByIdAsync(_ => _.Id == id);
            if (dataToUpdate == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingList));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            try
            {
                dataToUpdate.Name = model.Name;
                dataToUpdate.BuildingId = model.BuildingId;
                dataToUpdate.ShoppingDate = model.ShoppingDate;
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
        /// Delete shopping list with specified id
        /// </summary>
        /// <param name="id">Shopping list's id</param>
        /// <response code="204">Delete shopping list successfully</response>
        /// <response code="400">Shopping list's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            var dataToDelete = await _service.GetByIdAsync(_ => _.Id == id, _ => _.ShoppingItems);
            if (dataToDelete == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingList));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            try
            {
                // Delete all shopping items inside shopping list
                _shoppingItemService.DeleteRange(dataToDelete.ShoppingItems);

                // Then execute deleting shopping list
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

        /// <summary>
        /// Complete shopping list with specified id
        /// </summary>
        /// <param name="id">Shopping list's id</param>
        /// <response code="204">Complete shopping list successfully</response>
        /// <response code="400">Shopping list's id does not exist</response>
        /// <response code="500">Failed to complete</response>
        [HttpPut]
        [Route("{id}/complete")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CompleteShopping(int id)
        {
            ResponseModel responseModel = new();

            var dataToUpdate = await _service.GetByIdAsync(_ => _.Id == id, _ => _.ShoppingItems);
            if (dataToUpdate == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(ShoppingList));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            try
            {
                dataToUpdate.Status = Status.COMPLETE;
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
    }
}
