using System.Text.Json;
using Centaur.Domain.Entities;
using Centaur.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centaur.Infrastructure.Data.Configurations;

public class BlockTypeConfiguration : IEntityTypeConfiguration<BlockType>
{
    public void Configure(EntityTypeBuilder<BlockType> builder)
    {
        builder.ToTable("block_types");
        builder.HasKey(bt => bt.Id);
        builder.Property(bt => bt.Name).IsRequired().HasMaxLength(255);
        builder.Property(bt => bt.Slug).IsRequired().HasMaxLength(255);
        builder.HasIndex(bt => bt.Slug).IsUnique();
        builder.Property(bt => bt.CreatedAt).IsRequired();
        builder.Property(bt => bt.UpdatedAt).IsRequired();
        builder.Property(bt => bt.Fields)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<FieldDefinition>>(v, (JsonSerializerOptions?)null) ?? new());
    }
}
