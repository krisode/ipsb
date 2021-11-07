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
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/coupons")]
    [ApiController]
    // [Authorize(Roles = "Building Manager, Store Owner, Visitor")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _service;
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Coupon> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;

        public CouponController(ICouponService service, INotificationService notificationService, IPushNotificationService pushNotificationService,
            IMapper mapper, IPagingSupport<Coupon> pagingSupport, IUploadFileService uploadFileService, IAuthorizationService authorizationService)
        {
            _service = service;
            _notificationService = notificationService;
            _pushNotificationService = pushNotificationService;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Get a specific coupon by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the coupon with the corresponding id</returns>
        /// <response code="200">Returns the coupon with the specified id</response>
        /// <response code="404">No coupons found with the specified id</response>
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<CouponVM> GetCouponById(int id)
        {
            ResponseModel responseModel = new();

            var coupon = _service.GetByIdAsync(_ => _.Id == id, _ => _.Store, _ => _.CouponInUses).Result;

            if (coupon == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Coupon));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, coupon, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to access coupon with id: {id}");
            }*/

            var rtnCoupon = _mapper.Map<CouponVM>(coupon);

            return Ok(rtnCoupon);
        }

        /// <summary>
        /// Get all coupons
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
        /// <returns>All coupons</returns>
        /// <response code="200">Returns all coupons</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CouponVM>> GetAllCoupons([FromQuery] CouponSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            IQueryable<Coupon> list = _service.GetAll(_ => _.Store, _ => _.CouponInUses, _ => _.CouponType);

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.Store.BuildingId == model.BuildingId);
            }
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

            if (!string.IsNullOrEmpty(model.Code))
            {
                list = list.Where(_ => _.Code.Contains(model.Code));
            }

            if (model.CouponTypeId > 0)
            {
                list = list.Where(_ => _.CouponTypeId == model.CouponTypeId);
            }

            if (model.LowerPublishDate.HasValue)
            {
                list = list.Where(_ => _.PublishDate >= model.LowerPublishDate);
            }

            if (model.UpperPublishDate.HasValue)
            {
                list = list.Where(_ => _.PublishDate <= model.UpperPublishDate);
            }

            if (model.LowerExpireDate.HasValue)
            {
                list = list.Where(_ => _.ExpireDate >= model.LowerExpireDate);
            }

            if (model.UpperExpireDate.HasValue)
            {
                list = list.Where(_ => _.ExpireDate <= model.UpperExpireDate);
            }

            if (model.LowerAmount != 0)
            {
                list = list.Where(_ => _.Amount >= model.LowerAmount);
            }

            if (model.UpperAmount != 0)
            {
                list = list.Where(_ => _.Amount <= model.UpperAmount);
            }

            if (model.MaxDiscount != 0)
            {
                list = list.Where(_ => _.MaxDiscount == model.MaxDiscount);
            }

            if (model.MinSpend != 0)
            {
                list = list.Where(_ => _.MinSpend <= model.MinSpend);
            }


            if (model.LowerLimit != 0)
            {
                list = list.Where(_ => _.Limit >= model.LowerLimit);
            }

            if (model.UpperLimit != 0)
            {
                list = list.Where(_ => _.Limit <= model.UpperLimit);
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
                .Paginate<CouponVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Count coupons
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
        /// <returns>Number of coupons</returns>
        /// <response code="200">Returns number of coupons</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CouponVM>> CountCoupons([FromQuery] CouponSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            IQueryable<Coupon> list = _service.GetAll(_ => _.Store, _ => _.CouponInUses, _ => _.CouponType);

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.Store.BuildingId == model.BuildingId);
            }
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

            if (!string.IsNullOrEmpty(model.Code))
            {
                list = list.Where(_ => _.Code.Contains(model.Code));
            }

            if (model.CouponTypeId > 0)
            {
                list = list.Where(_ => _.CouponTypeId == model.CouponTypeId);
            }

            if (model.LowerPublishDate.HasValue)
            {
                list = list.Where(_ => _.PublishDate >= model.LowerPublishDate);
            }

            if (model.UpperPublishDate.HasValue)
            {
                list = list.Where(_ => _.PublishDate <= model.UpperPublishDate);
            }

            if (model.LowerExpireDate.HasValue)
            {
                list = list.Where(_ => _.ExpireDate >= model.LowerExpireDate);
            }

            if (model.UpperExpireDate.HasValue)
            {
                list = list.Where(_ => _.ExpireDate <= model.UpperExpireDate);
            }

            if (model.LowerAmount != 0)
            {
                list = list.Where(_ => _.Amount >= model.LowerAmount);
            }

            if (model.UpperAmount != 0)
            {
                list = list.Where(_ => _.Amount <= model.UpperAmount);
            }

            if (model.MaxDiscount != 0)
            {
                list = list.Where(_ => _.MaxDiscount == model.MaxDiscount);
            }

            if (model.MinSpend != 0)
            {
                list = list.Where(_ => _.MinSpend <= model.MinSpend);
            }


            if (model.LowerLimit != 0)
            {
                list = list.Where(_ => _.Limit >= model.LowerLimit);
            }

            if (model.UpperLimit != 0)
            {
                list = list.Where(_ => _.Limit <= model.UpperLimit);
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
        /// Create a new coupon
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {            
        ///         "ImageUrl": "List of image of the coupon", 
        ///         "Name": "Name of the coupon",     
        ///         "Description": "General description of the coupon",    
        ///         "StoreId": "Id of the store which the coupon belongs to",
        ///         "Code": "Code of the coupon",
        ///         "DiscountType":"Type of the coupon is used for",
        ///         "PublishDate": "The date time that the coupon is valid",
        ///         "ExpireDate": "The date time that the coupon expires",
        ///         "Amount": "A specific rate or amount is reduced when a customer uses the coupon",
        ///         "MaxDiscount": "The maximum amount that customers can reduce when using coupon",
        ///         "MinSpend": "The minimum amount that customers have to spend to use the coupon",
        ///         "ProductInclude": "The date time that the coupon expires",
        ///         "ProductExclude": "The date time that the coupon expires",
        ///         "Limit": "Number of customers who can use the coupon",
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new coupon</response>
        /// <response code="409">Coupon already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CouponCM>> CreateCoupon([FromForm] CouponCM model)
        {
            ResponseModel responseModel = new();

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            // Created coupon has publish date less than current date
            if (model.PublishDate < localTime.DateTime)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.PublishDate));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            // Created coupon has expired date less than current date
            if (model.ExpireDate < localTime.DateTime)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.ExpireDate));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            // Created coupon has publish date less than expired date
            if (model.PublishDate.Value.AddHours(2) > model.ExpireDate)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.PublishDate) + " must be larger 2 hours than " + nameof(model.ExpireDate));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            // CASE: Created coupon has coupon type as FIXED
            if (model.CouponTypeId == Constants.CouponType.FIXED)
            {
                // CASE-1: Created coupon does not has value for amount field 
                if (!model.Amount.HasValue)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Amount));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }
                // CASE-2: Created coupon has value for max discount field 
                if (model.MaxDiscount.HasValue)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.MaxDiscount));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }
            }

            // CASE: Coupon has coupon type as PERCENTAGE
            if (model.CouponTypeId == Constants.CouponType.PERCENTAGE)
            {
                // CASE-1: Created coupon does not has value for max discount field 
                if (!model.MaxDiscount.HasValue)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.MaxDiscount));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }

                // CASE-2: Created coupon has value for amount field 
                if (!model.Amount.HasValue)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Amount));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }
            }


            // Get coupon with the same code at the same store
            var coupon = _service.GetAll().Where(_ => _.Code == model.Code).Where(_ => _.StoreId == model.StoreId).FirstOrDefault();

            // CASE: Coupon with the same code at the same store do exist
            if (coupon is not null)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Code);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel.ToString());
            }

            Coupon crtCoupon = _mapper.Map<Coupon>(model);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, crtCoupon, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create coupon") { StatusCode = 403 };
            }*/

            // Default POST Status = "ACTIVE"
            crtCoupon.Status = Constants.Status.ACTIVE;

            string imageUrl = "";

            if (model.ImageUrl is not null && model.ImageUrl.Count > 0)
            {
                List<string> imageUrls = new List<string>();
                foreach (var url in model.ImageUrl)
                {
                    imageUrl = await _uploadFileService.UploadFile("123456798", url, "coupon", "coupon-detail");
                    imageUrls.Add(imageUrl);
                }
                imageUrl = string.Join(",", imageUrls);
            }

            crtCoupon.ImageUrl = imageUrl;

            try
            {
                await _service.AddAsync(crtCoupon);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };

            }

            return CreatedAtAction("GetCouponById", new { id = crtCoupon.Id }, crtCoupon);
        }

        /// <summary>
        /// Update coupon with specified id
        /// </summary>
        /// <param name="id">Coupon's id</param>
        /// <param name="model">Information applied to updated coupon</param>
        /// <response code="204">Update coupon successfully</response>
        /// <response code="400">Coupon's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Coupon already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutCoupon(int id, [FromForm] CouponUM model)
        {
            ResponseModel responseModel = new();

            Coupon updCoupon = await _service.GetByIdAsync(_ => _.Id == id);

            if (updCoupon is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Coupon));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updCoupon, Operations.Update);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update coupon with id: {id}") { StatusCode = 403 };
            // }


            string imageUrl = updCoupon.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Count > 0)
            {
                List<string> imageUrls = new List<string>();
                foreach (var url in model.ImageUrl)
                {
                    imageUrl = await _uploadFileService.UploadFile("123456798", url, "coupon", "coupon-detail");
                    imageUrls.Add(imageUrl);
                }
                imageUrl = string.Join(",", imageUrls);
            }

            try
            {
                updCoupon.ImageUrl = imageUrl;
                updCoupon.Name = model.Name;
                updCoupon.Description = model.Description;

                _service.Update(updCoupon);
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
        /// Change the status of coupon to inactive
        /// </summary>
        /// <param name="id">Coupon's id</param>
        /// <response code="204">Delete coupon's status successfully</response>
        /// <response code="400">Coupon's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> DeleteCoupon(int id)
        {
            ResponseModel responseModel = new();

            Coupon coupon = await _service.GetByIdAsync(_ => _.Id == id, _ => _.CouponInUses);

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, coupon, Operations.Delete);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to delete coupon with id: {id}") { StatusCode = 403 };
            // }

            if (coupon is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Coupon));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (coupon.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(Coupon));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            coupon.Status = Status.INACTIVE;

            try
            {
                _service.Update(coupon);
                if (await _service.Save() > 0)
                {
                    if (coupon.CouponInUses.Count > 0)
                    {
                        foreach (var item in coupon.CouponInUses)
                        {
                            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                            DateTimeOffset localServerTime = DateTimeOffset.Now;
                            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
                            var notification = new Notification();
                            notification.Title = "Coupon is no longer available";
                            notification.Body = "Coupon " + coupon.Name + " that you saved on " + item.RedeemDate.ToString("dddd, dd MMMM yyyy, hh:mm tt") + " is no longer available";
                            notification.ImageUrl = coupon.ImageUrl;
                            notification.Screen = Constants.Route.COUPON_DETAIL;
                            notification.Parameter = "couponId:" + coupon.Id;
                            notification.AccountId = item.VisitorId;
                            notification.Status = Constants.Status.UNREAD;
                            notification.Date = localTime.DateTime;
                            var crtNotification = await _notificationService.AddAsync(notification);
                            if (await _notificationService.Save() > 0)
                            {
                                var data = new Dictionary<string, string>();
                                data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
                                data.Add("notificationType", "coupon_changed");
                                data.Add("couponId", item.CouponId.ToString());
                                data.Add("couponInUseId", item.Id.ToString());
                                _ = _pushNotificationService.SendMessage(
                                    "Coupon is no longer available",
                                    "Coupon " + coupon.Name + " that you saved on " + item.RedeemDate.ToString("dddd, dd MMMM yyyy, hh:mm tt") + " is no longer available",
                                    "coupon_in_use_id_" + item.Id.ToString(),
                                    data
                                    );
                            }
                        }
                    }
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
