namespace Centaur.Application.Interfaces;

public interface IPageDemoService
{
    Task EnsureDemoPageAsync(string tenantSchema);
}
