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

namespace IPSB.Controllers
{
    [Route("api/v1.0/buildings")]
    [ApiController]
    [Authorize(Roles = "Building Manager")]
    public class BuildingController : AuthorizeController
    {
        private readonly IBuildingService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Building> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        public BuildingController(IBuildingService service, IMapper mapper, IPagingSupport<Building> pagingSupport, IUploadFileService uploadFileService, IAuthorizationService authorizationService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingVM>> GetBuildingById(int id)
        {
            var building = _service.GetByIdAsync(_ => _.Id == id, 
                _ => _.Admin, 
                _ => _.Manager, 
                _ => _.FloorPlans, 
                _ => _.Stores, 
                _ => _.VisitRoutes).Result;

            if (building == null)
            {
                return NotFound();
            }
            
            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to access building with id: {id}") { StatusCode = 403 };
            }*/

            var rtnBuilding = _mapper.Map<BuildingVM>(building);

            return Ok(rtnBuilding);
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
        /// <response code="404">No buildings found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<BuildingVM>> GetAllBuildings([FromQuery] BuildingSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Building> list = _service.GetAll(_ => _.Admin, _ => _.Manager, _ => _.FloorPlans, _ => _.Stores, _ => _.VisitRoutes);

            if (model.ManagerId != 0)
            {
                list = list.Where(_ => _.ManagerId == model.ManagerId);
            }

            if (model.AdminId != 0)
            {
                list = list.Where(_ => _.AdminId == model.AdminId);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (model.NumberOfFloor != 0)
            {
                list = list.Where(_ => _.NumberOfFloor == model.NumberOfFloor);
            }

            if (!string.IsNullOrEmpty(model.Address))
            {
                list = list.Where(_ => _.Address.Contains(model.Address));
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
                .Paginate<BuildingVM>();

            return Ok(pagedModel);
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
            Building building = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
            if (building is not null)
            {
                return Conflict();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create building") { StatusCode = 403 };
            }*/

            Building crtBuilding = _mapper.Map<Building>(model);
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
                return StatusCode(StatusCodes.Status500InternalServerError);
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

            Building updBuilding = await _service.GetByIdAsync(_ => _.Id == id);

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updBuilding, Operations.Update);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to update building with id: {id}") { StatusCode = 403 };
            }

            if (updBuilding == null || id != model.Id)
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

            string imageURL = updBuilding.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "building", "building-detail");
            }
            
            try
            {
                updBuilding.Id = model.Id;
                updBuilding.ManagerId = model.ManagerId;
                updBuilding.AdminId = model.AdminId;
                updBuilding.Name = model.Name;
                updBuilding.ImageUrl = imageURL;
                updBuilding.NumberOfFloor = model.NumberOfFloor;
                updBuilding.Address = model.Address;
                updBuilding.Status = model.Status;
                
                _service.Update(updBuilding);
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
