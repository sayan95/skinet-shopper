﻿using Microsoft.EntityFrameworkCore;
using Skinet.Entities.Common;
using Skinet.Entities.Entities;
using Skinet.Persistence.Configurations;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Skinet.Persistence
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions options) : base(options)
        {
        }

        // application dbsets
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }

        // skinet entity configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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