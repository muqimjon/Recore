using AutoMapper;
using Recore.Data.IRepositories;
using Recore.Service.Exceptions;
using Recore.Service.Extensions;
using Recore.Service.Interfaces;
using Recore.Domain.Configurations;
using Microsoft.EntityFrameworkCore;
using Recore.Service.DTOs.WareHouses;
using Recore.Domain.Entities.WareHouses;

namespace Recore.Service.Services;

public class WareHouseService : IWareHouseService
{
	private readonly IMapper mapper;
	private readonly IRepository<WareHouse> repository;
	public WareHouseService(IRepository<WareHouse> repository, IMapper mapper)
	{
		this.mapper = mapper;
		this.repository = repository;
	}

	public async ValueTask<WareHouseResultDto> AddAsync(WareHouseCreationDto dto)
	{
		WareHouse existWareHouse = await this.repository.SelectAsync(u => u.Name.Equals(dto.Name));
		if (existWareHouse is not null)
			throw new AlreadyExistException($"This WareHouse is already exists with phone = {dto.Name}");

		var mappedWareHouse = this.mapper.Map<WareHouse>(dto);
		await this.repository.CreateAsync(mappedWareHouse);
		await this.repository.SaveAsync();

		var result = this.mapper.Map<WareHouseResultDto>(mappedWareHouse);
		return result;
	}

	public async ValueTask<WareHouseResultDto> ModifyAsync(WareHouseUpdateDto dto)
	{
		WareHouse existWareHouse = await this.repository.SelectAsync(u => u.Id.Equals(dto.Id))
			?? throw new NotFoundException($"This WareHouse is not found with ID = {dto.Id}");

		this.mapper.Map(dto, existWareHouse);
		this.repository.Update(existWareHouse);
		await this.repository.SaveAsync();

		var result = this.mapper.Map<WareHouseResultDto>(existWareHouse);
		return result;
	}

	public async ValueTask<bool> RemoveAsync(long id)
	{
		WareHouse existWareHouse = await this.repository.SelectAsync(u => u.Id.Equals(id))
			?? throw new NotFoundException($"This WareHouse is not found with ID = {id}");

		this.repository.Delete(existWareHouse);
		await this.repository.SaveAsync();
		return true;
	}

	public async ValueTask<WareHouseResultDto> RetrieveByIdAsync(long id)
	{
		WareHouse existWareHouse = await this.repository.SelectAsync(u => u.Id.Equals(id))
			?? throw new NotFoundException($"This WareHouse is not found with ID = {id}");

		var result = this.mapper.Map<WareHouseResultDto>(existWareHouse);
		return result;
	}

	public async ValueTask<IEnumerable<WareHouseResultDto>> RetrieveAllAsync(PaginationParams @params, Filter filter, string search = null)
	{
		var WareHouses = (await this.repository.SelectAll()
			.ToPaginate(@params)
			.ToListAsync())
			.OrderBy(filter);

		var result = WareHouses.Where(WareHouse => WareHouse.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
		var mappedWareHouses = this.mapper.Map<List<WareHouseResultDto>>(result);
		return mappedWareHouses;
	}

	public async ValueTask<IEnumerable<WareHouseResultDto>> RetrieveAllAsync()
	{
		var WareHouses = await this.repository.SelectAll()
			.ToListAsync();
		var result = this.mapper.Map<IEnumerable<WareHouseResultDto>>(WareHouses);
		return result;
	}
}
