using CaseyHub.Core.Entities;
using CaseyHub.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseyHub.API.Data.Configurations;

// ─── BuildingType ────────────────────────────────────────────────────────────
public class BuildingTypeConfiguration : IEntityTypeConfiguration<BuildingType>
{
    public void Configure(EntityTypeBuilder<BuildingType> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Slug).HasMaxLength(50).IsRequired();
        entity.HasIndex(e => e.Slug).IsUnique();
        entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Description).HasMaxLength(500);
    }
}

// ─── PlanningClause ───────────────────────────────────────────────────────────
public class PlanningClauseConfiguration : IEntityTypeConfiguration<PlanningClause>
{
    public void Configure(EntityTypeBuilder<PlanningClause> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ClauseNumber).HasMaxLength(20).IsRequired();
        entity.HasIndex(e => e.ClauseNumber).IsUnique();
        entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
        entity.Property(e => e.Summary).HasMaxLength(1000);
        entity.Property(e => e.OfficialUrl).HasMaxLength(500);
    }
}

// ─── ZoneOverrideRule ─────────────────────────────────────────────────────────
public class ZoneOverrideRuleConfiguration : IEntityTypeConfiguration<ZoneOverrideRule>
{
    public void Configure(EntityTypeBuilder<ZoneOverrideRule> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.ZoneOrOverlayCode).HasMaxLength(20).IsRequired();
        entity.Property(e => e.OutcomeReason).HasMaxLength(1000).IsRequired();

        entity.Property(e => e.Outcome)
              .HasConversion<string>()
              .HasMaxLength(50);

        entity.HasOne(e => e.BuildingType)
              .WithMany(b => b.ZoneOverrideRules)
              .HasForeignKey(e => e.BuildingTypeId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.PlanningClause)
              .WithMany()
              .HasForeignKey(e => e.PlanningClauseId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── PermitRule ───────────────────────────────────────────────────────────────
public class PermitRuleConfiguration : IEntityTypeConfiguration<PermitRule>
{
    public void Configure(EntityTypeBuilder<PermitRule> entity)
    {
        entity.HasKey(e => e.Id);

        // JSONB column — stored as jsonb in Postgres for indexability
        entity.Property(e => e.TriggerContextJson)
              .HasColumnType("jsonb")
              .IsRequired();

        entity.Property(e => e.OutcomeReason).HasMaxLength(1000).IsRequired();

        entity.Property(e => e.RuleType)
              .HasConversion<string>()
              .HasMaxLength(50);

        entity.Property(e => e.Outcome)
              .HasConversion<string>()
              .HasMaxLength(50);

        entity.HasOne(e => e.BuildingType)
              .WithMany(b => b.PermitRules)
              .HasForeignKey(e => e.BuildingTypeId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.PlanningClause)
              .WithMany()
              .HasForeignKey(e => e.PlanningClauseId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── RuleQuestion ─────────────────────────────────────────────────────────────
public class RuleQuestionConfiguration : IEntityTypeConfiguration<RuleQuestion>
{
    public void Configure(EntityTypeBuilder<RuleQuestion> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.FieldKey).HasMaxLength(100).IsRequired();
        entity.Property(e => e.QuestionText).HasMaxLength(500).IsRequired();
        entity.Property(e => e.HelpText).HasMaxLength(500);

        entity.Property(e => e.InputType)
              .HasConversion<string>()
              .HasMaxLength(30);

        // JSONB for options and validation
        entity.Property(e => e.OptionsJson).HasColumnType("jsonb");
        entity.Property(e => e.ValidationJson).HasColumnType("jsonb");

        entity.HasOne(e => e.PermitRule)
              .WithMany(r => r.Questions)
              .HasForeignKey(e => e.PermitRuleId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─── PermitAssessment ─────────────────────────────────────────────────────────
public class PermitAssessmentConfiguration : IEntityTypeConfiguration<PermitAssessment>
{
    public void Configure(EntityTypeBuilder<PermitAssessment> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.NormalisedAddress).HasMaxLength(500).IsRequired();
        entity.Property(e => e.ZoneCode).HasMaxLength(30).IsRequired();
        entity.Property(e => e.OverlayCodes).HasMaxLength(200).IsRequired();
        entity.Property(e => e.BuildingTypeSlug).HasMaxLength(50).IsRequired();

        // Full JSON snapshot — stored as jsonb
        entity.Property(e => e.AnswersJson).HasColumnType("jsonb").IsRequired();

        entity.Property(e => e.OutcomeReason).HasMaxLength(1000).IsRequired();
        entity.Property(e => e.TriggeredClauseNumbers).HasMaxLength(200).IsRequired();

        entity.Property(e => e.Outcome)
              .HasConversion<string>()
              .HasMaxLength(50);

        // Index for analytics queries — find all assessments by outcome + building type
        entity.HasIndex(e => new { e.BuildingTypeSlug, e.Outcome });
        entity.HasIndex(e => e.AssessedAtUtc);

        // FK to User (nullable — anonymous users are allowed)
        entity.HasIndex(e => e.UserId);
    }
}