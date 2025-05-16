using AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthApi.Database.EntityConfig;

public class AppUserEntityConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Id).IsRequired().HasMaxLength(32);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(32);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(64);
        builder.Property(x => x.PasswordHash).HasColumnName("hash").IsRequired().HasMaxLength(255);
        //builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        //builder.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}