﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Skinet.Entities.Common;
using Skinet.Entities.Entities;
using Skinet.Entities.Entities.OrderAggregate;
using Skinet.Persistence.Configurations;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Skinet.Persistence
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) 
        : base(options)
        {}

        // application dbsets
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }

        // skinet entity configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            if(Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach(var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(x => x.PropertyType == typeof(decimal));

                    var dateTimeProperties = entityType.ClrType.GetProperties().Where(x => x.PropertyType == typeof(DateTimeOffset));

                    foreach(var property in properties)
                    {
                        modelBuilder.Entity(entityType.Name).Property(property.Name)
                            .HasConversion<double>();
                    }

                    foreach(var property in dateTimeProperties){
                        modelBuilder.Entity(entityType.Name).Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }

                }
            }
        }


        // deafule action setting on save changes
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach(var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = "default";
                        entry.Entity.CreatedAt = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = "default";
                        entry.Entity.LastModifiedAt = DateTime.Now;
                        break;
                }
                    
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
