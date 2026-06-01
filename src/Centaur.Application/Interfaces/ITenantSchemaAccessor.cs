namespace Centaur.Application.Interfaces;

public interface ITenantSchemaAccessor
{
    string? CurrentSchema { get; }
}
