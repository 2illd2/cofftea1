using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<address> addresses { get; set; }

    public virtual DbSet<article> articles { get; set; }

    public virtual DbSet<article_category> article_categories { get; set; }

    public virtual DbSet<cart> carts { get; set; }

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<chat_message> chat_messages { get; set; }

    public virtual DbSet<chat_thread> chat_threads { get; set; }

    public virtual DbSet<coupon> coupons { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<order_item> order_items { get; set; }

    public virtual DbSet<order_status> order_statuses { get; set; }

    public virtual DbSet<payment> payments { get; set; }

    public virtual DbSet<product> products { get; set; }

    public virtual DbSet<product_detail> product_details { get; set; }

    public virtual DbSet<review> reviews { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<shipment> shipments { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<address>(entity =>
        {
            entity.HasKey(e => e.id).HasName("addresses_pkey");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.city).HasMaxLength(100);
            entity.Property(e => e.country).HasMaxLength(100);
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.is_default).HasDefaultValue(false);
            entity.Property(e => e.line1).HasMaxLength(200);
            entity.Property(e => e.postal_code).HasMaxLength(20);

            entity.HasOne(d => d.user).WithMany(p => p.addresses)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("addresses_user_id_fkey");
        });

        modelBuilder.Entity<article>(entity =>
        {
            entity.HasKey(e => e.id).HasName("articles_pkey");

            entity.HasIndex(e => e.slug, "articles_slug_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.cover_image_url).HasMaxLength(500);
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.is_published).HasDefaultValue(false);
            entity.Property(e => e.published_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.slug).HasMaxLength(200);
            entity.Property(e => e.title).HasMaxLength(200);

            entity.HasOne(d => d.category).WithMany(p => p.articles)
                .HasForeignKey(d => d.category_id)
                .HasConstraintName("articles_category_id_fkey");

            entity.HasMany(d => d.products).WithMany(p => p.articles)
                .UsingEntity<Dictionary<string, object>>(
                    "article_product",
                    r => r.HasOne<product>().WithMany()
                        .HasForeignKey("product_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("article_products_product_id_fkey"),
                    l => l.HasOne<article>().WithMany()
                        .HasForeignKey("article_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("article_products_article_id_fkey"),
                    j =>
                    {
                        j.HasKey("article_id", "product_id").HasName("article_products_pkey");
                        j.ToTable("article_products");
                    });
        });

        modelBuilder.Entity<article_category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("article_categories_pkey");

            entity.HasIndex(e => e.name, "article_categories_name_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.name).HasMaxLength(100);
        });

        modelBuilder.Entity<cart>(entity =>
        {
            entity.HasKey(e => e.id).HasName("cart_pkey");

            entity.ToTable("cart");

            entity.HasIndex(e => e.user_id, "ix_cart_user_id");

            entity.HasIndex(e => new { e.user_id, e.product_id }, "ux_cart_user_product_not_deleted")
                .IsUnique()
                .HasFilter("(deleted = false)");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.added_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.price).HasPrecision(10, 2);

            entity.HasOne(d => d.product).WithMany(p => p.carts)
                .HasForeignKey(d => d.product_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cart_product_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.carts)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cart_user_id_fkey");
        });

        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("categories_pkey");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.name).HasMaxLength(100);

            entity.HasOne(d => d.parent).WithMany(p => p.Inverseparent)
                .HasForeignKey(d => d.parent_id)
                .HasConstraintName("categories_parent_id_fkey");
        });

        modelBuilder.Entity<chat_message>(entity =>
        {
            entity.HasKey(e => e.id).HasName("chat_messages_pkey");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.sender).WithMany(p => p.chat_messages)
                .HasForeignKey(d => d.sender_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_messages_sender_id_fkey");

            entity.HasOne(d => d.thread).WithMany(p => p.chat_messages)
                .HasForeignKey(d => d.thread_id)
                .HasConstraintName("chat_messages_thread_id_fkey");
        });

        modelBuilder.Entity<chat_thread>(entity =>
        {
            entity.HasKey(e => e.id).HasName("chat_threads_pkey");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'open'::character varying");

            entity.HasOne(d => d.client).WithMany(p => p.chat_threadclients)
                .HasForeignKey(d => d.client_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_threads_client_id_fkey");

            entity.HasOne(d => d.consultant).WithMany(p => p.chat_threadconsultants)
                .HasForeignKey(d => d.consultant_id)
                .HasConstraintName("chat_threads_consultant_id_fkey");
        });

        modelBuilder.Entity<coupon>(entity =>
        {
            entity.HasKey(e => e.id).HasName("coupons_pkey");

            entity.HasIndex(e => e.code, "coupons_code_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.code).HasMaxLength(50);
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.discount_type).HasMaxLength(20);
            entity.Property(e => e.min_total).HasPrecision(10, 2);
            entity.Property(e => e.valid_from).HasColumnType("timestamp without time zone");
            entity.Property(e => e.valid_to).HasColumnType("timestamp without time zone");
            entity.Property(e => e.value).HasPrecision(10, 2);
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.id).HasName("orders_pkey");

            entity.HasIndex(e => e.address_id, "ix_orders_address_id");

            entity.HasIndex(e => e.user_id, "ix_orders_user_id");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.payment_status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.total).HasPrecision(10, 2);

            entity.HasOne(d => d.address).WithMany(p => p.orders)
                .HasForeignKey(d => d.address_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_address_id_fkey");

            entity.HasOne(d => d.status).WithMany(p => p.orders)
                .HasForeignKey(d => d.status_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_status_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.orders)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_user_id_fkey");

            entity.HasMany(d => d.coupons).WithMany(p => p.orders)
                .UsingEntity<Dictionary<string, object>>(
                    "order_coupon",
                    r => r.HasOne<coupon>().WithMany()
                        .HasForeignKey("coupon_id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("order_coupons_coupon_id_fkey"),
                    l => l.HasOne<order>().WithMany()
                        .HasForeignKey("order_id")
                        .HasConstraintName("order_coupons_order_id_fkey"),
                    j =>
                    {
                        j.HasKey("order_id", "coupon_id").HasName("order_coupons_pkey");
                        j.ToTable("order_coupons");
                    });
        });

        modelBuilder.Entity<order_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("order_items_pkey");

            entity.HasIndex(e => e.order_id, "ix_order_items_order_id");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.unit_price).HasPrecision(10, 2);

            entity.HasOne(d => d.order).WithMany(p => p.order_items)
                .HasForeignKey(d => d.order_id)
                .HasConstraintName("order_items_order_id_fkey");

            entity.HasOne(d => d.product).WithMany(p => p.order_items)
                .HasForeignKey(d => d.product_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_items_product_id_fkey");
        });

        modelBuilder.Entity<order_status>(entity =>
        {
            entity.HasKey(e => e.id).HasName("order_statuses_pkey");

            entity.HasIndex(e => e.name, "order_statuses_name_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.name).HasMaxLength(50);
        });

        modelBuilder.Entity<payment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("payments_pkey");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.amount).HasPrecision(10, 2);
            entity.Property(e => e.method).HasMaxLength(50);
            entity.Property(e => e.paid_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.provider_txn_id).HasMaxLength(100);
            entity.Property(e => e.status).HasMaxLength(50);

            entity.HasOne(d => d.order).WithMany(p => p.payments)
                .HasForeignKey(d => d.order_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_order_id_fkey");
        });

        modelBuilder.Entity<product>(entity =>
        {
            entity.HasKey(e => e.id).HasName("products_pkey");

            entity.HasIndex(e => e.type, "ix_products_type");

            entity.HasIndex(e => e.sku, "products_sku_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.image_url).HasMaxLength(500);
            entity.Property(e => e.name).HasMaxLength(200);
            entity.Property(e => e.origin_country).HasMaxLength(100);
            entity.Property(e => e.origin_region).HasMaxLength(100);
            entity.Property(e => e.price).HasPrecision(10, 2);
            entity.Property(e => e.processing).HasMaxLength(50);
            entity.Property(e => e.quantity).HasDefaultValue(0);
            entity.Property(e => e.roast_level).HasMaxLength(50);
            entity.Property(e => e.sku).HasMaxLength(50);
            entity.Property(e => e.type).HasMaxLength(50);

            entity.HasOne(d => d.category).WithMany(p => p.products)
                .HasForeignKey(d => d.category_id)
                .HasConstraintName("products_category_id_fkey");
        });

        modelBuilder.Entity<product_detail>(entity =>
        {
            entity.HasKey(e => e.id).HasName("product_details_pkey");

            entity.HasIndex(e => e.product_id, "product_details_product_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.deleted).HasDefaultValue(false);

            entity.HasOne(d => d.product).WithOne(p => p.product_detail)
                .HasForeignKey<product_detail>(d => d.product_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_details_product_id_fkey");
        });

        modelBuilder.Entity<review>(entity =>
        {
            entity.HasKey(e => e.id).HasName("reviews_pkey");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.is_moderated).HasDefaultValue(false);

            entity.HasOne(d => d.product).WithMany(p => p.reviews)
                .HasForeignKey(d => d.product_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reviews_product_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.reviews)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reviews_user_id_fkey");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("roles_pkey");

            entity.HasIndex(e => e.name, "roles_name_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.name).HasMaxLength(50);
        });

        modelBuilder.Entity<shipment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("shipments_pkey");

            entity.HasIndex(e => e.order_id, "shipments_order_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.carrier).HasMaxLength(100);
            entity.Property(e => e.delivered_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.shipped_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.status).HasMaxLength(50);
            entity.Property(e => e.tracking_number).HasMaxLength(100);

            entity.HasOne(d => d.order).WithOne(p => p.shipment)
                .HasForeignKey<shipment>(d => d.order_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shipments_order_id_fkey");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("users_pkey");

            entity.HasIndex(e => e.email, "ix_users_email");

            entity.HasIndex(e => e.email, "users_email_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.deleted).HasDefaultValue(false);
            entity.Property(e => e.email).HasMaxLength(150);
            entity.Property(e => e.first_name).HasMaxLength(100);
            entity.Property(e => e.last_name).HasMaxLength(100);
            entity.Property(e => e.middle_name).HasMaxLength(100);
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(30);

            entity.HasOne(d => d.role).WithMany(p => p.users)
                .HasForeignKey(d => d.role_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
