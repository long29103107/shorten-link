using System.Linq.Expressions;
using System.Reflection;

namespace ShortenLink.Core.Querying;

public static class DynamicSortExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string? sort,
        IEnumerable<string> allowedProperties)
    {
        ArgumentNullException.ThrowIfNull(source);
        var allowed = new HashSet<string>(allowedProperties, StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(sort)) return source;

        IOrderedQueryable<T>? ordered = null;
        foreach (var rawField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = rawField.StartsWith('-');
            var propertyPath = rawField.TrimStart('-', '+').Trim();
            if (!allowed.Contains(propertyPath))
            {
                throw new ArgumentException($"Sorting by '{propertyPath}' is not allowed.", nameof(sort));
            }

            ordered = ApplyOrder(source, ordered, propertyPath, descending);
        }

        return ordered ?? source;
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(
        IQueryable<T> source,
        IOrderedQueryable<T>? ordered,
        string propertyPath,
        bool descending)
    {
        var parameter = Expression.Parameter(typeof(T), "item");
        Expression property = parameter;
        var currentType = typeof(T);
        foreach (var segment in propertyPath.Split('.'))
        {
            var info = currentType.GetProperty(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                ?? throw new ArgumentException($"Property '{propertyPath}' does not exist.", nameof(propertyPath));
            property = Expression.Property(property, info);
            currentType = info.PropertyType;
        }

        var lambda = Expression.Lambda(property, parameter);
        var method = ordered is null
            ? descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy)
            : descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);
        var target = ordered ?? source;
        return (IOrderedQueryable<T>)typeof(Queryable).GetMethods()
            .Single(candidate => candidate.Name == method && candidate.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), currentType)
            .Invoke(null, [target, lambda])!;
    }
}
