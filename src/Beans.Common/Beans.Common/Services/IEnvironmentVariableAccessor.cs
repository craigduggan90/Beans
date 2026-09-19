namespace Beans.Common.Services;

/// <summary>Accessor service for environment variables.</summary>
public interface IEnvironmentVariableAccessor
{
    /// <summary>Get an environment variable from the current environment by name.</summary>
    /// <param name="variable">The name of the variable.</param>
    /// <returns>The variable value if a matching variable is found; otherwise null.</returns>
    string? Get(string variable);

    /// <summary>Set an environment variable in the current environment.</summary>
    /// <param name="variable">The name of the variable.</param>
    /// <param name="value">The value to assign to the variable.</param>
    void Set(string variable, string? value);
}