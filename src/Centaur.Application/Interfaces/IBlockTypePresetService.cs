using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IBlockTypePresetService
{
    Task EnsurePresetsAsync();
    IReadOnlyList<BlockTypeSummaryDto> GetPresetOverview();
}
