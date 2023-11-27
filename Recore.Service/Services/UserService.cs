using AutoMapper;
using Recore.Domain.Enums;
using Recore.Service.Helpers;
using Recore.Service.DTOs.Users;
using Recore.Service.Exceptions;
using Recore.Service.Extensions;
using Recore.Service.Interfaces;
using Recore.Data.IRepositories;
using Recore.Domain.Configurations;
using Recore.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Recore.Domain.Entities.Carts;

namespace Recore.Service.Services;

public class UserService : IUserService
{
    private readonly IMapper mapper;
    private readonly IRepository<User> userRepository;
    private readonly IRepository<Cart> cartRepository;
	public UserService(IRepository<User> userRepository, IMapper mapper, IRepository<Cart> cartRepository)
	{
		this.mapper = mapper;
		this.userRepository = userRepository;
		this.cartRepository = cartRepository;
	}

	public async ValueTask<UserResultDto> AddAsync(UserCreationDto dto)
    {
        User existUser = await this.userRepository.SelectAsync(u => u.Phone.Equals(dto.Phone));
        if (existUser is not null)
            throw new AlreadyExistException($"This user is already exists with phone = {dto.Phone}");

        var mappedUser = this.mapper.Map<User>(dto);
        mappedUser.Password = PasswordHash.Encrypt(mappedUser.Password);
        await this.userRepository.CreateAsync(mappedUser);
        await this.userRepository.SaveAsync();

        var cart = new Cart { UserId = mappedUser.Id };
        await this.cartRepository.CreateAsync(cart);

        var result = this.mapper.Map<UserResultDto>(mappedUser);
        return result;
    }

    public async ValueTask<UserResultDto> ModifyAsync(UserUpdateDto dto)
    {
        User existUser = await this.userRepository.SelectAsync(u => u.Id.Equals(dto.Id))
            ?? throw new NotFoundException($"This user is not found with ID = {dto.Id}");

        this.mapper.Map(dto, existUser);
        existUser.Password = PasswordHash.Encrypt(dto.Password);
        this.userRepository.Update(existUser);
        await this.userRepository.SaveAsync();
        
        var result = this.mapper.Map<UserResultDto>(existUser);
        return result;
    }

    public async ValueTask<bool> RemoveAsync(long id)
    {
        User existUser = await this.userRepository.SelectAsync(u => u.Id.Equals(id))
            ?? throw new NotFoundException($"This user is not found with ID = {id}");

        this.userRepository.Delete(existUser);
        await this.userRepository.SaveAsync();
        return true;
    }

    public async ValueTask<UserResultDto> RetrieveByIdAsync(long id)
    {
        User existUser = await this.userRepository.SelectAsync(u => u.Id.Equals(id))
            ?? throw new NotFoundException($"This user is not found with ID = {id}");

        var result = this.mapper.Map<UserResultDto>(existUser);
        return result;
    }

    public async ValueTask<IEnumerable<UserResultDto>> RetrieveAllAsync(PaginationParams @params, Filter filter, string search = null)
    {
        var users = (await this.userRepository.SelectAll()
            .ToPagedList(@params)
            .ToListAsync())
            .OrderBy(filter);
        
        var result = search is null ? users :
            users.Where(user => user.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase));

        var mappedUsers = this.mapper.Map<List<UserResultDto>>(result);
        return mappedUsers;
    }

    public async ValueTask<IEnumerable<UserResultDto>> RetrieveAllAsync()
    {
        var users = await this.userRepository.SelectAll()
            .ToListAsync();
        var result = this.mapper.Map<IEnumerable<UserResultDto>>(users);
        return result;
    }

    public async ValueTask<UserResultDto> UpgradeRoleAsync(long id, UserRole role)
	{
		User existUser = await this.userRepository.SelectAsync(u => u.Id.Equals(id))
			?? throw new NotFoundException($"This user is not found with ID = {id}");

        existUser.Role = role;
		await this.userRepository.SaveAsync();

		var result = this.mapper.Map<UserResultDto>(existUser);
		return result;
	}
}
