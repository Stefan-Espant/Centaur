using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;

namespace Centaur.Application.Tests.Services;

public class ApiKeyServiceTests
{
    private readonly MockApiKeyRepository _repository = new();

    [Fact]
    public async Task CreateAsync_ReturnsKeyOnlyOnceInResponse()
    {
        var tenantId = Guid.NewGuid();
        var service = new ApiKeyService(_repository);

        var result = await service.CreateAsync(tenantId, new CreateApiKeyRequest("Frontend", null));

        Assert.Equal("Frontend", result.Label);
        Assert.Equal(tenantId, result.TenantId);
        Assert.False(string.IsNullOrWhiteSpace(result.Key));
    }
}
