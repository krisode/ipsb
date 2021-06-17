using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class indoor_positioning_mainContext : DbContext
    {
        public indoor_positioning_mainContext()
        {
        }

        public indoor_positioning_mainContext(DbContextOptions<indoor_positioning_mainContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Building> Buildings { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
        public virtual DbSet<CouponInUse> CouponInUses { get; set; }
        public virtual DbSet<Edge> Edges { get; set; }
        public virtual DbSet<FavoriteStore> FavoriteStores { get; set; }
        public virtual DbSet<FloorPlan> FloorPlans { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationType> LocationTypes { get; set; }
        public virtual DbSet<LocatorTag> LocatorTags { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<ProductGroup> ProductGroups { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<VisitPoint> VisitPoints { get; set; }
        public virtual DbSet<VisitRoute> VisitRoutes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseSqlServer("Server=KRIS;Database=indoor_positioning_main;Trusted_Connection=True;");
            //}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable("Building");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(400);

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.BuildingAdmins)
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Building_Account");

                entity.HasOne(d => d.Manager)
                    .WithMany(p => p.BuildingManagers)
                    .HasForeignKey(d => d.ManagerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Building_Account1");
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.ToTable("Coupon");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.DiscountType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProductExclude)
                    .HasMaxLength(100)
                    .IsFixedLength(true);

                entity.Property(e => e.ProductInclude)
                    .HasMaxLength(100)
                    .IsFixedLength(true);

                entity.Property(e => e.PublishDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Coupons)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Coupon_Store");
            });

            modelBuilder.Entity<CouponInUse>(entity =>
            {
                entity.ToTable("CouponInUse");

                entity.Property(e => e.ApplyDate).HasColumnType("datetime");

                entity.Property(e => e.RedeemDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Coupon)
                    .WithMany(p => p.CouponInUses)
                    .HasForeignKey(d => d.CouponId)
                    .HasConstraintName("FK_CouponInUse_Coupon");

                entity.HasOne(d => d.Visitor)
                    .WithMany(p => p.CouponInUses)
                    .HasForeignKey(d => d.VisitorId)
                    .HasConstraintName("FK_CouponInUse_Account");
            });

            modelBuilder.Entity<Edge>(entity =>
            {
                entity.ToTable("Edge");

                //entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.FromLocation)
                    .WithMany(p => p.EdgeFromLocations)
                    .HasForeignKey(d => d.FromLocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Edge_Location");

                entity.HasOne(d => d.ToLocation)
                    .WithMany(p => p.EdgeToLocations)
                    .HasForeignKey(d => d.ToLocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Edge_Location1");
            });

            modelBuilder.Entity<FavoriteStore>(entity =>
            {
                entity.ToTable("FavoriteStore");

                //entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.RecordDate).HasColumnType("datetime");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.FavoriteStores)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FavoriteStore_Store");
            });

            modelBuilder.Entity<FloorPlan>(entity =>
            {
                entity.ToTable("FloorPlan");

                entity.Property(e => e.FloorCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.FloorPlans)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FloorPlan_Building");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Location");

                entity.HasOne(d => d.FloorPlan)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.FloorPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Location_FloorPlan");

                entity.HasOne(d => d.LocationType)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.LocationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Location_LocationType");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Location_Store");
            });

            modelBuilder.Entity<LocationType>(entity =>
            {
                entity.ToTable("LocationType");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<LocatorTag>(entity =>
            {
                entity.ToTable("LocatorTag");

                entity.Property(e => e.LastSeen).HasColumnType("datetime");

                entity.Property(e => e.MacAddress)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.HasOne(d => d.FloorPlan)
                    .WithMany(p => p.LocatorTags)
                    .HasForeignKey(d => d.FloorPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LocatorTag_FloorPlan");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.LocatorTags)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LocatorTag_Location");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_ProductCategory");

                entity.HasOne(d => d.ProductGroup)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductGroupId)
                    .HasConstraintName("FK_Product_ProductGroup");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_Store");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ProductGroup>(entity =>
            {
                entity.ToTable("ProductGroup");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Image).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.ProductGroups)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductGroup_Store");
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Store");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Phone)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ProductCategoryIds)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Store_Account");

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Store_Building");

                entity.HasOne(d => d.FloorPlan)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.FloorPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Store_FloorPlan");
            });

            modelBuilder.Entity<VisitPoint>(entity =>
            {
                entity.ToTable("VisitPoint");

                entity.Property(e => e.RecordTime).HasColumnType("datetime");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.VisitPoints)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VisitPoint_Location");

                entity.HasOne(d => d.VisitRoute)
                    .WithMany(p => p.VisitPoints)
                    .HasForeignKey(d => d.VisitRouteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VisitPoint_VisitRoute");
            });

            modelBuilder.Entity<VisitRoute>(entity =>
            {
                entity.ToTable("VisitRoute");

                entity.Property(e => e.RecordTime).HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.VisitRoutes)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VisitRoute_Account");

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.VisitRoutes)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VisitRoute_Building");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
