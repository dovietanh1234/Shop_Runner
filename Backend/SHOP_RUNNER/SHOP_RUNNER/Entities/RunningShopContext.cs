using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SHOP_RUNNER.Entities;

public partial class RunningShopContext : DbContext
{
    public RunningShopContext()
    {
    }

    public RunningShopContext(DbContextOptions<RunningShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderProductsDetail> OrderProductsDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-DKL7C0F\\SQLEXPRESS;Database=Running_shop;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__brand__3213E83F373E4C22");

            entity.ToTable("brand");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("carts");

            entity.Property(e => e.BuyQty)
                .HasDefaultValueSql("((1))")
                .HasColumnName("buy_qty");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__carts__product_i__5812160E");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__carts__user_id__571DF1D5");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3213E83F829CDC54");

            entity.ToTable("categories");

            entity.HasIndex(e => e.Name, "UQ__categori__72E12F1B52937DC7").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__colors__3213E83F0A6FE04E");

            entity.ToTable("colors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__genders__3213E83F21A23AEC");

            entity.ToTable("genders");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orders__3213E83FB0179D9E");

            entity.ToTable("orders");

            entity.HasIndex(e => e.InvoiceId, "UQ__orders__F58DFD4899AC5C1D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GrandTotal)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("grand_total");
            entity.Property(e => e.InvoiceId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("invoice_id");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("payment_method");
            entity.Property(e => e.ShippingAddress)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("shipping_address");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("((1))")
                .HasColumnName("status");
            entity.Property(e => e.Tel)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tel");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__orders__user_id__5DCAEF64");
        });

        modelBuilder.Entity<OrderProductsDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("order_products_detail");

            entity.Property(e => e.BuyQty)
                .HasDefaultValueSql("((1))")
                .HasColumnName("buy_qty");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__order_pro__order__628FA481");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__order_pro__produ__619B8048");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__products__3213E83F3F5E1F88");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.GenderId).HasColumnName("gender_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.SizeId).HasColumnName("size_id");
            entity.Property(e => e.Thumbnail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("thumbnail");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__products__brand___5165187F");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__products__catego__4E88ABD4");

            entity.HasOne(d => d.Color).WithMany(p => p.Products)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__products__color___534D60F1");

            entity.HasOne(d => d.Gender).WithMany(p => p.Products)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__products__gender__5070F446");

            entity.HasOne(d => d.Size).WithMany(p => p.Products)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__products__size_i__52593CB8");

            entity.HasOne(d => d.User).WithMany(p => p.Products)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__products__user_i__4F7CD00D");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__size__3213E83F1A3161D3");

            entity.ToTable("size");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F5701C047");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E61646BA6D761").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fullname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("passwordHash");
            entity.Property(e => e.PasswordResetToken)
                .HasMaxLength(350)
                .IsUnicode(false)
                .HasColumnName("passwordResetToken");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("passwordSalt");
            entity.Property(e => e.ResetTokenExpires).HasColumnType("datetime");
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Tel)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tel");
            entity.Property(e => e.VerificationToken)
                .HasMaxLength(350)
                .IsUnicode(false)
                .HasColumnName("verificationToken");
            entity.Property(e => e.VerifiedAt)
                .HasColumnType("datetime")
                .HasColumnName("verifiedAt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
