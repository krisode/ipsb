using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IPSB.Cache;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPSB.Controllers
{
    [Route("api/v1.0/coupon-types")]
    [ApiController]
    public class CouponTypeController : ControllerBase
    {
        private readonly ICouponTypeService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<CouponType> _pagingSupport;
        private readonly ICacheStore _cacheService;

        public CouponTypeController(ICouponTypeService service, IMapper mapper, IPagingSupport<CouponType> pagingSupport, ICacheStore cacheService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _cacheService = cacheService;
        }



        /// <summary>
        /// Get a specific coupon type by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the coupon type with the corresponding id</returns>
        /// <response code="200">Returns the coupon type with the specified id</response>
        /// <response code="404">No coupon type found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCouponTypeById(int id)
        {
            ResponseModel responseModel = new();

            var result = await _service.GetByIdAsync(_ => _.Id == id);

            if (result == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = Constants.ResponseMessage.NOT_FOUND.Replace("Object", nameof(CouponType));
                responseModel.Type = Constants.ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }

            var rtnCouponType = _mapper.Map<CouponTypeVM>(result);
            return Ok(rtnCouponType);
        }

        /// <summary>
        /// Get all coupon types
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET 
        ///     {
        ///         "name": "Fixed",
        ///         "description": "Discount total price"
        ///     }
        ///
        /// </remarks>
        /// <returns>All coupon types</returns>
        /// <response code="200">Returns all coupon types</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetAllCouponTypes([FromQuery] CouponTypeSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var couponTypeList = _service.GetAll();

            if (!string.IsNullOrEmpty(model.Name))
            {
                couponTypeList = couponTypeList.Where(_ => _.Name.Contains(model.Name));
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                couponTypeList = couponTypeList.Where(_ => _.Description.Contains(model.Description));
            }
            if (!string.IsNullOrEmpty(model.Status))
            {
                couponTypeList = couponTypeList.Where(_ => _.Status.Equals(model.Status));
            }

            var pagedModel = _pagingSupport.From(couponTypeList)
                                            .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                                            .Paginate<CouponTypeVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Count coupon types
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
        /// <returns>Number of coupon types</returns>
        /// <response code="200">Returns number of coupon types</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult CountCouponTypes([FromQuery] CouponTypeSM model)
        {
            var couponTypeList = _service.GetAll();

            if (!string.IsNullOrEmpty(model.Name))
            {
                couponTypeList = couponTypeList.Where(_ => _.Name.Contains(model.Name));
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                couponTypeList = couponTypeList.Where(_ => _.Description.Contains(model.Description));
            }
            if (!string.IsNullOrEmpty(model.Status))
            {
                couponTypeList = couponTypeList.Where(_ => _.Status.Equals(model.Status));
            }


            return Ok(couponTypeList.Count());
        }

        /// <summary>
        /// Create a new coupon type
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "name": "Coupon type's name",   
        ///         "description": "Coupon type's description",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created new coupon type</response>
        /// <response code="400">Required create data is missing</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CouponTypeVM>> CreateCouponType([FromBody] CouponTypeCM model)
        {
            ResponseModel responseModel = new();

            var createdCouponType = _mapper.Map<CouponType>(model);
            bool isExisted = _service.GetAll().Where(_ => _.Name.ToLower().Equals(model.Name)).Count() >= 1;
            if (isExisted)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = Constants.ResponseMessage.DUPLICATED.Replace("Object", model.Name);
                responseModel.Type = Constants.ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }
            try
            {
                createdCouponType.Status = Constants.Status.ACTIVE;
                await _service.AddAsync(createdCouponType);
                if (await _service.Save() > 0)
                {
                    await _cacheService.Remove<Coupon>(Constants.DefaultValue.INTEGER);
                }
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = Constants.ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = Constants.ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("CreateCouponType", new { id = createdCouponType.Id }, createdCouponType);
        }

        /// <summary>
        /// Update coupon type with specified id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT 
        ///     {
        ///         "name": "Coupon type's name",   
        ///         "description": "Coupon type's description",   
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Coupon type's id</param>
        /// <param name="model">Information applied to updated coupon type</param>
        /// <response code="204">Update coupon type successfully</response>
        /// <response code="400">Required update data is missing</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateCouponType(int id, [FromBody] CouponTypeUM model)
        {
            ResponseModel responseModel = new();

            bool isExisted = _service.GetAll().Where(_ => _.Name.ToLower().Equals(model.Name) && id != _.Id).Count() >= 1;
            if (isExisted)
            {
                return Conflict();
            }

            var updateCouponType = await _service.GetByIdAsync(_ => _.Id == id);

            if (updateCouponType is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = Constants.ResponseMessage.NOT_FOUND.Replace("Object", nameof(CouponType));
                responseModel.Type = Constants.ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            try
            {
                updateCouponType.Name = model.Name;
                updateCouponType.Description = model.Description;
                _service.Update(updateCouponType);
                if (await _service.Save() > 0)
                {
                    await _cacheService.Remove<Coupon>(Constants.DefaultValue.INTEGER);
                }
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = Constants.ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = Constants.ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return NoContent();
        }

        /// <summary>
        /// Delete coupon type with specified id
        /// </summary>
        /// <param name="id">Coupon type's id</param>
        /// <response code="204">Delete coupon type successfully</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            var deleteCouponType = await _service.GetByIdAsync(_ => _.Id == id);

            if (deleteCouponType is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = Constants.ResponseMessage.NOT_FOUND.Replace("Object", nameof(CouponType));
                responseModel.Type = Constants.ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (deleteCouponType.Status.Equals(Constants.Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = Constants.ResponseMessage.DELETED.Replace("Object", nameof(CouponType));
                responseModel.Type = Constants.ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            try
            {
                deleteCouponType.Status = Constants.Status.INACTIVE;
                _service.Update(deleteCouponType);
                if (await _service.Save() > 0)
                {
                    await _cacheService.Remove<Coupon>(Constants.DefaultValue.INTEGER);
                }
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = Constants.ResponseMessage.CAN_NOT_DELETE;
                responseModel.Type = Constants.ResponseType.CAN_NOT_DELETE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return NoContent();
        }
    }

}