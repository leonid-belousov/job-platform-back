using JobPlatform.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.DAL.Context.Configurations;

public sealed class UserAuthTokenConfiguration : IEntityTypeConfiguration<UserAuthToken>
{
    public void Configure(EntityTypeBuilder<UserAuthToken> builder)
    {
        builder.ToTable("user_auth_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedByIp).HasMaxLength(64);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Type, x.UsedAt });
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasOne(x => x.User)
            .WithMany(x => x.AuthTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
