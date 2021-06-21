﻿using AutoMapper;
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
    [Route("api/v1.0/coupons")]
    [ApiController]
    public class CouponController : AuthorizeController
    {
        private readonly ICouponService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Coupon> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;

        public CouponController(ICouponService service, IMapper mapper, IPagingSupport<Coupon> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<CouponVM> GetCouponById(int id)
        {
            var coupon = _service.GetByIdAsync(_ => _.Id == id, _ => _.Store, _ => _.CouponInUses).Result;

            if (coupon == null)
            {
                return NotFound();
            }

            var rtncoupon = _mapper.Map<CouponVM>(coupon);

            return Ok(rtncoupon);
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
        /// <response code="404">No coupons found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<CouponVM>> GetAllCoupons([FromQuery] CouponSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Coupon> list = _service.GetAll(_ => _.Store, _ => _.CouponInUses);

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

            if (!string.IsNullOrEmpty(model.DiscountType))
            {
                list = list.Where(_ => _.DiscountType.Contains(model.DiscountType));
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

            
            if (model.ProductInclude is not null && model.ProductInclude.Length > 0)
            {
                foreach (var include in model.ProductInclude)
                {
                    list = list.Where(_ => _.ProductInclude.Contains(include));
                }
            }
            
            if (model.ProductExclude is not null && model.ProductExclude.Length > 0)
            {
                foreach (var exclude in model.ProductExclude)
                {
                    list = list.Where(_ => _.ProductExclude.Contains(exclude));
                }   
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
                .Paginate<CouponVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new coupon
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
        ///         "Status": "Status of the product group",   
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
            //DateTime currentDateTime = DateTime.Now;

            //if (model.PublishDate.Value < currentDateTime || model.ExpireDate.Value < currentDateTime 
            //    || model.PublishDate < model.ExpireDate) 
            //{
            //    return BadRequest();
            //}
            

            //Coupon productGroup = _service.GetByIdAsync(_ => _.Status == Constants.Status.ACTIVE).Result;
            
            //if (productGroup is not null)
            //{
            //    return Conflict();
            //}

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

            Coupon crtCoupon = _mapper.Map<Coupon>(model);
            
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

            string productInclude = "";
            if (model.ProductInclude is not null && model.ProductInclude.Length > 0)
            {
                Array.Sort(model.ProductInclude);
                productInclude = string.Join(",", model.ProductInclude);
            }
            crtCoupon.ProductInclude = productInclude;

            string productExclude = "";
            if (model.ProductExclude is not null && model.ProductExclude.Length > 0)
            {
                Array.Sort(model.ProductExclude);
                productExclude = string.Join(",", model.ProductExclude);
            }
            crtCoupon.ProductExclude = productExclude;

            try
            {
                await _service.AddAsync(crtCoupon);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
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

            Coupon updCoupon = await _service.GetByIdAsync(_ => _.Id == id);

            //if (!updProduct.Name.ToUpper().Equals(model.Name.ToUpper()))
            //{
            //    Product product = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
            //    if (product is not null)
            //    {
            //        return Conflict();
            //    }
            //}

            if (updCoupon == null || id != model.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

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

            string productInclude = updCoupon.ProductInclude;

            if (model.ProductInclude is not null && model.ProductInclude.Length > 0)
            {
                Array.Sort(model.ProductInclude);
                productInclude = string.Join(",", model.ProductInclude);
            }

            string productExclude = updCoupon.ProductExclude;
            if (model.ProductExclude is not null && model.ProductExclude.Length > 0)
            {
                Array.Sort(model.ProductExclude);
                productExclude = string.Join(",", model.ProductExclude);
            }

            try
            {
                updCoupon.Id = model.Id;
                updCoupon.ImageUrl = imageUrl;
                updCoupon.Name = model.Name;
                updCoupon.Description = model.Description;
                updCoupon.StoreId = model.StoreId;
                updCoupon.Code = model.Code;
                updCoupon.DiscountType = model.DiscountType;
                updCoupon.Amount = model.Amount;
                updCoupon.MaxDiscount = model.MaxDiscount;
                updCoupon.MinSpend = model.MinSpend;
                updCoupon.ProductInclude = productInclude;
                updCoupon.ProductExclude = productExclude;
                updCoupon.Limit = model.Limit;
                updCoupon.Status = model.Status;

                _service.Update(updCoupon);
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
