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

namespace IPSB.Controllers
{
    [Route("api/v1.0/shopping-lists")]
    [ApiController]
    // [Authorize(Roles = "Visitor")]
    public class ShoppingListController : Controller
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
            var result = _service.GetAll(_ => _.Building)
                                .Include(_ => _.ShoppingItems)
                                .ThenInclude(_ => _.Product)
                                .ThenInclude(_ => _.Store)
                                .ThenInclude(_ => _.Locations)
                                .AsSplitQuery()
                                .FirstOrDefault(_ => _.Id == id);
            if (result == null)
            {
                return NotFound();
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ShoppingListVM>> GetAllShoppingLists([FromQuery] ShoppingListSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var result = _service.GetAll(_ => _.Building);
            if (model.BuildingId > 0)
            {
                result.Where(_ => _.BuildingId == model.BuildingId);
            }
            if (model.AccountId > 0)
            {
                result.Where(_ => _.AccountId == model.AccountId);
            }
            if (model.Name != null)
            {
                result.Where(_ => _.Name.Contains(model.Name));
            }
            if (model.StartShoppingDate != null)
            {
                result.Where(_ => _.ShoppingDate >= model.StartShoppingDate);
            }
            if (model.EndShoppingDate != null)
            {
                result.Where(_ => _.ShoppingDate <= model.EndShoppingDate);
            }
            if (model.Status != null)
            {
                result.Where(_ => _.Status.Equals(model.Status));
            }
            var pagedModel = _pagingSupport.From(result)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<ShoppingListVM>();

            return Ok(pagedModel);
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
            var dataToInsert = _mapper.Map<ShoppingList>(model);
            try
            {
                dataToInsert.Status = Constants.Status.ACTIVE;
                await _service.AddAsync(dataToInsert);
                await _service.Save();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction("GetShoppingListById", new { id = dataToInsert.Id }, dataToInsert);
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
            var dataToUpdate = await _service.GetByIdAsync(_ => _.Id == id);
            if (dataToUpdate == null)
            {
                return NotFound();
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
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        // DELETE api/<ShoppingListController>/5
        // Change Status to Inactive
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var dataToDelete = await _service.GetByIdAsync(_ => _.Id == id);
            if (dataToDelete == null)
            {

                return NotFound();
            }
            try
            {
                dataToDelete.Status = Constants.Status.INACTIVE;
                _service.Update(dataToDelete);
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
