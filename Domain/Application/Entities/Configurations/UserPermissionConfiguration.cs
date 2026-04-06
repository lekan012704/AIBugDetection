using Domain.Application.Entities.UserPermissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        builder.HasKey(up => up.Id);

        builder.Property(up => up.Id)
            .HasConversion(
                id => id.Value,
                value => new UserPermissionId(value))
            .ValueGeneratedOnAdd();

        // ✅ Match EXACTLY the length of Permissions.PermissionId
        builder.Property(up => up.PermissionId)
            .IsRequired()
            .HasMaxLength(450); // ← changed from 100 to 450

        // ✅ Match EXACTLY the length of Users.Id
        builder.Property(up => up.UserId)
            .IsRequired()
            .HasMaxLength(450); // ← match Users.Id length

        builder.Property(up => up.IsActive)
            .IsRequired()
            .HasDefaultValue(true);


        builder.HasOne(up => up.Permission)
.WithMany(p => p.UserPermissions)  // ← specify the collection
.HasForeignKey(up => up.PermissionId)
.HasPrincipalKey(p => p.PermissionId)
.OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(up => up.User)
            .WithMany(u => u.UserPermissions)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint
        builder.HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique();

    }
}