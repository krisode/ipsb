﻿using AutoMapper;
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

namespace IPSB.Controllers
{
    [Route("api/v1.0/location-types")]
    [ApiController]
    public class LocationTypeController : Controller
    {
        private readonly ILocationTypeService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<LocationType> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        // private readonly IAuthorizationService _authorizationService;

        public LocationTypeController(ILocationTypeService service, IMapper mapper, IPagingSupport<LocationType> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
        }

        /// <summary>
        /// Get a specific location type by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the location type with the corresponding id</returns>
        /// <response code="200">Returns the location type with the specified id</response>
        /// <response code="404">No location types found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<LocationTypeVM> GetLocationTypeById(int id)
        {
            var locationType = _service.GetByIdAsync(_ => _.Id == id, _ => _.Locations).Result;

            if (locationType == null)
            {
                return NotFound();
            }

            var rtnLocationType = _mapper.Map<LocationTypeRefModel>(locationType);

            return Ok(rtnLocationType);
        }

        /// <summary>
        /// Get all location types
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET 
        ///     {
        ///         "Name": "Name of the location type",   
        ///         "Description": "Description of location type",   
        ///         "ImageUrl": "Image url of location type", 
        ///         "Status": "Status of location type"   
        ///     }
        ///
        /// </remarks>
        /// <returns>All location types</returns>
        /// <response code="200">Returns all location types</response>
        /// <response code="404">No location types found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<LocationTypeVM>> GetAllLocationTypes([FromQuery] LocationTypeSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<LocationType> list = _service.GetAll(_ => _.Locations);

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                list = list.Where(_ => _.Description.Contains(model.Description));
            }
            if (!string.IsNullOrEmpty(model.Status))
            {
                list = list.Where(_ => _.Status == model.Status);
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<LocationTypeRefModel>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new location type
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Name": "Name of the location type",   
        ///         "Description": "Description of location type",   
        ///         "ImageUrl": "Image url of location type", 
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new location type</response>
        /// <response code="400">Invalid parameters</response>
        /// <response code="409">Location type already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LocationTypeCM>> CreateLocationType([FromForm] LocationTypeCM model)
        {
            bool isExisted = _service.GetAll().Where(_ => _.Name.ToLower().Equals(model.Name)).Count() >= 1;
            if (isExisted)
            {
                return Conflict();
            }

            LocationType crtLocationType = _mapper.Map<LocationType>(model);
            crtLocationType.ImageUrl = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "building", "building-detail");

            try
            {
                crtLocationType.Status = Constants.Status.ACTIVE;
                await _service.AddAsync(crtLocationType);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetLocationTypeById", new { id = crtLocationType.Id }, crtLocationType);
        }

        /// <summary>
        /// Update location type with specified id
        /// </summary>
        /// <param name="id">Location type's id</param>
        /// <param name="model">Information applied to updated location type</param>
        /// <response code="204">Update location type successfully</response>
        /// <response code="400">Location type's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Location type already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutLocationType(int id, [FromForm] LocationTypeUM model)
        {
            bool isExisted = _service.GetAll().Where(_ => _.Name.ToLower().Equals(model.Name) && _.Id != id).Count() >= 1;
            if (isExisted)
            {
                return Conflict();
            }
            LocationType updLocationType = await _service.GetByIdAsync(_ => _.Id == id);


            string imageURL = updLocationType.ImageUrl;
            if (model.ImageUrl is not null)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "building", "building-detail");
                updLocationType.ImageUrl = imageURL;
            }

            try
            {
                updLocationType.Name = model.Name;
                updLocationType.Description = model.Description;
                updLocationType.ImageUrl = imageURL;
                _service.Update(updLocationType);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete location type with specified id
        /// </summary>
        /// <param name="id">Location type's id</param>
        /// <response code="204">Delete location type successfully</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            LocationType updLocationType = await _service.GetByIdAsync(_ => _.Id == id);

            try
            {
                updLocationType.Status = Constants.Status.INACTIVE;
                _service.Update(updLocationType);
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
