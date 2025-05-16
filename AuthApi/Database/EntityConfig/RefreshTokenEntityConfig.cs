using AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthApi.Database.EntityConfig;

public class RefreshTokenEntityConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Token).HasColumnName("refreshTokenSha1")
            .HasMaxLength(40);
        //builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        //builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}