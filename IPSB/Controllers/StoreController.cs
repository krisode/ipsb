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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/stores")]
    [ApiController]
    [Authorize(Roles = "Visitor, Building Manager, Store Owner")]
    public class StoreController : Controller
    {
        private readonly IStoreService _service;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Store> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILocationService _locationService;

        public StoreController(IStoreService service, IProductCategoryService productCategoryService, IMapper mapper, IPagingSupport<Store> pagingSupport, IUploadFileService uploadFileService, IAuthorizationService authorizationService, ILocationService locationService)
        {
            _service = service;
            _productCategoryService = productCategoryService;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _locationService = locationService;
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
            var store = await _service.GetByIdAsync(_ => _.Id == id, _ => _.Account, _ => _.Building, _ => _.FloorPlan);

            if (store == null)
            {
                return NotFound();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, store, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to access store with id: {id}") { StatusCode = 403 };
            }*/

            var rtnStore = _mapper.Map<StoreVM>(store);

            return Ok(rtnStore);
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
        /// <response code="404">No stores found</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StoreVM>> GetAllStores([FromQuery] StoreSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            //IQueryable<Store> list = _service.GetAll(_ => _.Account, _ => _.Building,
            //    _ => _.FloorPlan, _ => _.Coupons, _ => _.FavoriteStores, _ => _.Locations,
            //    _ => _.ProductGroups, _ => _.Products);
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
                foreach (var include in model.ProductCategoryIds)
                {
                    list = list.Where(_ => _.ProductCategoryIds.Contains(include));
                }
            }

            //foreach (var store in list)
            //{
            //    if (store.ProductCategoryIds.Length > 1)
            //    {
            //        foreach (var productCategoryId in store.ProductCategoryIds)
            //        {
            //            ProductCategory productCategory = _productCategoryService.GetByIdAsync(_ => _.Id == productCategoryId).Result;
            //            store.ProductCategory.Add(productCategory);
            //        }
            //    }
            //}


            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
                else
                {
                    list = list.Where(_ => _.Status == model.Status);
                }
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<StoreVM>();

            //foreach (var store in pagedModel.Content.ToList())
            //{
            //    List<ProductCategoryRefModel> listProCate = new List<ProductCategoryRefModel>();

            //    foreach (var productCategoryId in store.ProductCategoryIds)
            //    {
            //        int idd = 0;
            //        if (!productCategoryId.ToString().Equals(","))
            //        {
            //            idd = int.Parse(productCategoryId.ToString());
            //            ProductCategory productCategory = _productCategoryService.GetByIdAsync(_ => _.Id == idd).Result;
            //            ProductCategoryRefModel productCategoryRefModel = _mapper.Map<ProductCategoryRefModel>(productCategory);
            //            listProCate.Add(productCategoryRefModel);
            //        }
            //    }
            //    store.ProductCategories = listProCate;

            //}
            return Ok(pagedModel);
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


            bool isExisted = _service.IsExisted(_ => _.Name.ToLower().Equals(model.Name.ToLower()));
            if (isExisted)
            {
                return Conflict();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, store, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create store") { StatusCode = 403 };
            }*/
            // Default POST Status = "Active" of Location
            
            Store crtStore = _mapper.Map<Store>(model);
            
            // Default POST Status = "Active"
            crtStore.Status = Constants.Status.ACTIVE;
            crtStore.LocationId = await _locationService.CreateLocationJson(model.LocationJson);


            if (model.ImageUrl != null)
            {
                crtStore.ImageUrl = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "store", "store-detail");
            }

            try
            {

                await _service.AddAsync(crtStore);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
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
            if (_service.IsExisted(_ => _.Id != id && _.Name.ToLower().Equals(model.Name.ToLower())))
            {
                return Conflict();
            }

            Store updStore = await _service.GetByIdAsync(_ => _.Id == id);

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updStore, Operations.Update);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to update store with id: {id}") { StatusCode = 403 };
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
                await _locationService.UpdateLocationJson(updStore.LocationId, model.LocationJson);
                _service.Update(updStore);
                await _service.Save();
            }
            catch (Exception e)
            {

                return BadRequest(e);
            }

            return NoContent();
        }

        /// <summary>
        /// Change the status of store to inactive
        /// </summary>
        /// <param name="id">Store's id</param>
        /// <response code="204">Update store's status successfully</response>
        /// <response code="400">Store's id does not exist</response>
        /// <response code="500">Failed to update</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            var store = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (store is null)
            {
                return BadRequest();
            }

            if (store.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }

            store.Status = Constants.Status.INACTIVE;
            store.AccountId = null;      
            try
            {
                _service.Update(store);
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
