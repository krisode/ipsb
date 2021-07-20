using AutoMapper;
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
    [Route("api/v1.0/stores")]
    [ApiController]
    public class StoreController : AuthorizeController
    {
        private readonly IStoreService _service;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Store> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;

        public StoreController(IStoreService service, IMapper mapper, IPagingSupport<Store> pagingSupport, IUploadFileService uploadFileService, IProductCategoryService productCategoryService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _productCategoryService = productCategoryService;
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
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreVM>> GetStoreById(int id)
        {
            //var store = _service.GetByIdAsync(_ => _.Id == id, _ => _.Account, _ => _.Building,
            //    _ => _.FloorPlan, _ => _.Coupons, _ => _.FavoriteStores, _ => _.Locations,
            //    _ => _.ProductGroups, _ => _.Products).Result;
            var store = await _service.GetByIdAsync(_ => _.Id == id, _ => _.Account, _ => _.Building, _ => _.FloorPlan);

            if (store == null)
            {
                return NotFound();
            }

            //if (store.ProductCategoryIds.Length > 1)
            //{
            //    foreach (var productCategoryId in store.ProductCategoryIds)
            //    {
            //        ProductGroup productGroup = _productGroupService.GetByIdAsync(_ => _.Id == productCategoryId).Result;
            //        store.ProductGroups.Add(productGroup);
            //    }
            //}

            var rtnStore = _mapper.Map<StoreVM>(store);
            //rtnStore.ProductCategories = new List<ProductCategoryRefModel>();
            //foreach (var productCategoryId in store.ProductCategoryIds)
            //{

            //    int idd = 0;
            //    if (!productCategoryId.ToString().Equals(","))
            //    {
            //        idd = int.Parse(productCategoryId.ToString());
            //        ProductCategory productCategory = _productCategoryService.GetByIdAsync(_ => _.Id == idd).Result;
            //        ProductCategoryRefModel productCategoryRefModel = _mapper.Map<ProductCategoryRefModel>(productCategory);
            //        rtnStore.ProductCategories.Add(productCategoryRefModel);
            //    }

            //}


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
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StoreVM>> GetAllStores([FromQuery] StoreSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            //IQueryable<Store> list = _service.GetAll(_ => _.Account, _ => _.Building,
            //    _ => _.FloorPlan, _ => _.Coupons, _ => _.FavoriteStores, _ => _.Locations,
            //    _ => _.ProductGroups, _ => _.Products);
            IQueryable<Store> list = _service.GetAll(_ => _.Account, _ => _.Building, _ => _.FloorPlan);

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

            Store store = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;

            if (store is not null)
            {
                return Conflict();
            }

            //if (!string.IsNullOrEmpty(model.Status))
            //{
            //    if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
            //    {
            //        return BadRequest();
            //    }
            //}

            Store crtStore = _mapper.Map<Store>(model);

            // Default POST Status = "Active"
            crtStore.Status = Constants.Status.ACTIVE;

            string imageUrl = "";

            if (model.ImageUrl is not null && model.ImageUrl.Count > 0)
            {
                //List<string> imageUrls = new List<string>();
                var task = model.ImageUrl.ToList().Select(_ => _uploadFileService.UploadFile("123456798", _, "store", "store-detail")).ToArray();
                var imageUrls = await Task.WhenAll(task);
                //var testList = task.Select<string>(_ => _.);

                imageUrl = string.Join(",", imageUrls);

                //foreach (var url in model.ImageUrl)
                //{
                //    imageUrl = await _uploadFileService.UploadFile("123456798", url, "store", "store-detail");
                //    imageUrls.Add(imageUrl);
                //}
            }

            crtStore.ImageUrl = imageUrl;

            string productCategoryIds = "";
            if (model.ProductCategoryIds is not null && model.ProductCategoryIds.Length > 0)
            {
                Array.Sort(model.ProductCategoryIds);
                productCategoryIds = string.Join(",", model.ProductCategoryIds);
            }
            crtStore.ProductCategoryIds = productCategoryIds;

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

            Store updStore = await _service.GetByIdAsync(_ => _.Id == id);

            if (!updStore.Name.ToUpper().Equals(model.Name.ToUpper()))
            {
                Store store = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
                if (store is not null)
                {
                    return Conflict();
                }
            }

            if (updStore == null || id != model.Id)
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

            string imageUrl = updStore.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Count > 0)
            {
                List<string> imageUrls = new List<string>();
                foreach (var url in model.ImageUrl)
                {
                    imageUrl = await _uploadFileService.UploadFile("123456798", url, "store", "store-detail");
                    imageUrls.Add(imageUrl);
                }
                imageUrl = string.Join(",", imageUrls);
            }

            string productCategoryIds = updStore.ProductCategoryIds;

            if (model.ProductCategoryIds is not null && model.ProductCategoryIds.Length > 0)
            {
                Array.Sort(model.ProductCategoryIds);
                productCategoryIds = string.Join(",", model.ProductCategoryIds);
            }

            try
            {
                updStore.Id = model.Id;
                updStore.Name = model.Name;
                updStore.AccountId = model.AccountId;
                updStore.ImageUrl = imageUrl;
                updStore.BuildingId = model.BuildingId;
                updStore.Description = model.Description;
                updStore.FloorPlanId = model.FloorPlanId;
                updStore.ProductCategoryIds = productCategoryIds;
                updStore.Phone = model.Phone;
                updStore.Status = model.Status;

                _service.Update(updStore);
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
