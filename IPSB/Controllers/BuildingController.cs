using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Cache;
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
using Newtonsoft.Json;
using static IPSB.Utils.Constants;
using System.Linq.Expressions;

namespace IPSB.Controllers
{
    [Route("api/v1.0/buildings")]
    [ApiController]
    [Authorize(Roles = "Admin, Building Manager")]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _service;
        private readonly ILocatorTagService _locatorTagService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Building> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheStore _cacheStore;
        public BuildingController(IBuildingService service, ILocatorTagService locatorTagService, IMapper mapper, IPagingSupport<Building> pagingSupport,
            IUploadFileService uploadFileService, IAuthorizationService authorizationService, ICacheStore cacheStore)
        {
            _service = service;
            _locatorTagService = locatorTagService;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _cacheStore = cacheStore;
        }

        /// <summary>
        /// Get a specific building by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the building with the corresponding id</returns>
        /// <response code="200">Returns the building with the specified id</response>
        /// <response code="404">No buildings found with the specified id</response>
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingVM>> GetBuildingById(int id)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Building>(id);
            var cacheObjectType = new Building();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var building = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var building = _service.GetByIdAsync(_ => _.Id == id, _ => _.Manager).Result;

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(building);

                }, ifModifiedSince);


                if (building == null)
                {
                    responseModel.Code = StatusCodes.Status404NotFound;
                    responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                    responseModel.Type = ResponseType.NOT_FOUND;

                    return NotFound(responseModel);
                }

                /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Read);
                if (!authorizedResult.Succeeded)
                {
                    return new ObjectResult($"Not authorize to access building with id: {id}") { StatusCode = 403 };
                }*/

                var rtnBuilding = _mapper.Map<BuildingVM>(building);

                return Ok(rtnBuilding);
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
        /// Get all buildings
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
        /// <returns>All buildings</returns>
        /// <response code="200">Returns all buildings</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BuildingVM>>> GetAllBuildings([FromQuery] BuildingSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Building>(DefaultValue.INTEGER);
            var cacheObjectType = new Building();
            string ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var list = _service.GetAll(_ => _.Manager);

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(list);

                }, setLastModified: (cachedTime) =>
                {
                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedTime);
                    return cachedTime;
                }, ifModifiedSince);


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
                if (model.ManagerId >= 0)
                {
                    list = list.Where(_ => _.ManagerId == model.ManagerId);
                }


                if (!string.IsNullOrEmpty(model.Name))
                {
                    list = list.Where(_ => _.Name.Contains(model.Name));
                }

                if (!string.IsNullOrEmpty(model.Address))
                {
                    list = list.Where(_ => _.Address.Contains(model.Address));
                }

                if (model.FindCurrentBuilding && model.Lat != 0 && model.Lng != 0)
                {
                    list = list.Where(_ => IndoorPositioningContext.DistanceBetweenLatLng(_.Lat, _.Lng, model.Lat, model.Lng) < 0.5);
                }

                Expression<Func<Building, object>> sorter = _ => _.Id;

                bool includeDistanceTo = model.Lat != 0 && model.Lng != 0;
                if (includeDistanceTo)
                {
                    sorter = _ => IndoorPositioningContext.DistanceBetweenLatLng(_.Lat, _.Lng, model.Lat, model.Lng);
                }

                Func<BuildingVM, Building, BuildingVM> transformData = (buildingVM, building) =>
                {
                    if (includeDistanceTo)
                    {
                        double fromLat = building.Lat;
                        double fromLng = building.Lng;
                        double toLat = model.Lat;
                        double toLng = model.Lng;
                        buildingVM.DistanceTo = HelperFunctions.DistanceBetweenLatLng(fromLat, fromLng, toLat, toLng);
                    }
                    return buildingVM;
                };

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, sorter, isAll, isAscending)
                    .Paginate<BuildingVM>(transform: transformData);

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
        /// Count buildings
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
        /// <returns>Number of buildings</returns>
        /// <response code="200">Returns number of buildings</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BuildingVM>>> CountBuildings([FromQuery] BuildingSM model)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Building>(Utils.Constants.DefaultValue.INTEGER);
            var cacheObjectType = new Building();
            string ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var list = _service.GetAll(_ => _.Manager);

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(list);

                }, setLastModified: (cachedTime) =>
                {
                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedTime);
                    return cachedTime;
                }, ifModifiedSince);


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

                if (model.ManagerId >= 0)
                {
                    list = list.Where(_ => _.ManagerId == model.ManagerId);
                }


                if (!string.IsNullOrEmpty(model.Name))
                {
                    list = list.Where(_ => _.Name.Contains(model.Name));
                }

                if (!string.IsNullOrEmpty(model.Address))
                {
                    list = list.Where(_ => _.Address.Contains(model.Address));
                }


                return Ok(list.Count());
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
        /// Create a new building
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "ManagerId": "Id of the manager in charge of the building",   
        ///         "AdminId": "Id of the admin in charge of the building",   
        ///         "Name": "Name of the building",   
        ///         "ImageUrl": "Image of the building",   
        ///         "NumberOfFloor": "Number of floors in the building",   
        ///         "Address": "Address of the buildings",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new building</response>
        /// <response code="409">Building already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BuildingCM>> CreateBuilding([FromForm] BuildingCM model)
        {
            ResponseModel responseModel = new();

            bool isDuplicate = _service.GetAll()
                                        .Where(_ => _.Name.ToLower().Equals(model.Name.ToLower()) || _.ManagerId == model.ManagerId)
                                        .Count() >= 1;
            if (isDuplicate)
            {

                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", nameof(Building) + " " + model.Name + " or " + " Manager with id {" + model.ManagerId.ToString() + "}");
                responseModel.Type = ResponseType.INVALID_REQUEST;

                return Conflict(responseModel);
            }


            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create building") { StatusCode = 403 };
            }*/


            Building crtBuilding = _mapper.Map<Building>(model);
            var addressJson = JsonConvert.DeserializeObject<AddressJson>(model.AddressJson);
            crtBuilding.Address = addressJson.Address;
            crtBuilding.Lat = addressJson.Lat;
            crtBuilding.Lng = addressJson.Lng;

            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "building", "building-detail");
            crtBuilding.ImageUrl = imageURL;

            // Default POST Status = "Active"
            crtBuilding.Status = Constants.Status.ACTIVE;
            try
            {
                await _service.AddAsync(crtBuilding);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("GetBuildingById", new { id = crtBuilding.Id }, crtBuilding);
        }

        /// <summary>
        /// Update building with specified id
        /// </summary>
        /// <param name="id">Building's id</param>
        /// <param name="model">Information applied to updated building</param>
        /// <response code="204">Update building successfully</response>
        /// <response code="400">Building's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Building already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutBuilding(int id, [FromForm] BuildingUM model)
        {
            ResponseModel responseModel = new();

            #region Get building by ID
            Building updBuilding = await _service.GetByIdAsync(_ => _.Id == id);
            #endregion

            if (updBuilding is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Building));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            #region Authorization(Role = "Building Manager, Admin")
            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updBuilding, Operations.Update);

            if (!authorizedResult.Succeeded)
            {
                responseModel.Code = StatusCodes.Status403Forbidden;
                responseModel.Message = ResponseMessage.UNAUTHORIZE_UPDATE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Forbid(responseModel.ToString());
            }
            #endregion



            #region If building has image, set it as new image in case inputted image request is null
            string imageURL = updBuilding.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "building", "building-detail");
            }
            #endregion

            #region Updating building
            try
            {

                updBuilding.ManagerId = model.ManagerId;
                updBuilding.Name = model.Name;
                updBuilding.ImageUrl = imageURL;
                var addressJson = JsonConvert.DeserializeObject<AddressJson>(model.AddressJson);
                updBuilding.Address = addressJson.Address;
                updBuilding.Lat = addressJson.Lat;
                updBuilding.Lng = addressJson.Lng;

                _service.Update(updBuilding);
                if (await _service.Save() > 0)
                {
                    // #region Updating cache
                    // var cacheId = new CacheKey<Building>(id);
                    // await _cacheStore.Remove(cacheId);
                    // #endregion
                }

            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            #endregion

            // Success
            return NoContent();
        }

        /// <summary>
        /// Change the status of building to inactive
        /// </summary>
        /// <param name="id">Coupon's id</param>
        /// <response code="204">Delete building's status successfully</response>
        /// <response code="400">Building's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();
            Building building = await _service.GetByIdAsync(_ => _.Id == id);

            if (building is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Building));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/


            if (building.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(Building));
                responseModel.Type = ResponseType.INVALID_REQUEST;

                return BadRequest(responseModel);
            }

            building.Status = Status.INACTIVE;
            try
            {
                _service.Update(building);
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
