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
using Microsoft.EntityFrameworkCore;

namespace IPSB.Controllers
{
    [Route("api/v1.0/products")]
    [ApiController]
    [Authorize(Roles = "Visitor, Store Owner")]
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Product> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INotificationService _notificationService;

        public ProductController(IProductService service, IMapper mapper, IPagingSupport<Product> pagingSupport,
            IUploadFileService uploadFileService, IAuthorizationService authorizationService, IPushNotificationService pushNotificationService,
            INotificationService notificationService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _pushNotificationService = pushNotificationService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get a specific product by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the product with the corresponding id</returns>
        /// <response code="200">Returns the product with the specified id</response>
        /// <response code="404">No products found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult<ProductVM> GetProductById(int id)
        {
            var product = _service.GetByIdAsync(_ => _.Id == id, _ => _.ProductCategory, _ => _.Store).Result;

            if (product == null)
            {
                return NotFound();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, product, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to access product with id: {id}");
            }*/

            var rtnEdge = _mapper.Map<ProductVM>(product);

            return Ok(rtnEdge);
        }

        /// <summary>
        /// Get all products
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
        /// <returns>All products</returns>
        /// <response code="200">Returns all products</response>
        /// <response code="404">No products found</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ProductVM>> GetAllProducts([FromQuery] ProductSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Product> list = _service.GetAll(_ => _.ProductCategory, _ => _.Store);

            if (model.StoreId > 0)
            {
                list = list.Where(_ => _.StoreId == model.StoreId);
            }
            if (model.BuildingId > 0)
            {
                list = list.Where(_ => _.Store.BuildingId == model.BuildingId);
            }
            if (model.ProductCategoryId > 0)
            {
                list = list.Where(_ => _.ProductCategoryId == model.ProductCategoryId);
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                list = list.Where(_ => _.Description.Contains(model.Description));
            }

            if (model.LowerPrice > 0)
            {
                list = list.Where(_ => _.Price >= model.LowerPrice);
            }

            if (model.UpperPrice > 0)
            {
                list = list.Where(_ => _.Price <= model.UpperPrice);
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
                .Paginate<ProductVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Name": "Name of the product",   
        ///         "StoreId": "Id of the store which the product belongs to",   
        ///         "ProductGroupId": "Id of the product group which the product belongs to",   
        ///         "ImageUrl": "Image of the product",   
        ///         "Description": "General description of the product",   
        ///         "ProductCategoryId": "Id of the product category which the product belongs to",   
        ///         "Price": "Price of the product",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new product</response>
        /// <response code="409">Product already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductCM>> CreateProduct([FromForm] ProductCM model)
        {

            // Product existed if the name is non-duplicate within the store that this user owned
            bool isExisted = _service.GetAll()
                                    .Where(
                                        _ => _.Name.ToLower().Equals(model.Name.ToLower()) 
                                        && _.Store.AccountId == int.Parse(User.Identity.Name)
                                    )
                                    .Count() == 1;
            if (isExisted)
            {
                return Conflict();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, product, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create product") { StatusCode = 403 };
            }*/

            Product crtProduct = _mapper.Map<Product>(model);
            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "product", "product-detail");
            crtProduct.ImageUrl = imageURL;

            // Default POST Status = "New"
            crtProduct.Status = Constants.Status.ACTIVE;

            try
            {
                await _service.AddAsync(crtProduct);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetProductById", new { id = crtProduct.Id }, crtProduct);
        }

        /// <summary>
        /// Update product with specified id
        /// </summary>
        /// <param name="id">Product's id</param>
        /// <param name="model">Information applied to updated product</param>
        /// <response code="204">Update product successfully</response>
        /// <response code="400">Product's id does not exist or does not match with the id in parameter</response>
        /// <response code="404">Product not found</response>
        /// <response code="409">Product already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutProduct(int id, [FromForm] ProductUM model)
        {
            // Product existed if the name is non-duplicate within the store that this user owned
            bool isExisted = _service.GetAll()
                                    .Where(
                                        _ => _.Name.ToLower().Equals(model.Name.ToLower())
                                        && _.Store.AccountId == int.Parse(User.Identity.Name)
                                        && _.Id != id
                                    )
                                    .Count() == 1;
            if (isExisted)
            {
                return Conflict();
            }

            Product updProduct = await _service.GetByIdAsync(_ => _.Id == id);

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updProduct, Operations.Update);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update product with id: {id}") { StatusCode = 403 };
            // }

            if (updProduct == null)
            {
                return NotFound();
            }

            if (updProduct.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }


            string imageUrl = updProduct.ImageUrl;

            if (model.ImageUrl != null)
            {
                imageUrl = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "product", "product-detail");
            }

            try
            {
                updProduct.Name = model.Name;
                updProduct.ImageUrl = imageUrl;
                updProduct.Description = model.Description;
                if (model.ProductCategoryId > 0)
                {
                    updProduct.ProductCategoryId = model.ProductCategoryId;
                }
                if (model.Price > 0)
                {
                    updProduct.Price = model.Price;
                }
                _service.Update(updProduct);
                await _service.Save();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete product with specified id
        /// </summary>
        /// <param name="id">Product's id</param>
        /// <response code="204">Delete product successfully</response>
        /// <response code="400">Product delete bad request</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            Product product = _service.GetAll().Include(_ => _.ShoppingItems).ThenInclude(_ => _.ShoppingList).FirstOrDefault(_ => _.Id == id);


            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, product, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete product with id: {id}") { StatusCode = 403 };
            }*/

            if (product is null)
            {
                return NotFound();
            }

            if (product.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }

            try
            {
                product.Status = Constants.Status.INACTIVE;
                _service.Update(product);
                if (_service.Save().Result > 0)
                {
                    if (product.ShoppingItems.Count > 0)
                    {
                        foreach (var item in product.ShoppingItems)
                        {
                            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                            DateTimeOffset localServerTime = DateTimeOffset.Now;
                            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
                            var notification = new Notification();
                            notification.Title = "Product is no longer available";
                            notification.Body = "Product " + product.Name + " in your shopping list " + item.ShoppingList.Name + " is no longer available";
                            notification.ImageUrl = product.ImageUrl;
                            notification.Screen = Constants.Route.SHOPPING_LIST_DETAIL;
                            notification.Parameter = "shoppingListId:" + item.ShoppingListId;
                            notification.AccountId = item.ShoppingList.AccountId;
                            notification.Status = Constants.Status.UNREAD;
                            notification.Date = localTime.DateTime;
                            var crtNotification = await _notificationService.AddAsync(notification);
                            if (await _notificationService.Save() > 0)
                            {
                                var data = new Dictionary<String, String>();
                                data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
                                data.Add("notificationType", "shopping_list_changed");
                                data.Add("shoppingListId", item.ShoppingListId.ToString());
                                _ = _pushNotificationService.SendMessage(
                                    "Product is no longer available",
                                    "Product " + product.Name + " in your shopping list " + item.ShoppingList.Name + " is no longer available",
                                    "shopping_list_id_" + item.ShoppingListId,
                                    data
                                    );
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }


    }
}
