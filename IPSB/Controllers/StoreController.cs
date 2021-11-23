using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Cache;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/stores")]
    [ApiController]
    [Authorize(Roles = "Visitor, Building Manager, Store Owner")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _service;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Store> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILocationService _locationService;
        private readonly ICacheStore _cacheStore;

        public StoreController(IStoreService service, IProductCategoryService productCategoryService, IMapper mapper,
            IPagingSupport<Store> pagingSupport, IUploadFileService uploadFileService, IAuthorizationService authorizationService,
            ILocationService locationService, ICacheStore cacheStore)
        {
            _service = service;
            _productCategoryService = productCategoryService;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _locationService = locationService;
            _cacheStore = cacheStore;
        }


        /// <summary>
        /// Get a specific store by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the store with the corresponding id</returns>
        /// <response code="200">Returns the store with the specified id</response>
        /// <response code="404">No stores found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreVM>> GetStoreById(int id)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Store>(id);
            var cacheObjectType = new Store();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var store = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var store = _service.GetByIdAsync(_ => _.Id == id, _ => _.Account, _ => _.Building, _ => _.FloorPlan).Result;

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(store);

                }, ifModifiedSince);

                if (store == null)
                {
                    responseModel.Code = StatusCodes.Status404NotFound;
                    responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Store));
                    responseModel.Type = ResponseType.NOT_FOUND;
                    return NotFound(responseModel);
                }


                var rtnStore = _mapper.Map<StoreVM>(store);

                return Ok(rtnStore);
            }
            catch (Exception e)
            {
                if (e.Message.Equals(ExceptionMessage.NOT_MODIFIED))
                {
                    responseModel.Code = StatusCodes.Status304NotModified;
                    responseModel.Message = ResponseMessage.NOT_MODIFIED;
                    responseModel.Type = ResponseType.NOT_MODIFIED;
                    return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status304NotModified };
                }

                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_READ;
                responseModel.Type = ResponseType.CAN_NOT_READ;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };

            }
        }

        /// <summary>
        /// Get all stores
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
        /// <returns>All stores</returns>
        /// <response code="200">Returns all stores</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StoreVM>>> GetAllStores([FromQuery] StoreSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Store>(DefaultValue.INTEGER);
            var cacheObjectType = new Store();
            string ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];
            bool includeDistanceToBuilding = model.Lat != 0 && model.Lng != 0;
            try
            {
                var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var list = _service.GetAll(_ => _.Account, _ => _.Building, _ => _.FloorPlan, _ => _.Location);

                    if (includeDistanceToBuilding)
                    {
                        list = list.OrderBy(_ => IndoorPositioningContext.DistanceBetweenLatLng(_.Building.Lat, _.Building.Lng, model.Lat, model.Lng));
                    }
                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(list);

                }, setLastModified: (cachedTime) =>
                {
                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedTime);
                    return cachedTime;
                }, ifModifiedSince);


                if (model.AccountId != 0)
                {
                    list = list.Where(_ => _.AccountId == model.AccountId);
                }

                if (model.BuildingId != 0)
                {
                    list = list.Where(_ => _.BuildingId == model.BuildingId);
                }

                if (model.FloorPlanId != 0)
                {
                    list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
                }

                if (!string.IsNullOrEmpty(model.Name))
                {
                    list = list.Where(_ => _.Name.Contains(model.Name));
                }

                if (!string.IsNullOrEmpty(model.Description))
                {
                    list = list.Where(_ => _.Description.Contains(model.Description));
                }

                if (!string.IsNullOrEmpty(model.Phone))
                {
                    list = list.Where(_ => _.Phone.Contains(model.Phone));
                }

                //Cache disabled
                // if (model.ProductCategoryIds is not null && model.ProductCategoryIds.Length > 0)
                // {
                //     list = list.Where(store => store.Products.Any(product => model.ProductCategoryIds.Contains(product.ProductCategoryId)));
                // }

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
                        list = list.Where(_ => _.Status == model.Status);
                    }
                }

                Func<StoreVM, Store, StoreVM> transformData = (storeVM, store) =>
                {

                    if (includeDistanceToBuilding)
                    {
                        double fromLat = store.Building.Lat;
                        double fromLng = store.Building.Lng;
                        double toLat = model.Lat;
                        double toLng = model.Lng;
                        storeVM.Building.Name = store.Building.Name;
                        storeVM.Building.DistanceTo = HelperFunctions.DistanceBetweenLatLng(fromLat, fromLng, toLat, toLng);
                    }
                    return storeVM;
                };

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending, random: model.Random, noSort: includeDistanceToBuilding)
                    .Paginate<StoreVM>(transform: transformData);


                return Ok(pagedModel);
            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    responseModel.Code = StatusCodes.Status304NotModified;
                    responseModel.Message = ResponseMessage.NOT_MODIFIED;
                    responseModel.Type = ResponseType.NOT_MODIFIED;
                    return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status304NotModified };

                }
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_READ;
                responseModel.Type = ResponseType.CAN_NOT_READ;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

        }

        /// <summary>
        /// Count stores
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
        /// <returns>Number of stores</returns>
        /// <response code="200">Returns number of stores</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<StoreVM>> CountStores([FromQuery] StoreSM model)
        {
            ResponseModel responseModel = new();

            IQueryable<Store> list = _service.GetAll(_ => _.Account, _ => _.Building, _ => _.FloorPlan, _ => _.Location);

            if (model.AccountId != 0)
            {
                list = list.Where(_ => _.AccountId == model.AccountId);
            }

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.BuildingId == model.BuildingId);
            }

            if (model.FloorPlanId != 0)
            {
                list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                list = list.Where(_ => _.Description.Contains(model.Description));
            }

            if (!string.IsNullOrEmpty(model.Phone))
            {
                list = list.Where(_ => _.Phone.Contains(model.Phone));
            }

            if (model.ProductCategoryIds is not null && model.ProductCategoryIds.Length > 0)
            {
                list = list.Where(store => store.Products.Any(product => model.ProductCategoryIds.Contains(product.ProductCategoryId)));
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
                    list = list.Where(_ => _.Status == model.Status);
                }
            }


            return Ok(list.Count());
        }

        /// <summary>
        /// Create a new store
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Name": "Name of the store",     
        ///         "AccountId": "Id of the store owner's account",           
        ///         "ImageUrl": "List of image of the store",          
        ///         "BuildingId": "Id of the building that the store belongs to",
        ///         "Description": "General description of the store",
        ///         "ProductCategoryIds": "List of ids of product category that belongs to the store",
        ///         "Phone": "Phone number of the store owner",
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new store</response>
        /// <response code="409">Store already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StoreCM>> CreateStore([FromForm] StoreCM model)
        {
            ResponseModel responseModel = new();
            bool isExisted = false;
            isExisted = _service.IsExisted(_ => _.Name.ToLower() == model.Name.ToLower() && _.BuildingId == model.BuildingId);
            if (isExisted)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Name);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            isExisted = _service.IsExisted(_ => _.Phone == model.Phone);
            if (isExisted)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Phone);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }


            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, store, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create store") { StatusCode = 403 };
            }*/
            // Default POST Status = "Active" of Location

            Store crtStore = _mapper.Map<Store>(model);

            // Default POST Status = "Active"
            crtStore.Status = Status.ACTIVE;
            crtStore.LocationId = await _locationService.CreateLocationJson(model.LocationJson);


            if (model.ImageUrl != null)
            {
                crtStore.ImageUrl = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "store", "store-detail");
            }

            try
            {

                await _service.AddAsync(crtStore);
                if (await _service.Save() > 0)
                {
                    await Task.WhenAll(
                        _cacheStore.Remove<Store>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Edge>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Location>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Coupon>(DefaultValue.INTEGER)
                    );
                }
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("GetStoreById", new { id = crtStore.Id }, crtStore);
        }

        /// <summary>
        /// Update store with specified id
        /// </summary>
        /// <param name="id">Store's id</param>
        /// <param name="model">Information applied to updated store</param>
        /// <response code="204">Update store successfully</response>
        /// <response code="400">Store's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Store already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutStore(int id, [FromForm] StoreUM model)
        {
            ResponseModel responseModel = new();

            if (_service.IsExisted(_ => _.Id != id && _.Name.ToLower().Equals(model.Name.ToLower())))
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Name);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            bool isExisted = _service.IsExisted(_ => _.Phone == model.Phone && _.Id != id);
            if (isExisted)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Phone);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            Store updStore = await _service.GetByIdAsync(_ => _.Id == id);

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updStore, Operations.Update);
            if (!authorizedResult.Succeeded)
            {
                responseModel.Code = StatusCodes.Status403Forbidden;
                responseModel.Message = ResponseMessage.UNAUTHORIZE_UPDATE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Forbid(responseModel.ToString());
            }

            if (model.ImageUrl != null)
            {
                updStore.ImageUrl = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "store", "store-detail");
            }

            try
            {

                updStore.Name = model.Name;
                updStore.AccountId = model.AccountId;
                updStore.Description = model.Description;
                updStore.FloorPlanId = model.FloorPlanId;
                updStore.Phone = model.Phone;
                updStore.LocationId = await _locationService.UpdateLocationJson(updStore.LocationId, model.LocationJson);
                _service.Update(updStore);
                if (await _service.Save() > 0)
                {
                    await Task.WhenAll(
                        _cacheStore.Remove<Store>(id),
                        _cacheStore.Remove<Edge>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Location>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Coupon>(DefaultValue.INTEGER)
                    );
                }
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
        /// Change the status of store to inactive
        /// </summary>
        /// <param name="id">Store's id</param>
        /// <response code="204">Delete store successfully</response>
        /// <response code="400">Store's id does not exist</response>
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

            var store = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (store is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Store));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (store.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(Store));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            store.Status = Status.INACTIVE;
            store.AccountId = null;
            try
            {
                _service.Update(store);
                if (await _service.Save() > 0)
                {
                    await Task.WhenAll(
                        _cacheStore.Remove<Store>(id),
                        _cacheStore.Remove<Edge>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Location>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Coupon>(DefaultValue.INTEGER)
                    );
                }
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
