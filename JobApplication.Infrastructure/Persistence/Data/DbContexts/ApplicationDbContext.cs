using JobApplication.Domain.Entities.Business;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobApplication.Infrastructure.Persistence.Data.DbContexts;

public class ApplicationDbContext : DbContext
{

    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobCandidateApplication> JobApplications { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Recruiter> Recruiters { get; set; }
    public DbSet<Interview> Interviews { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
