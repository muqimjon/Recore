using Recore.Domain.Configurations;
using Recore.Service.DTOs.BonusSetting;
using Recore.Service.DTOs.WareHouses;

namespace Recore.Service.Interfaces;

public interface IWareHouseService
{
	ValueTask<WareHouseResultDto> AddAsync(WareHouseCreationDto dto);
	ValueTask<WareHouseResultDto> ModifyAsync(WareHouseUpdateDto dto);
	ValueTask<bool> RemoveAsync(long id);
	ValueTask<WareHouseResultDto> RetrieveByIdAsync(long id);
	ValueTask<IEnumerable<WareHouseResultDto>> RetrieveAllAsync(PaginationParams @params, Filter filter=null, string search = null);
	ValueTask<IEnumerable<WareHouseResultDto>> RetrieveAllAsync();
}
