using System.Reflection;

namespace Beans.Requestable.UnitTests.TestSupport;

/// <summary>An assembly that reports exactly the types it is given, so a scan can be pointed at chosen types.</summary>
internal sealed class FakeAssembly(params Type[] types) : Assembly
{
    public override IEnumerable<TypeInfo> DefinedTypes => types.Select(type => type.GetTypeInfo());
}