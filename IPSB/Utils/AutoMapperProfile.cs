using AutoMapper;
using IPSB.Infrastructure.Contexts;
using IPSB.ViewModels;

namespace IPSB.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            
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
            CreateMap<Building, BuildingRefModelForStore>();
            CreateMap<Building, BuildingVM>();
            CreateMap<Building, BuildingRefModelForAccount>();
            CreateMap<Building, BuildingRefModelForShoppingList>();
            CreateMap<BuildingCM, Building>();
            #endregion

            #region CouponModel
            CreateMap<Coupon, CouponRefModel>();
            CreateMap<Coupon, CouponVM>();
            CreateMap<CouponCM, Coupon>();
            #endregion

            #region CouponTypeModel
            CreateMap<CouponType, CouponTypeVM>();
            CreateMap<CouponTypeCM, CouponType>();
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

            #region FloorPlanModel
            CreateMap<FloorPlan, FloorPlanRefModel>();
            CreateMap<FloorPlan, FloorPlanStoreRefModel>();
            CreateMap<FloorPlan, FloorPlanVM>();
            CreateMap<FloorPlanCM, FloorPlan>();
            #endregion

            #region LocationModel
            CreateMap<Location, LocationRefModel>();
            CreateMap<Location, LocationRefModelForEdge>();
            CreateMap<Location, LocationRefModelForStore>();
            CreateMap<Location, LocationRefModelForVisitPoint>();
            CreateMap<Location, LocationVM>();
            CreateMap<LocationCM, Location>();
            #endregion

            #region LocationTypeModel
            CreateMap<Facility, FacilityVM>();
            CreateMap<Facility, FacilityRefModel>();
            CreateMap<FacilityCM, Facility>();
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

            #region NotificationModel
            CreateMap<Notification, NotificationRefModel>();
            CreateMap<Notification, NotificationVM>();
            CreateMap<NotificationCM, Notification>();
            #endregion

            #region ProductModel
            CreateMap<Product, ProductRefModel>();
            CreateMap<Product, ProductVM>();
            CreateMap<Product, ProductRefModelForShoppingItem>();
            CreateMap<ProductCM, Product>();
            #endregion

            #region ProductCategoryModel
            CreateMap<ProductCategory, ProductCategoryRefModel>();
            CreateMap<ProductCategory, ProductCategoryVM>();
            CreateMap<ProductCategoryCM, ProductCategory>();
            #endregion

            #region StoreModel
            CreateMap<Store, StoreRefModel>();
            CreateMap<Store, StoreRefModelForEdge>();
            CreateMap<Store, StoreRefModelForProduct>();
            CreateMap<Store, StoreVM>();
            CreateMap<Store, StoreRefModelForAccount>();
            CreateMap<StoreCM, Store>();
            #endregion

            #region VisitPointModel
            CreateMap<VisitStore, VisitStoreRefModel>();
            CreateMap<VisitStore, VisitStoreVM>();
            CreateMap<VisitStoreCM, VisitStore>();
            #endregion

            #region ShoppingList
            CreateMap<ShoppingList, ShoppingListVM>();
            CreateMap<ShoppingList, ShoppingListRefModel>();
            CreateMap<ShoppingListCM, ShoppingList>();
            #endregion

            #region ShoppingItem
            CreateMap<ShoppingItem, ShoppingItemVM>();
            CreateMap<ShoppingItemCM, ShoppingItem>();
            #endregion
        }
    }
}
