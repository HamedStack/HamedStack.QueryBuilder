// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo

namespace HamedStack.QueryBuilder;

/// <summary>
/// Represents a query expression.
/// </summary>
[Serializable]
public class Query
{
    /// <summary>
    /// Gets or sets the logical operator for this query.
    /// </summary>
    public Operator Operator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the query is negated.
    /// </summary>
    public bool Not { get; set; }

    /// <summary>
    /// Gets or sets the property to filter on.
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Gets or sets the filter condition for the query.
    /// </summary>
    public Filter Filter { get; set; }

    /// <summary>
    /// Gets or sets the value to compare against in the query.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets a list of subqueries associated with this query.
    /// </summary>
    public List<Query>? Queries { get; set; }
}