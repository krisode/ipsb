using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class IndoorPositioningContext : DbContext
    {
        public IndoorPositioningContext()
        {
        }

        public IndoorPositioningContext(DbContextOptions<IndoorPositioningContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Building> Buildings { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
        public virtual DbSet<CouponInUse> CouponInUses { get; set; }
        public virtual DbSet<CouponType> CouponTypes { get; set; }
        public virtual DbSet<Edge> Edges { get; set; }
        public virtual DbSet<Facility> Facilities { get; set; }
        public virtual DbSet<FloorPlan> FloorPlans { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationType> LocationTypes { get; set; }
        public virtual DbSet<LocatorTag> LocatorTags { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<ShoppingItem> ShoppingItems { get; set; }
        public virtual DbSet<ShoppingList> ShoppingLists { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<VisitPoint> VisitPoints { get; set; }
        public virtual DbSet<VisitRoute> VisitRoutes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=indoor-positioning.cm4zyhsrdgxp.ap-southeast-1.rds.amazonaws.com, 1433;Initial Catalog=indoor_positioning_main;User ID=admin;Password=TheHien2407abcX123");
            }
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

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.Phone)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable("Building");

                entity.HasIndex(e => e.ManagerId, "UQ__Building__3BA2AAE0CB321A0A")
                    .IsUnique();

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
                    .HasConstraintName("FK_Building_Account");

                entity.HasOne(d => d.Manager)
                    .WithOne(p => p.BuildingManager)
                    .HasForeignKey<Building>(d => d.ManagerId)
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

                entity.HasOne(d => d.CouponType)
                    .WithMany(p => p.Coupons)
                    .HasForeignKey(d => d.CouponTypeId)
                    .HasConstraintName("FK_Coupon_CouponType");

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

                entity.Property(e => e.FeedbackContent).HasMaxLength(400);

                entity.Property(e => e.FeedbackDate).HasColumnType("datetime");

                entity.Property(e => e.FeedbackImage)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.FeedbackReply).HasMaxLength(400);

                entity.Property(e => e.RedeemDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Coupon)
                    .WithMany(p => p.CouponInUses)
                    .HasForeignKey(d => d.CouponId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CouponInUse_Coupon");

                entity.HasOne(d => d.Visitor)
                    .WithMany(p => p.CouponInUses)
                    .HasForeignKey(d => d.VisitorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CouponInUse_Account");
            });

            modelBuilder.Entity<CouponType>(entity =>
            {
                entity.ToTable("CouponType");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Edge>(entity =>
            {
                entity.ToTable("Edge");

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

            modelBuilder.Entity<Facility>(entity =>
            {
                entity.ToTable("Facility");

                entity.HasIndex(e => e.LocationId, "IX_Facility")
                    .IsUnique();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.FloorPlan)
                    .WithMany(p => p.Facilities)
                    .HasForeignKey(d => d.FloorPlanId)
                    .HasConstraintName("FK_Facility_FloorPlan");

                entity.HasOne(d => d.Location)
                    .WithOne(p => p.Facility)
                    .HasForeignKey<Facility>(d => d.LocationId)
                    .HasConstraintName("FK_Facility_Location");
            });

            modelBuilder.Entity<FloorPlan>(entity =>
            {
                entity.ToTable("FloorPlan");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.FloorCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FloorType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
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

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

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
            });

            modelBuilder.Entity<LocationType>(entity =>
            {
                entity.ToTable("LocationType");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LocatorTag>(entity =>
            {
                entity.ToTable("LocatorTag");

                entity.HasIndex(e => e.LocationId, "idx_LocationId")
                    .IsUnique()
                    .HasFilter("([LocationId] IS NOT NULL)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.Property(e => e.Uuid)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.FloorPlan)
                    .WithMany(p => p.LocatorTags)
                    .HasForeignKey(d => d.FloorPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LocatorTag_FloorPlan");

                entity.HasOne(d => d.Location)
                    .WithOne(p => p.LocatorTag)
                    .HasForeignKey<LocatorTag>(d => d.LocationId)
                    .HasConstraintName("FK_LocatorTag_Location");

                entity.HasOne(d => d.LocatorTagGroup)
                    .WithMany(p => p.InverseLocatorTagGroup)
                    .HasForeignKey(d => d.LocatorTagGroupId)
                    .HasConstraintName("FK_LocatorTag_LocatorTag");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Body)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Parameter)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Screen)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Notificat__Accou__725BF7F6");
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

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_Store");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ShoppingItem>(entity =>
            {
                entity.ToTable("ShoppingItem");

                entity.Property(e => e.Note).HasMaxLength(100);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ShoppingItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingItem_Product");

                entity.HasOne(d => d.ShoppingList)
                    .WithMany(p => p.ShoppingItems)
                    .HasForeignKey(d => d.ShoppingListId)
                    .HasConstraintName("FK_ShoppingItem_ShoppingList");
            });

            modelBuilder.Entity<ShoppingList>(entity =>
            {
                entity.ToTable("ShoppingList");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ShoppingDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ShoppingLists)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingList_Account");

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.ShoppingLists)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingList_Building");
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Store");

                entity.HasIndex(e => e.AccountId, "idx_AccountId")
                    .IsUnique()
                    .HasFilter("([AccountId] IS NOT NULL)");

                entity.HasIndex(e => e.LocationId, "idx_locationid")
                    .IsUnique()
                    .HasFilter("([LocationId] IS NOT NULL)");

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
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.Store)
                    .HasForeignKey<Store>(d => d.AccountId)
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

                entity.HasOne(d => d.Location)
                    .WithOne(p => p.Store)
                    .HasForeignKey<Store>(d => d.LocationId)
                    .HasConstraintName("FK_Store_Location");
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
