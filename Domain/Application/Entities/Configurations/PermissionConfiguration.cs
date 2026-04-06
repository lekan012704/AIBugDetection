//using Domain.Application.Entities.Permissions;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Domain.Application.Entities.Configurations
//{
//    internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
//    {
//        public void Configure(EntityTypeBuilder<Permission> builder)
//        {
//            // Specify table name and schema
//            builder.ToTable("Permission");

//            // Primary key
//            builder.HasKey(p => p.PermissionId);

//            // Properties
//            builder.Property(p => p.PermissionId)
//                .IsRequired()
//                .ValueGeneratedNever();

//        }
//    }
//}
