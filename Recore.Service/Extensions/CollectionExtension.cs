using Newtonsoft.Json;
using Recore.Domain.Commons;
using Recore.Domain.Configurations;
using Recore.Domain.Configurations.Pagination;
using Recore.Service.Exceptions;
using Recore.Service.Helpers;

namespace Recore.Service.Extensions;

public static class CollectionExtension
{
    public static IQueryable<T> ToPaginate<T>(this IQueryable<T> values, PaginationParams @params)
        => values.Skip((@params.PageIndex - 1) * @params.PageSize).Take(@params.PageSize);

    public static IQueryable<TEntity> ToPagedList<TEntity>(this IQueryable<TEntity> entities, PaginationParams @params)
        where TEntity : Auditable
    {
        if (@params.PageSize == 0 || @params.PageIndex == 0)
            @params = new PaginationParams()
            {
                PageSize = @params.PageSize > 0 ? @params.PageSize : 1,
                PageIndex = 1
            };

        var metaData = new PaginationMetaData(entities.Count(), @params);
        var json = JsonConvert.SerializeObject(metaData);

        if (HttpContextHelper.ResponseHeaders != null)
        {
            if (HttpContextHelper.ResponseHeaders.ContainsKey("X-Pagination"))
                HttpContextHelper.ResponseHeaders.Remove("X-Pagination");

            HttpContextHelper.ResponseHeaders.Add("X-Pagination", json);
        }

        return entities.ToPaginate(@params);
    }


    public static IEnumerable<TEntity> OrderBy<TEntity>(this IEnumerable<TEntity> collect, Filter filter) where TEntity : Auditable
    {
        var prop = filter.OrderBy ?? "Id";

        var property = typeof(TEntity).GetProperties().FirstOrDefault(n
            => n.Name.Equals(prop, StringComparison.OrdinalIgnoreCase))
            ?? throw new CustomException(400, "Property that does not exist");

        if (filter.IsDesc)
        {
            if (prop == "Id")
                return collect.OrderByDescending(x => x.Id);
            return collect.OrderByDescending(x => property.GetValue(x));
        }

        if (prop == "Id")
            return collect.OrderBy(x => x.Id);
        return collect.OrderBy(x => property.GetValue(x));
    }
}
