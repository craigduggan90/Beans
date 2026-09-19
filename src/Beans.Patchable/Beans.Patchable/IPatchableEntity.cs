namespace Beans.Patchable;

/// <summary>
/// An entity that can be patched using <see cref="Optional{T}"/> values, and that tracks whether it has been changed.
/// </summary>
public interface IPatchableEntity
{
    /// <summary>Indicates whether this object has been marked as changed.</summary>
    bool IsDirty { get; }

    /// <summary>Updates a property when <see cref="Optional{T}"/> is set.</summary>
    /// <typeparam name="T">The type of the supplied property value.</typeparam>
    /// <param name="propertyName">The name of the property to update.</param>
    /// <param name="optionalValue">
    /// The optional value to apply. An unset value leaves the property unchanged; a set value, including
    /// <see langword="null"/>, is applied to the property.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the property was changed; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="PatchableException">
    /// Thrown when the property does not exist, the supplied value is incompatible with the property type,
    /// or <see langword="null"/> is assigned to a non-nullable property.
    /// </exception>
    bool UpdateProperty<T>(string propertyName, Optional<T> optionalValue);

    /// <summary>
    /// Marks the current object as dirty. Valid to call independently of <see cref="UpdateProperty{T}"/> — for
    /// example, after mutating a collection or other reference property that <see cref="UpdateProperty{T}"/>
    /// wouldn't otherwise detect as changed.
    /// </summary>
    /// <remarks>
    /// When extending this method, be sure to call <c>base.SetDirty</c> to ensure that <see cref="IsDirty"/> is set to
    /// <see langword="true"/>.  <see cref="IsDirty"/> is intentionally read-only, so cannot be directly mutated by
    /// inheritors.
    /// </remarks>
    void SetDirty();
}