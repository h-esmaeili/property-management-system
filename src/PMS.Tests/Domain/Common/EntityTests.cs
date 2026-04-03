using FluentAssertions;
using PMS.Domain.Common;

namespace PMS.Tests.Domain.Common;

public sealed class EntityTests
{
    private sealed class SampleEntity : Entity
    {
        public static SampleEntity WithId(Guid id) => new() { Id = id };
    }

    [Fact]
    public void GetHashCode_matches_Id_GetHashCode()
    {
        var id = Guid.NewGuid();
        var entity = SampleEntity.WithId(id);

        entity.GetHashCode().Should().Be(id.GetHashCode());
    }

    [Fact]
    public void HashSet_treats_same_id_as_single_entry()
    {
        var id = Guid.NewGuid();
        var a = SampleEntity.WithId(id);
        var b = SampleEntity.WithId(id);

        var set = new HashSet<Entity> { a, b };

        set.Should().ContainSingle();
        set.Should().Contain(a).And.Contain(b);
    }

    [Fact]
    public void ObjectEquals_static_uses_entity_equals()
    {
        var id = Guid.NewGuid();
        var a = SampleEntity.WithId(id);
        var b = SampleEntity.WithId(id);

        object.Equals(a, b).Should().BeTrue();
        object.Equals(a, (object?)null).Should().BeFalse();
        object.Equals((object?)null, b).Should().BeFalse();
    }

    [Fact]
    public void Equals_via_entity_reference_same_id()
    {
        var id = Guid.NewGuid();
        Entity a = SampleEntity.WithId(id);
        Entity b = SampleEntity.WithId(id);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_with_null_returns_false()
    {
        var entity = SampleEntity.WithId(Guid.NewGuid());

        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_with_non_entity_returns_false()
    {
        var entity = SampleEntity.WithId(Guid.NewGuid());

        entity.Equals("not an entity").Should().BeFalse();
        entity.Equals(new object()).Should().BeFalse();
    }

    [Fact]
    public void Equals_same_reference_returns_true()
    {
        var entity = SampleEntity.WithId(Guid.NewGuid());

        entity.Equals(entity).Should().BeTrue();
    }

    [Fact]
    public void Equals_different_instances_same_id_returns_true()
    {
        var id = Guid.NewGuid();
        var a = SampleEntity.WithId(id);
        var b = SampleEntity.WithId(id);

        a.Equals(b).Should().BeTrue();
        b.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Equals_different_ids_returns_false()
    {
        var a = SampleEntity.WithId(Guid.NewGuid());
        var b = SampleEntity.WithId(Guid.NewGuid());

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_empty_guid_same_for_two_instances_returns_true()
    {
        var a = SampleEntity.WithId(Guid.Empty);
        var b = SampleEntity.WithId(Guid.Empty);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_matches_for_same_id()
    {
        var id = Guid.NewGuid();
        var a = SampleEntity.WithId(id);
        var b = SampleEntity.WithId(id);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_differs_when_ids_differ()
    {
        var a = SampleEntity.WithId(Guid.NewGuid());
        var b = SampleEntity.WithId(Guid.NewGuid());

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }
}
