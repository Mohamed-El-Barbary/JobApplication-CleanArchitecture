using JobApplication.Domain.Entities.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplication.Infrastructure.Persistence.Data.Configurations;

internal class JobCandidateApplicationConfiguration : IEntityTypeConfiguration<JobCandidateApplication>
{
    public void Configure(EntityTypeBuilder<JobCandidateApplication> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("AppliedAt");

        builder.Ignore(x => x.ApplicationStatus);

        builder.HasOne(x => x.Candidate)
            .WithMany(x => x.JobApplications)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Job)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CandidateId, x.JobId })
            .IsUnique();
    }
}
