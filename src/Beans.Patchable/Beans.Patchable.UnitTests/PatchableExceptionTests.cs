namespace Beans.Patchable.UnitTests;

public static class PatchableExceptionTests
{
    public class ForIncorrectPropertyType
    {
        [Fact]
        public void ShouldCreateExceptionWithExpectedMessage()
        {
            var exception = PatchableException.ForIncorrectPropertyType(
                typeof(TestEntity),
                "Name",
                typeof(String),
                typeof(Int32));

            Assert.Equal(
                "Failed to set property 'Name' on type 'TestEntity': Incorrect value type (expected String, received Int32).",
                exception.Message);
        }
    }

    public class ForPropertyNotFoundInType
    {
        [Fact]
        public void ShouldCreateExceptionWithExpectedMessage()
        {
            var exception = PatchableException.ForPropertyNotFoundInType(
                typeof(TestEntity),
                "Name");

            Assert.Equal(
                "Failed to set property 'Name' on type 'TestEntity': Property not found.",
                exception.Message);
        }
    }

    public class ForNullAssignedToNonNullableProperty
    {
        [Fact]
        public void ShouldCreateExceptionWithExpectedMessage()
        {
            var exception = PatchableException.ForNullAssignedToNonNullableProperty(
                typeof(TestEntity),
                "Name");

            Assert.Equal(
                "Failed to set property 'Name' on type 'TestEntity': Cannot assign null to a non-nullable property.",
                exception.Message);
        }
    }

    private sealed class TestEntity;
}