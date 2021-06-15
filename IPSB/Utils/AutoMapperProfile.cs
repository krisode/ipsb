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

            #region ProductCategoryModel
            CreateMap<ProductCategory, ProductCategoryVM>();
            CreateMap<ProductCategoryCM, Product>();
            #endregion
            #region ProductModel
            CreateMap<Product, ProductVM>();
            #endregion


        }
    }
}
