using JobApplication.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace JobApplication.Infrastructure.Persistence.IdentityData.DbContexts;

public class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) 
    : IdentityDbContext<ApplicationUser,ApplicationRole,Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .ToTable("Users");

        builder.Entity<ApplicationUser>()
            .OwnsMany(x => x.RefreshTokens, a =>
        {
            a.HasIndex(t => t.Token)
            .IsUnique();
        });

        builder.Entity<IdentityRole>()
            .ToTable("Roles");

        builder.Entity<IdentityUserRole<Guid>>()
            .ToTable("UserRoles");
    }
}
