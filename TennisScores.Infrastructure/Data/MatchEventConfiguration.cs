using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TennisScores.Domain.Entities;

namespace TennisScores.Infrastructure.Data;

public sealed class MatchEventConfiguration : IEntityTypeConfiguration<MatchEvent>
{
    public void Configure(EntityTypeBuilder<MatchEvent> builder)
    {
        builder.ToTable("MatchEvents");
        builder.HasKey(matchEvent => matchEvent.Id);

        builder.Property(matchEvent => matchEvent.EventType)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(matchEvent => matchEvent.EventVersion)
            .IsRequired();
        builder.Property(matchEvent => matchEvent.Payload)
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(matchEvent => matchEvent.OccurredAt)
            .IsRequired();

        builder.HasIndex(matchEvent => new
            {
                matchEvent.MatchId,
                matchEvent.Sequence
            })
            .IsUnique();

        builder.HasOne(matchEvent => matchEvent.Match)
            .WithMany()
            .HasForeignKey(matchEvent => matchEvent.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
