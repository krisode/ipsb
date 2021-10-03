
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using IPSB.AuthorizationHandler;
using IPSB.Cache;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using IPSB.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region NewtonsoftJson Configuration to ignore reference loop handling for Controller
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            #endregion

            #region NewtonsoftJson Configuration to ignore reference loop handling for ControllerWithViews
            services.AddControllersWithViews()
            .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            #endregion

            services.AddCors();

            #region AWS SQL Database Connection
            services.AddDbContext<IndoorPositioningContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("IPSBDatabase")));
            #endregion

            #region Redis Cache Connection
            services.AddStackExchangeRedisCache(setupAction =>
            {
                setupAction.Configuration = Configuration.GetConnectionString("RedisConnectionString");
            });
            #endregion

            #region Redis Cache Configuration
            var children = Configuration.GetSection("Caching").GetChildren();
            Dictionary<string, TimeSpan> configuration =
            children.ToDictionary(child => child.Key, child => TimeSpan.Parse(child.Value));
            services.AddSingleton<ICacheStore>(x => new RedisCacheStore(x.GetService<IDistributedCache>(), configuration));
            #endregion

            #region Repository
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            #endregion

            #region Firebase Initial
            var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "firebase_admin_sdk.json");
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(pathToKey)
            });
            #endregion

            #region Utilities
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped(typeof(IPagingSupport<>), typeof(PagingSupport<>));
            services.AddSingleton<IUploadFileService, UploadFileService>();
            services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
            #endregion

            #region Authorization Handler
            services.AddSingleton<IAuthorizationHandler, AccountHandler>();
            services.AddSingleton<IAuthorizationHandler, QueryAccountHandler>();
            services.AddSingleton<IAuthorizationHandler, BuildingHandler>();
            services.AddSingleton<IAuthorizationHandler, CouponHandler>();
            services.AddSingleton<IAuthorizationHandler, CouponInUseHandler>();
            /*services.AddSingleton<IAuthorizationHandler, CouponInUseActionHandler>();*/
            services.AddSingleton<IAuthorizationHandler, EdgeHandler>();
            /*services.AddSingleton<IAuthorizationHandler, FavoriteStoreHandler>();*/
            services.AddSingleton<IAuthorizationHandler, FloorPlanHandler>();
            /*services.AddSingleton<IAuthorizationHandler, LocationHandler>();
            services.AddSingleton<IAuthorizationHandler, LocationTypeHandler>();*/
            services.AddSingleton<IAuthorizationHandler, LocatorTagHandler>();
            services.AddSingleton<IAuthorizationHandler, ProductCategoryHandler>();
            /*services.AddSingleton<IAuthorizationHandler, ProductGroupHandler>();*/
            services.AddSingleton<IAuthorizationHandler, ProductHandler>();

            services.AddSingleton<IAuthorizationHandler, StoreHandler>();
            /*services.AddSingleton<IAuthorizationHandler, VisitPointHandler>();
            services.AddSingleton<IAuthorizationHandler, VisitRouteHandler>();*/

            #endregion

            #region DB Services
            // Add AccountService
            services.AddTransient<IAccountService, AccountService>();
            // Add BuildingService
            services.AddTransient<IBuildingService, BuildingService>();
            // Add CouponService
            services.AddTransient<ICouponService, CouponService>();
            // Add CouponService
            services.AddTransient<ICouponTypeService, CouponTypeService>();
            // Add CouponInUseService
            services.AddTransient<ICouponInUseService, CouponInUseService>();
            // Add EdgeService
            services.AddTransient<IEdgeService, EdgeService>();
            // Add FloorPlanService
            services.AddTransient<IFloorPlanService, FloorPlanService>();
            // Add LocationService
            services.AddTransient<ILocationService, LocationService>();
            // Add LocationTypeService
            services.AddTransient<ILocationTypeService, LocationTypeService>();
            // Add LocatorTagService
            services.AddTransient<ILocatorTagService, LocatorTagService>();
            // Add ProductService
            services.AddTransient<IProductService, ProductService>();
            // Add ProductCategoryService
            services.AddTransient<IProductCategoryService, ProductCategoryService>();
            // Add ProductGroupService
            services.AddTransient<IProductGroupService, ProductGroupService>();
            // Add StoreService
            services.AddTransient<IStoreService, StoreService>();
            // Add VisitPointService
            services.AddTransient<IVisitPointService, VisitPointService>();
            // Add VisitRouteService
            services.AddTransient<IVisitRouteService, VisitRouteService>();
            // Add ShoppingListService
            services.AddTransient<IShoppingListService, ShoppingListService>();
            // Add ShoppingItemService
            services.AddTransient<IShoppingItemService, ShoppingItemService>();
            #endregion

            #region Authentication JWT Bearer
            services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration[Constants.Config.KEY])),
                    ValidIssuer = Configuration[Constants.Config.ISSUER],
                    ValidAudience = Configuration[Constants.Config.AUDIENCE],
                };
            });
            #endregion

            #region Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.QUERY_ACCOUNT, policy => policy.Requirements.Add(new QueryAccountRequirement()));
            });
            #endregion

            #region Swagger Configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Indoor Positioning System", Version = "v1.0" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Enter JWT Bearer token in the text box below. ",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IPSB v1"));
            }

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
