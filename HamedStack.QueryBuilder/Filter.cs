namespace HamedStack.QueryBuilder;

/// <summary>
/// Represents various filter conditions for query expressions.
/// </summary>
public enum Filter
{
    /// <summary>
    /// Represents the "Equal" filter condition.
    /// </summary>
    Equal,

    /// <summary>
    /// Represents the "NotEqual" filter condition.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Represents the "GreaterOrEqual" filter condition.
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// Represents the "LessOrEqual" filter condition.
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// Represents the "GreaterThan" filter condition.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Represents the "LessThan" filter condition.
    /// </summary>
    LessThan,

    /// <summary>
    /// Represents the "StartsWith" filter condition.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Represents the "EndsWith" filter condition.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Represents the "DoesNotStartWith" filter condition.
    /// </summary>
    DoesNotStartWith,

    /// <summary>
    /// Represents the "DoesNotEndWith" filter condition.
    /// </summary>
    DoesNotEndWith,

    /// <summary>
    /// Represents the "Contains" filter condition.
    /// </summary>
    Contains,

    /// <summary>
    /// Represents the "DoesNotContain" filter condition.
    /// </summary>
    DoesNotContain,

    /// <summary>
    /// Represents the "IsNull" filter condition.
    /// </summary>
    IsNull,

    /// <summary>
    /// Represents the "NotNull" filter condition.
    /// </summary>
    NotNull,

    /// <summary>
    /// Represents the "Matches" filter condition.
    /// </summary>
    Matches,

    /// <summary>
    /// Represents the "DoesNotMatch" filter condition.
    /// </summary>
    DoesNotMatch,

    /// <summary>
    /// Represents the "In" filter condition.
    /// </summary>
    In,

    /// <summary>
    /// Represents the "NotIn" filter condition.
    /// </summary>
    NotIn,

    /// <summary>
    /// Represents the "All" filter condition.
    /// </summary>
    All,

    /// <summary>
    /// Represents the "Any" filter condition.
    /// </summary>
    Any
}