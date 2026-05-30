using JobPlatform.Core.Entities.Questionnaires;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class QuestionnaireConfiguration : IEntityTypeConfiguration<Questionnaire>
{
    public void Configure(EntityTypeBuilder<Questionnaire> builder)
    {
        builder.ToTable("questionnaires");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasMany(x => x.Sections).WithOne(x => x.Questionnaire).HasForeignKey(x => x.QuestionnaireId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Responses).WithOne(x => x.Questionnaire).HasForeignKey(x => x.QuestionnaireId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class QuestionnaireSectionConfiguration : IEntityTypeConfiguration<QuestionnaireSection>
{
    public void Configure(EntityTypeBuilder<QuestionnaireSection> builder)
    {
        builder.ToTable("questionnaire_sections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.HasIndex(x => x.QuestionnaireId);
        builder.HasIndex(x => new { x.QuestionnaireId, x.SortOrder });
        builder.HasMany(x => x.Questions).WithOne(x => x.Section).HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class QuestionnaireQuestionConfiguration : IEntityTypeConfiguration<QuestionnaireQuestion>
{
    public void Configure(EntityTypeBuilder<QuestionnaireQuestion> builder)
    {
        builder.ToTable("questionnaire_questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.QuestionType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Placeholder).HasMaxLength(500);
        builder.Property(x => x.HelpText).HasMaxLength(1000);
        builder.Property(x => x.ValidationRules).HasColumnType("jsonb");
        builder.Property(x => x.VisibilityCondition).HasColumnType("jsonb");
        builder.HasIndex(x => x.SectionId);
        builder.HasIndex(x => new { x.SectionId, x.SortOrder });
        builder.HasMany(x => x.Options).WithOne(x => x.Question).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class QuestionnaireOptionConfiguration : IEntityTypeConfiguration<QuestionnaireOption>
{
    public void Configure(EntityTypeBuilder<QuestionnaireOption> builder)
    {
        builder.ToTable("questionnaire_options");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.QuestionId);
        builder.HasIndex(x => new { x.QuestionId, x.SortOrder });
    }
}

public sealed class QuestionnaireResponseConfiguration : IEntityTypeConfiguration<QuestionnaireResponse>
{
    public void Configure(EntityTypeBuilder<QuestionnaireResponse> builder)
    {
        builder.ToTable("questionnaire_responses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.QuestionnaireId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SubmittedAt);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasMany(x => x.Answers).WithOne(x => x.Response).HasForeignKey(x => x.ResponseId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class QuestionnaireAnswerConfiguration : IEntityTypeConfiguration<QuestionnaireAnswer>
{
    public void Configure(EntityTypeBuilder<QuestionnaireAnswer> builder)
    {
        builder.ToTable("questionnaire_answers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasColumnType("jsonb");
        builder.HasIndex(x => x.ResponseId);
        builder.HasIndex(x => x.QuestionId);
        builder.HasIndex(x => x.FileId);
    }
}
