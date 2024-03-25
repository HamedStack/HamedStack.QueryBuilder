// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Linq.Expressions;
// ReSharper disable MemberCanBePrivate.Global

namespace HamedStack.QueryBuilder;

/// <summary>
/// Represents a query builder for constructing complex queries.
/// </summary>
public class QueryBuilder
{
    private readonly Query _currentQuery;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryBuilder"/> class with the specified logical operator.
    /// </summary>
    /// <param name="condition">The initial logical operator.</param>
    public QueryBuilder(Operator condition = Operator.And)
    {
        _currentQuery = new Query
        {
            Operator = condition,
            Queries = new List<Query>()
        };
    }

    /// <summary>
    /// Sets the logical operator for the current query to "And".
    /// </summary>
    /// <returns>The current <see cref="QueryBuilder"/> instance.</returns>
    public QueryBuilder And()
    {
        _currentQuery.Operator = Operator.And;
        return this;
    }

    /// <summary>
    /// Sets the logical operator for the current query to "Or".
    /// </summary>
    /// <returns>The current <see cref="QueryBuilder"/> instance.</returns>
    public QueryBuilder Or()
    {
        _currentQuery.Operator = Operator.Or;
        return this;
    }

    /// <summary>
    /// Sets the logical operator for the current query to "Xor".
    /// </summary>
    /// <returns>The current <see cref="QueryBuilder"/> instance.</returns>
    public QueryBuilder Xor()
    {
        _currentQuery.Operator = Operator.Xor;
        return this;
    }

    /// <summary>
    /// Adds a filter condition to the current query.
    /// </summary>
    /// <param name="property">The property to filter on.</param>
    /// <param name="filter">The filter condition.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The current <see cref="QueryBuilder"/> instance.</returns>
    public QueryBuilder Add(string property, Filter filter, object? value = null)
    {
        var query = new Query
        {
            Property = property,
            Filter = filter,
            Value = value
        };
        _currentQuery.Queries?.Add(query);
        return this;
    }

    /// <summary>
    /// Adds a filter condition to the current query using a property expression.
    /// </summary>
    /// <typeparam name="T">The type of entity to filter.</typeparam>
    /// <param name="propertyExpression">An expression representing the property to filter on.</param>
    /// <param name="filter">The filter condition.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The current <see cref="QueryBuilder"/> instance.</returns>
    public QueryBuilder Add<T>(Expression<Func<T, object>> propertyExpression, Filter filter, object? value = null)
    {
        var propertyName = GetPropertyName(propertyExpression);

        var query = new Query
        {
            Property = propertyName,
            Filter = filter,
            Value = value
        };
        _currentQuery.Queries?.Add(query);
        return this;
    }

    /// <summary>
    /// Gets the name of the property represented by the provided expression.
    /// </summary>
    /// <typeparam name="T">The type of entity containing the property.</typeparam>
    /// <param name="propertyExpression">An expression representing the property.</param>
    /// <returns>The name of the property.</returns>
    private static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
    {
        return propertyExpression.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression { Operand: MemberExpression innerMemberExpression } => innerMemberExpression.Member.Name,
            _ => throw new ArgumentException("Invalid property expression")
        };
    }

    /// <summary>
    /// Groups multiple query conditions with a specified logical operator.
    /// </summary>
    /// <param name="groupAction">An action to define the grouped queries.</param>
    /// <param name="condition">The logical operator for the grouped queries.</param>
    /// <returns>The current <see cref="QueryBuilder"/> instance.</returns>
    public QueryBuilder Group(Action<QueryBuilder> groupAction, Operator condition = Operator.And)
    {
        var group = new QueryBuilder(condition);
        groupAction(group);
        _currentQuery.Queries?.Add(group.Build());
        return this;
    }

    /// <summary>
    /// Builds and returns the final query.
    /// </summary>
    /// <returns>The constructed <see cref="Query"/>.</returns>
    public Query Build()
    {
        return _currentQuery;
    }

}