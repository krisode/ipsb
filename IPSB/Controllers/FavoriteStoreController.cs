using AutoMapper;
using IPSB.Core.Services;
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
    [Route("api/v1.0/favorite-stores")]
    [ApiController]
    public class FavoriteStoreController : AuthorizeController
    {
        private readonly IFavoriteStoreService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<FavoriteStore> _pagingSupport;
        // private readonly IAuthorizationService _authorizationService;

        public FavoriteStoreController(IFavoriteStoreService service, IMapper mapper, IPagingSupport<FavoriteStore> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }

        /// <summary>
        /// Get a specific favorite store by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the favorite store with the corresponding id</returns>
        /// <response code="200">Returns the favorite store with the specified id</response>
        /// <response code="404">No favorite stores found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<FavoriteStoreVM> GetFavoriteStoreById(int id)
        {
            var favoriteStore = _service.GetByIdAsync(_ => _.Id == id, _ => _.Store, _ => _.Store.Building).Result;

            if (favoriteStore is null)
            {
                return NotFound();
            }

            var rtnFavoriteStore = _mapper.Map<FavoriteStoreVM>(favoriteStore);

            return Ok(rtnFavoriteStore);
        }

        /// <summary>
        /// Get all favorite stores
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
        /// <returns>All favorite stores</returns>
        /// <response code="200">Returns all favorite stores</response>
        /// <response code="404">No favorite stores found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<FavoriteStoreVM>> GetAllFavoriteStores([FromQuery] FavoriteStoreSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<FavoriteStore> list = _service.GetAll(_ => _.Store, _ => _.Store.Building);

            if (model.StoreId != 0)
            {
                list = list.Where(_ => _.StoreId == model.StoreId);
            }

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.BuildingId == model.BuildingId);
            }

            if (model.LowerRecordDate.HasValue)
            {
                list = list.Where(_ => _.RecordDate >= model.LowerRecordDate);
            }

            if (model.UpperRecordDate.HasValue)
            {
                list = list.Where(_ => _.RecordDate <= model.UpperRecordDate);
            }

            if (model.LowerVisitCount > 0)
            {
                list = list.Where(_ => _.VisitCount >= model.LowerVisitCount);
            }

            if (model.UpperVisitCount > 0)
            {
                list = list.Where(_ => _.VisitCount <= model.UpperVisitCount);
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<FavoriteStoreVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new favorite store
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "StoreId": "Id of the store which is favorite",   
        ///         "BuildingId": "Id of the visit building that the route belongs to",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new favorite stores</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FavoriteStoreCM>> CreateFavoriteStore([FromBody] FavoriteStoreCM model)
        {

            FavoriteStore crtFavoriteStore = _mapper.Map<FavoriteStore>(model);
            DateTime currentDateTime = DateTime.Now;
            crtFavoriteStore.RecordDate = currentDateTime;
            crtFavoriteStore.VisitCount = 1;

            try
            {
                await _service.AddAsync(crtFavoriteStore);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetFavoriteStoreById", new { id = crtFavoriteStore.Id }, crtFavoriteStore);
        }

        /// <summary>
        /// Update favorite store with specified id
        /// </summary>
        /// <param name="id">Favorite store's id</param>
        /// <param name="model">Information applied to updated favorite store</param>
        /// <response code="204">Update favorite store successfully</response>
        /// <response code="400">Favorite store's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutFavoriteStore(int id, [FromBody] FavoriteStoreUM model)
        {

            FavoriteStore updFavoriteStore = await _service.GetByIdAsync(_ => _.Id == id);

            if (updFavoriteStore == null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                updFavoriteStore.Id = model.Id;
                updFavoriteStore.StoreId = model.StoreId;
                updFavoriteStore.BuildingId = model.BuildingId;
                updFavoriteStore.VisitCount = model.VisitCount;
                updFavoriteStore.RecordDate = model.RecordDate.Value;

                _service.Update(updFavoriteStore);
                await _service.Save();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return NoContent();
        }

        /*/// <summary>
        /// Change the status of favorite store to inactive
        /// </summary>
        /// <param name="id">Coupon's id</param>
        /// <response code="204">Update coupon's status successfully</response>
        /// <response code="400">Coupon's id does not exist</response>
        /// <response code="500">Failed to update</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]*/
        // Future Plan
        public void Delete(int id)
        {

        }
        protected override bool IsAuthorize()
        {
            throw new NotImplementedException();
        }
    }
}
