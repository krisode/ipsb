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
            #region AccountModel
            CreateMap<Account, AccountRefModel>();
            #endregion

            #region BuildingModel
            CreateMap<Building, BuildingRefModel>();
            CreateMap<Building, BuildingVM>();
            CreateMap<BuildingCM, Building>();
            #endregion

            #region CouponModel
            #endregion

            #region CouponInUseModel
            #endregion

            #region EdgeModel
            CreateMap<Edge, EdgeRefModel>();
            CreateMap<Edge, EdgeVM>();
            CreateMap<EdgeCM, Edge>();
            #endregion

            #region FavoriteStoreModel
            #endregion

            #region FloorPlanModel
            CreateMap<FloorPlan, FloorPlanRefModel>();
            CreateMap<FloorPlan, FloorPlanVM>();
            CreateMap<FloorPlanCM, FloorPlan>();
            #endregion

            #region LocationModel
            CreateMap<Location, LocationRefModel>();
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
            #endregion

            #region ProductModel
            CreateMap<Product, ProductRefModel>();
            #endregion

            #region ProductCategoryModel
            CreateMap<ProductCategory, ProductCategoryVM>();
            CreateMap<ProductCategoryCM, ProductCategory>();
            #endregion

            #region ProductGroupModel
            #endregion

            #region StoreModel
            CreateMap<Store, StoreRefModel>();
            #endregion

            #region VisitPointModel
            CreateMap<VisitPoint, VisitPointRefModel>();
            #endregion

            #region VisitRouteModel
            CreateMap<VisitRoute, VisitRouteRefModel>();
            #endregion








        }
    }
}
