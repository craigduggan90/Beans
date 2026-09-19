namespace Beans.Common.Services;

/// <inheritdoc/>
public class EnvironmentVariableAccessor : IEnvironmentVariableAccessor
{
    /// <inheritdoc />
    public string? Get(string variable)
        => Environment.GetEnvironmentVariable(variable);

    /// <inheritdoc />
    public void Set(string variable, string? value)
        => Environment.SetEnvironmentVariable(variable, value);
}