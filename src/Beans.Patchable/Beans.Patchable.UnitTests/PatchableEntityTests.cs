namespace Beans.Patchable.UnitTests;

public static class PatchableEntityTests
{
    public class UpdateProperty
    {
        [Fact]
        public void ShouldReturnFalseAndLeavePropertyUnchanged_WhenValueIsUnset()
        {
            var entity = new TestEntity();

            var result = entity.UpdateProperty("Name", Optional<string>.Unset);

            Assert.False(result);
            Assert.Equal("Alice", entity.Name);
            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldUpdatePropertyAndMarkDirty_WhenValueIsDifferent()
        {
            var entity = new TestEntity();

            var result = entity.UpdateProperty("Name", Optional<string>.Of("Bob"));

            Assert.True(result);
            Assert.Equal("Bob", entity.Name);
            Assert.True(entity.IsDirty);
        }

        [Fact]
        public void ShouldReturnFalseAndLeaveDirtyStateUnchanged_WhenValueIsTheSame()
        {
            var entity = new TestEntity();

            var result = entity.UpdateProperty("Name", Optional<string>.Of("Alice"));

            Assert.False(result);
            Assert.Equal("Alice", entity.Name);
            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldSetNullableReferencePropertyToNullAndMarkDirty_WhenNullIsProvided()
        {
            var entity = new TestEntity();

            var result = entity.UpdateProperty("Description", Optional<string?>.Of(null));

            Assert.True(result);
            Assert.Null(entity.Description);
            Assert.True(entity.IsDirty);
        }

        [Fact]
        public void ShouldReturnFalseAndLeaveDirtyStateUnchanged_WhenNullableReferencePropertyIsAlreadyNull()
        {
            var entity = new TestEntity
            {
                Description = null
            };

            var result = entity.UpdateProperty("Description", Optional<string?>.Of(null));

            Assert.False(result);
            Assert.Null(entity.Description);
            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldUpdateNullableValueProperty_WhenValueIsProvided()
        {
            var entity = new TestEntity();

            var result = entity.UpdateProperty("Score", Optional<int>.Of(42));

            Assert.True(result);
            Assert.Equal(42, entity.Score);
            Assert.True(entity.IsDirty);
        }

        [Fact]
        public void ShouldSetNullableValuePropertyToNullAndMarkDirty_WhenNullIsProvided()
        {
            var entity = new TestEntity
            {
                Score = 42
            };

            var result = entity.UpdateProperty("Score", Optional<int?>.Of(null));

            Assert.True(result);
            Assert.Null(entity.Score);
            Assert.True(entity.IsDirty);
        }

        [Fact]
        public void ShouldReturnFalseAndLeavePropertyUnchanged_WhenNullableValuePropertyHasSameValue()
        {
            var entity = new TestEntity
            {
                Score = 42
            };

            var result = entity.UpdateProperty("Score", Optional<int>.Of(42));

            Assert.False(result);
            Assert.Equal(42, entity.Score);
            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldThrow_WhenNullIsAssignedToNonNullableProperty()
        {
            var entity = new TestEntity();

            var exception = Assert.Throws<PatchableException>(
                () => entity.UpdateProperty("Name", Optional<string?>.Of(null)));

            Assert.Equal(
                "Failed to set property 'Name' on type 'TestEntity': Cannot assign null to a non-nullable property.",
                exception.Message);

            Assert.Equal("Alice", entity.Name);
            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldThrow_WhenValueTypeIsIncompatibleWithPropertyType()
        {
            var entity = new TestEntity();

            var exception = Assert.Throws<PatchableException>(
                () => entity.UpdateProperty("Name", Optional<int>.Of(42)));

            Assert.Equal(
                "Failed to set property 'Name' on type 'TestEntity': Incorrect value type (expected String, received Int32).",
                exception.Message);

            Assert.Equal("Alice", entity.Name);
            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldUpdateNullableValueProperty_WhenNonNullableValueIsProvided()
        {
            var entity = new TestEntity();

            var result = entity.UpdateProperty("Score", Optional<int>.Of(42));

            Assert.True(result);
            Assert.Equal(42, entity.Score);
            Assert.True(entity.IsDirty);
        }

        [Fact]
        public void ShouldThrow_WhenPropertyDoesNotExist()
        {
            var entity = new TestEntity();

            var exception = Assert.Throws<PatchableException>(
                () => entity.UpdateProperty("Missing", Optional<string>.Of("value")));

            Assert.Equal(
                "Failed to set property 'Missing' on type 'TestEntity': Property not found.",
                exception.Message);

            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldThrow_WhenPropertyIsReadOnly()
        {
            var entity = new TestEntity();

            var exception = Assert.Throws<PatchableException>(
                () => entity.UpdateProperty("ReadOnly", Optional<string>.Of("value")));

            Assert.Equal(
                "Failed to set property 'ReadOnly' on type 'TestEntity': Property not found.",
                exception.Message);

            Assert.Equal("Read only", entity.ReadOnly);
            Assert.False(entity.IsDirty);
        }
    }

    public class IsDirty
    {
        [Fact]
        public void ShouldBeFalse_WhenEntityIsCreated()
        {
            var entity = new TestEntity();

            Assert.False(entity.IsDirty);
        }

        [Fact]
        public void ShouldRemainTrue_WhenASubsequentUpdateIsANoOp()
        {
            var entity = new TestEntity();
            entity.UpdateProperty("Name", Optional<string>.Of("Bob"));

            entity.UpdateProperty("Name", Optional<string>.Of("Bob"));

            Assert.True(entity.IsDirty);
        }
    }

    public class SetDirty
    {
        [Fact]
        public void ShouldMarkEntityDirty()
        {
            var entity = new TestEntity();

            entity.SetDirty();

            Assert.True(entity.IsDirty);
        }

        [Fact]
        public void ShouldNotChangeAnyProperty()
        {
            var entity = new TestEntity();

            entity.SetDirty();

            Assert.Equal("Alice", entity.Name);
            Assert.Equal("Description", entity.Description);
            Assert.Equal(30, entity.Age);
            Assert.Null(entity.Score);
        }

        [Fact]
        public void ShouldRemainDirty_WhenCalledAgain()
        {
            var entity = new TestEntity();
            entity.SetDirty();

            entity.SetDirty();

            Assert.True(entity.IsDirty);
        }
    }

    private sealed class TestEntity : PatchableEntity
    {
        public string Name { get; set; } = "Alice";

        public string? Description { get; set; } = "Description";

        public int Age { get; set; } = 30;

        public int? Score { get; set; }

        public string ReadOnly => "Read only";
    }
}