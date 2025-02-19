﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Recore.Data.IRepositories;
using Recore.Data.Repositories;
using Recore.Service.Interfaces;
using Recore.Service.Mappers;
using Recore.Service.Services;
using System.Text;

namespace Recore.WebApi.Extensions;

public static class ServicesCollection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<IAddressService, AddressService>();
		    services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IBonusSettingService, BonusSettingService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
		    services.AddScoped<IVehicleService, VehicleService>();
		    services.AddScoped<ISupplierService, SupplierService>();
		    services.AddScoped<IWareHouseService, WareHouseService>();
		    services.AddScoped<IInventoryService, InventoryService>();
    }

    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
		services.AddAuthentication(x =>
		{
			x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(o =>
		{
			var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
			o.SaveToken = true;
			o.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = configuration["JWT:Issuer"],
				ValidAudience = configuration["JWT:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(key)
			};
		});
	}

	public static void ConfigureSwagger(this IServiceCollection services)
	{
		services.AddSwaggerGen(setup =>
		{
			// Include 'SecurityScheme' to use JWT Authentication
			var jwtSecurityScheme = new OpenApiSecurityScheme
			{
				BearerFormat = "JWT",
				Name = "JWT Authentication",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = JwtBearerDefaults.AuthenticationScheme,
				Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

				Reference = new OpenApiReference
				{
					Id = JwtBearerDefaults.AuthenticationScheme,
					Type = ReferenceType.SecurityScheme
				}
			};

			setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

			setup.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{ jwtSecurityScheme, Array.Empty<string>() }
				});
		});
	}
}
