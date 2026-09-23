using JobApplication.Domain.Entities.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplication.Infrastructure.Persistence.Data.Configurations;

internal class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduledAt)
            .IsRequired();

        builder.Property(x => x.Duration)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.MeetingUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.Interviews)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
