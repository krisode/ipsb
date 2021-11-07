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
using static IPSB.Utils.Constants;


namespace IPSB.Controllers
{
    [Route("api/v1.0/visit-points")]
    [ApiController]
    public class VisitStoreController : ControllerBase
    {
        private readonly IVisitStoreService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<VisitStore> _pagingSupport;
        // private readonly IAuthorizationService _authorizationService;
        public VisitStoreController(IVisitStoreService service, IMapper mapper, IPagingSupport<VisitStore> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }

        /// <summary>
        /// Get a specific visit store by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the visit store with the corresponding id</returns>
        /// <response code="200">Returns the visit store with the specified id</response>
        /// <response code="404">No visit stores found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<VisitStoreVM> GetVisitStoreById(int id)
        {
            ResponseModel responseModel = new();

            var visitStore = _service.GetByIdAsync(_ => _.Id == id, _ => _.Store.Building).Result;

            if (visitStore is null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(VisitStore));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }

            var rtnVisitStore = _mapper.Map<VisitStoreVM>(visitStore);

            return Ok(rtnVisitStore);
        }

        /// <summary>
        /// Get all visit stores
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
        /// <returns>All visit stores</returns>
        /// <response code="200">Returns all visit stores</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VisitStoreVM>> GetAllVisitStores([FromQuery] VisitStoreSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<VisitStore> list = _service.GetAll(_ => _.Store.Building);
            // IQueryable<VisitPoint> list = _service.GetAll(_ => _.VisitRoute)
            //                                         .Include(_ => _.Location)
            //                                         .ThenInclude(_ => _.Store)
            //                                         .ThenInclude(_ => _.Building);

            if (model.StoreId != 0)
            {
                list = list.Where(_ =>_.StoreId == model.StoreId);
            }
            
            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.Store.BuildingId == model.BuildingId);
            }

            if (model.LowerRecordTime.HasValue)
            {
                list = list.Where(_ => _.RecordTime >= model.LowerRecordTime);
            }

            if (model.UpperRecordTime.HasValue)
            {
                list = list.Where(_ => _.RecordTime <= model.UpperRecordTime);
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<VisitStoreVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Count visit stores
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
        /// <returns>Number of visit stores</returns>
        /// <response code="200">Returns number of visit stores</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VisitStoreVM>> CountVisitStores([FromQuery] VisitStoreSM model)
        {
            IQueryable<VisitStore> list = _service.GetAll(_ => _.Store.Building);
            
            if (model.StoreId != 0)
            {
                list = list.Where(_ =>_.StoreId == model.StoreId);
            }
            
            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.Store.BuildingId == model.BuildingId);
            }

            if (model.LowerRecordTime.HasValue)
            {
                list = list.Where(_ => _.RecordTime >= model.LowerRecordTime);
            }

            if (model.UpperRecordTime.HasValue)
            {
                list = list.Where(_ => _.RecordTime <= model.UpperRecordTime);
            }
           
            return Ok(list.Count());
        }

        /// <summary>
        /// Create a new visit store
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "StoreId": "Id of store",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Successfully created a new visit store</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VisitStoreCM>> CreateVisitStore([FromBody] VisitStoreCM model)
        {
            ResponseModel responseModel = new();

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            VisitStore crtVisitStore = _mapper.Map<VisitStore>(model);
            DateTime currentDateTime = localTime.DateTime;
            crtVisitStore.RecordTime = currentDateTime;

            try
            {
                await _service.AddAsync(crtVisitStore);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("GetVisitStoreById", new { id = crtVisitStore.Id }, crtVisitStore);
        }

        /// <summary>
        /// Update visit store with specified id
        /// </summary>
        /// <param name="id">Visit store's id</param>
        /// <param name="model">Information applied to updated visit store</param>
        /// <response code="204">Update visit store successfully</response>
        /// <response code="400">Visit store's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutVisitStore(int id, [FromBody] VisitStoreUM model)
        {
            ResponseModel responseModel = new();

            if (id != model.Id)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Id));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            VisitStore updVisitStore = await _service.GetByIdAsync(_ => _.Id == id);

            if (updVisitStore == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(VisitStore));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            try
            {
                updVisitStore.Id = model.Id;
                updVisitStore.StoreId = model.StoreId;
                updVisitStore.RecordTime = model.RecordTime.Value;

                _service.Update(updVisitStore);
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
        /// Delete visit store with specified id
        /// </summary>
        /// <param name="id">Visit store's id</param>
        /// <response code="204">Delete visit store successfully</response>
        /// <response code="404">Visit store's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            var dataToDelete = await _service.GetByIdAsync(_ => _.Id == id);
            if (dataToDelete == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(VisitStore));
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
