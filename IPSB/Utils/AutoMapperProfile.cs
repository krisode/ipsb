using AutoMapper;
using IPSB.Infrastructure.Contexts;
using IPSB.ViewModels;

namespace IPSB.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //#region AutoMapper ServiceViewModel
            //CreateMap<ServiceCM, Service>();
            //CreateMap<Service, ServiceVM>();
            //CreateMap<Service, ServicePagingSM>();
            //CreateMap<ServicePagingSM, Service>();
            //CreateMap<ServiceVM, Service>();
            //CreateMap<ServiceCM, Service>();
            //#endregion
            #region AuthModel
            CreateMap<Account, AuthLoginSuccess>();
            #endregion

            #region AccountModel
            CreateMap<Account, AccountRefModel>();
            CreateMap<Account, AccountStoreRefModel>();
            CreateMap<Account, AccountVM>();
            CreateMap<AccountCM, Account>();
            #endregion

            #region BuildingModel
            CreateMap<Building, BuildingRefModel>();
            CreateMap<Building, BuildingStoreRefModel>();
            CreateMap<Building, BuildingVM>();
            CreateMap<BuildingCM, Building>();
            #endregion

            #region CouponModel
            CreateMap<Coupon, CouponRefModel>();
            CreateMap<Coupon, CouponVM>();
            CreateMap<CouponCM, Coupon>();
            #endregion

            #region CouponInUseModel
            CreateMap<CouponInUse, CouponInUseRefModel>();
            CreateMap<CouponInUse, CouponInUseVM>();
            CreateMap<CouponInUseCM, CouponInUse>();
            #endregion

            #region EdgeModel
            CreateMap<Edge, EdgeRefModel>();
            CreateMap<Edge, EdgeVM>();
            CreateMap<EdgeCM, Edge>();
            #endregion

            #region FavoriteStoreModel
            CreateMap<FavoriteStore, FavoriteStoreRefModel>();
            CreateMap<FavoriteStore, FavoriteStoreVM>();
            CreateMap<FavoriteStoreCM, FavoriteStore>();
            #endregion

            #region FloorPlanModel
            CreateMap<FloorPlan, FloorPlanRefModel>();
            CreateMap<FloorPlan, FloorPlanStoreRefModel>();
            CreateMap<FloorPlan, FloorPlanVM>();
            CreateMap<FloorPlanCM, FloorPlan>();
            #endregion

            #region LocationModel
            CreateMap<Location, LocationRefModel>();
            CreateMap<Location, LocationRefModelForEdge>();
            CreateMap<Location, LocationVM>();
            CreateMap<LocationCM, Location>();
            #endregion

            #region LocationTypeModel
            CreateMap<LocationType, LocationTypeRefModel>();
            CreateMap<LocationType, LocationTypeVM>();
            CreateMap<LocationTypeCM, LocationType>();
            #endregion

            #region LocatorTagModel
            CreateMap<LocatorTag, LocatorTagRefModel>();
            CreateMap<LocatorTag, LocatorTagVM>();
            CreateMap<LocatorTagCM, LocatorTag>();
            #endregion

            #region ProductModel
            CreateMap<Product, ProductRefModel>();
            CreateMap<Product, ProductVM>();
            CreateMap<ProductCM, Product>();
            #endregion

            #region ProductCategoryModel
            CreateMap<ProductCategory, ProductCategoryRefModel>();
            CreateMap<ProductCategory, ProductCategoryVM>();
            CreateMap<ProductCategoryCM, ProductCategory>();
            #endregion

            #region ProductGroupModel
            CreateMap<ProductGroup, ProductGroupRefModel>();
            CreateMap<ProductGroup, ProductGroupVM>();
            CreateMap<ProductGroupCM, ProductGroup>();
            #endregion

            #region StoreModel
            CreateMap<Store, StoreRefModel>();
            CreateMap<Store, StoreRefModelForEdge>();
            CreateMap<Store, StoreVM>();
            CreateMap<StoreCM, Store>();
            #endregion

            #region VisitPointModel
            CreateMap<VisitPoint, VisitPointRefModel>();
            CreateMap<VisitPoint, VisitPointVM>();
            CreateMap<VisitPointCM, VisitPoint>();
            #endregion

            #region VisitRouteModel
            CreateMap<VisitRoute, VisitRouteRefModel>();
            CreateMap<VisitRoute, VisitRouteVM>();
            CreateMap<VisitRouteCM, VisitRoute>();
            #endregion

            #region ShoppingList
            CreateMap<ShoppingList, ShoppingListVM>();
            CreateMap<ShoppingListCM, ShoppingList>();
            #endregion

            #region ShoppingItem
            CreateMap<ShoppingItem, ShoppingItemVM>();
            CreateMap<ShoppingItemCM, ShoppingItem>();
            #endregion
        }
    }
}
