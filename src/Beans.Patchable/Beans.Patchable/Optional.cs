namespace Beans.Patchable;

/// <summary>
/// Represents an optional value that can distinguish between a value that was not supplied and a value that
/// was explicitly supplied, including <see langword="null"/>.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
/// <remarks>
/// <para>
/// Unlike a nullable value alone, an <see cref="Optional{T}"/> has three meaningful states:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="Unset"/> — no value was supplied.</description></item>
/// <item><description><see cref="Of(T)"/> containing <see langword="null"/> — null was explicitly supplied.</description></item>
/// <item><description><see cref="Of(T)"/> containing a value — a value was explicitly supplied.</description></item>
/// </list>
/// <para>
/// This distinction is useful when applying partial updates, where an omitted property and a property explicitly
/// assigned <see langword="null"/> must have different meanings.
/// </para>
/// </remarks>
public readonly struct Optional<T>
{
    /// <summary>
    /// The value supplied to the optional, when one was supplied.
    /// </summary>
    private readonly T? _value;

    /// <summary>
    /// Gets a value indicating whether a value was explicitly supplied.
    /// </summary>
    /// <remarks>
    /// This property indicates whether the optional is set, not whether its value is non-null.
    /// Consequently, an optional can be set while its <see cref="Value"/> is <see langword="null"/>.
    /// </remarks>
    public bool IsSet { get; }

    /// <summary>
    /// Gets the supplied value, or <see langword="default"/> when no value was supplied.
    /// </summary>
    /// <returns>
    /// The supplied value, which may be <see langword="null"/> when <typeparamref name="T"/> permits it; or
    /// <see langword="default"/> when the optional is <see cref="Unset"/>.
    /// </returns>
    /// <remarks>
    /// Because an unset optional and one explicitly set to <see langword="null"/> both surface as
    /// <see langword="default"/>/<see langword="null"/> here, use <see cref="IsSet"/> or
    /// <see cref="TryGetValue"/> when that distinction matters.
    /// </remarks>
    public T? Value => IsSet ? _value : default;

    /// <summary>
    /// Gets the declared type of the optional value.
    /// </summary>
    /// <remarks>
    /// This returns <typeparamref name="T"/> rather than the runtime type of <see cref="Value"/>.
    /// This is important when the value is <see langword="null"/>, because a null value has no runtime type.
    /// </remarks>
    public Type ValueType => typeof(T);

    /// <summary>
    /// Attempts to retrieve the supplied value.
    /// </summary>
    /// <param name="value">
    /// When the optional is set, receives the supplied value; otherwise, receives <see langword="default"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a value was explicitly supplied; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method allows callers to distinguish an unset optional from an optional that was explicitly set to
    /// <see langword="null"/> without having to access <see cref="Value"/> and handle its exception.
    /// </remarks>
    public bool TryGetValue(out T? value)
    {
        value = IsSet ? _value : default;
        return IsSet;
    }

    /// <summary>
    /// Creates an optional with the specified value and set state.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="isSet">Whether the optional represents an explicitly supplied value.</param>
    private Optional(T? value, bool isSet) => (_value, IsSet) = (value, isSet);

    /// <summary>
    /// Gets an optional representing a value that was not supplied.
    /// </summary>
    /// <remarks>
    /// <see cref="Unset"/> is distinct from <see cref="Of(T)"/> with a <see langword="null"/> value.
    /// The former means "do not update this property", while the latter means "explicitly set this property to null".
    /// </remarks>
    public static Optional<T> Unset => new(default, false);

    /// <summary>
    /// Creates an optional representing an explicitly supplied value.
    /// </summary>
    /// <param name="value">The value to supply, which may be <see langword="null"/>.</param>
    /// <returns>An optional containing the supplied value and marked as set.</returns>
    public static Optional<T> Of(T? value) => new(value, true);
}