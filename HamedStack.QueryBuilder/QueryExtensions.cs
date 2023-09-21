// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace HamedStack.QueryBuilder;

/// <summary>
/// Provides extensions for working with <see cref="Query"/> objects.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Generates a string representation of the query for debugging purposes with optional indentation.
    /// </summary>
    /// <param name="query">The query to generate the string representation for.</param>
    /// <param name="level">The level of indentation (default is 0).</param>
    /// <returns>A string representation of the query.</returns>
    public static string Print(this Query query, int level = 0)
    {
        var sb = new StringBuilder();
        var indent = new string(' ', level * 2);

        if (query.Property != null)
        {
            string valueStr;

            if (query.Value is string strValue)
            {
                valueStr = $"\"{strValue}\"";
            }
            else if (query.Value is IEnumerable enumerable and not string)
            {
                var elements = new List<string>();
                foreach (var elem in enumerable)
                {
                    elements.Add(elem?.ToString() ?? "null");
                }
                valueStr = $"[{string.Join(", ", elements)}]";
            }
            else
            {
                valueStr = query.Value?.ToString() ?? "null";
            }

            sb.AppendLine($"{indent}Property: {query.Property}, Filter: {query.Filter}, Value: {valueStr}");
        }
        else
        {
            sb.AppendLine($"{indent}Operator: {query.Operator}, Not: {query.Not}");
        }

        if (query.Queries == null) return sb.ToString();

        foreach (var subQuery in query.Queries)
        {
            sb.Append(subQuery.Print(level + 1));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a <see cref="Query"/> object into a LINQ expression.
    /// </summary>
    /// <typeparam name="T">The type of entity to filter.</typeparam>
    /// <param name="query">The query to convert.</param>
    /// <returns>A LINQ expression representing the query.</returns>
    public static Expression<Func<T, bool>> ToExpression<T>(this Query query)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var expr = BuildExpression(query, param);
        return Expression.Lambda<Func<T, bool>>(expr, param);
    }

    /// <summary>
    /// Converts a <see cref="Query"/> object into a LINQ expression for the "All" filter.
    /// </summary>
    private static Expression BuildExpression(Query query, Expression param)
    {
        if (query.Queries != null && query.Queries.Any())
        {
            var expressions = query.Queries.Select(r => BuildExpression(r, param)).ToList();
            var combined = expressions.First();

            foreach (var expression in expressions.Skip(1))
            {
                combined = query.Operator switch
                {
                    Operator.And => Expression.AndAlso(combined, expression),
                    Operator.Or => Expression.OrElse(combined, expression),
                    Operator.Xor => Expression.OrElse(
                        Expression.AndAlso(combined, Expression.Not(expression)),
                        Expression.AndAlso(Expression.Not(combined), expression)
                    ),
                    _ => throw new NotSupportedException("Invalid query operator")
                };
            }

            return query.Not ? Expression.Not(combined) : combined;
        }

        var properties = query.Property?.Split('.');
        var member = param;

        if (properties != null)
        {
            foreach (var property in properties)
            {
                if (property.EndsWith("]"))
                {
                    var match = Regex.Match(property, @"(.+)\[(\d+)\]");
                    if (match.Success)
                    {
                        var propName = match.Groups[1].Value;
                        var index = int.Parse(match.Groups[2].Value);
                        member = Expression.Property(member, propName);

                        var elementAtMethod = typeof(Enumerable).GetMethods()
                            .First(m => m.Name == "ElementAtOrDefault" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(member.Type.GetGenericArguments()[0]);

                        member = Expression.Call(elementAtMethod, member, Expression.Constant(index));
                        continue;
                    }
                }
                if (typeof(IEnumerable).IsAssignableFrom(member.Type) && member.Type != typeof(string))
                {
                    var itemType = member.Type.GetGenericArguments()[0];
                    var itemParam = Expression.Parameter(itemType, "i");
                    var newQuery = new Query
                    {
                        Property = string.Join('.', properties.Skip(1)),
                        Filter = query.Filter,
                        Value = query.Value
                    };
                    var lambda = BuildExpression(newQuery, itemParam);

                    var anyMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == "Any" && m.GetParameters().Length == 2).MakeGenericMethod(itemType);
                    var anyExpression = Expression.Call(anyMethod, member, Expression.Lambda(lambda, itemParam));
                    return query.Not ? Expression.Not(anyExpression) : anyExpression;

                }
                member = Expression.Property(member, property);
            }
        }

        var constant = Expression.Constant(query.Value);
        Expression comparison = query.Filter switch
        {
            Filter.Equal => Expression.Equal(member, constant),
            Filter.NotEqual => Expression.NotEqual(member, constant),
            Filter.GreaterOrEqual => Expression.GreaterThanOrEqual(member, constant),
            Filter.LessOrEqual => Expression.LessThanOrEqual(member, constant),
            Filter.GreaterThan => Expression.GreaterThan(member, constant),
            Filter.LessThan => Expression.LessThan(member, constant),
            Filter.StartsWith => Expression.Call(member,
                typeof(string).GetMethod("StartsWith", new[] { typeof(string) }) ??
                throw new InvalidOperationException(), constant),
            Filter.EndsWith => Expression.Call(member,
                typeof(string).GetMethod("EndsWith", new[] { typeof(string) }) ?? throw new InvalidOperationException(),
                constant),
            Filter.DoesNotStartWith => Expression.Not(Expression.Call(member,
                typeof(string).GetMethod("StartsWith", new[] { typeof(string) }) ??
                throw new InvalidOperationException(), constant)),
            Filter.DoesNotEndWith => Expression.Not(Expression.Call(member,
                typeof(string).GetMethod("EndsWith", new[] { typeof(string) }) ?? throw new InvalidOperationException(),
                constant)),
            Filter.Contains => Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) }) ?? throw new InvalidOperationException(),
                constant),
            Filter.DoesNotContain => Expression.Not(Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) }) ?? throw new InvalidOperationException(),
                constant)),
            Filter.IsNull => Expression.Equal(member, Expression.Constant(null, member.Type)),
            Filter.NotNull => Expression.NotEqual(member, Expression.Constant(null, member.Type)),
            Filter.Matches => Expression.Call(
                typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) }) ??
                throw new InvalidOperationException(), member, constant),
            Filter.DoesNotMatch => Expression.Not(Expression.Call(
                typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) }) ??
                throw new InvalidOperationException(), member, constant)),
            Filter.In => Expression.Call(typeof(Enumerable), "Contains", new[] { member.Type }, constant, member),
            Filter.NotIn => Expression.Not(Expression.Call(typeof(Enumerable), "Contains", new[] { member.Type },
                constant, member)),
            Filter.All => BuildAllExpression(member, query, properties),
            Filter.Any => BuildAnyExpression(member, query, properties),
            _ => throw new NotSupportedException($"Invalid query filter: {query.Filter}")
        };

        return query.Not ? Expression.Not(comparison) : comparison;
    }
    /// <summary>
    /// Converts a <see cref="Query"/> object into a LINQ expression for the "Any" filter.
    /// </summary>
    private static Expression BuildAllExpression(Expression member, Query query, string[]? properties)
    {
        var itemType = member.Type.GetGenericArguments()[0];
        var itemParam = Expression.Parameter(itemType, "i");

        var newQuery = new Query
        {
            Property = properties is null ? null : string.Join('.', properties.Skip(1)),
            Filter = query.Filter,
            Value = query.Value
        };

        var lambda = BuildExpression(newQuery, itemParam);

        var allMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "All" && m.GetParameters().Length == 2).MakeGenericMethod(itemType);

        return Expression.Call(allMethod, member, Expression.Lambda(lambda, itemParam));
    }

    private static Expression BuildAnyExpression(Expression member, Query query, string[]? properties)
    {
        var itemType = member.Type.GetGenericArguments()[0];
        var itemParam = Expression.Parameter(itemType, "i");

        var newQuery = new Query
        {
            Property = properties is null ? null : string.Join('.', properties.Skip(1)),
            Filter = query.Filter,
            Value = query.Value
        };

        var lambda = BuildExpression(newQuery, itemParam);

        var anyMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2).MakeGenericMethod(itemType);

        return Expression.Call(anyMethod, member, Expression.Lambda(lambda, itemParam));
    }

}