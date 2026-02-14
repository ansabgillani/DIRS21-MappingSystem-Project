using FluentAssertions;
using MappingSystem.Core.Exceptions;
using MappingSystem.Implementation;

namespace MappingSystem.UnitTests;

public class ExpressionBuilderTests
{
    public class Source
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class Target
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class PartialTarget
    {
        public int Id { get; set; }

        public double Value { get; set; }
    }

    public class NoConstructorTarget
    {
        public NoConstructorTarget(int required)
        {
            _ = required;
        }

        public int Id { get; set; }
    }

    [Fact]
    public void BuildExpression_AllPropertiesMatch_GeneratesCorrectAssignments()
    {
        var expression = ExpressionBuilder.BuildPropertyMappingExpression(typeof(Source), typeof(Target));
        var compiled = (Func<Source, Target>)expression.Compile();

        var source = new Source { Id = 123, Name = "Test", CreatedAt = DateTime.UtcNow };
        var result = compiled(source);

        result.Id.Should().Be(123);
        result.Name.Should().Be("Test");
        result.CreatedAt.Should().Be(source.CreatedAt);
    }

    [Fact]
    public void BuildExpression_PartialMatch_SkipsMismatchedProperties()
    {
        var expression = ExpressionBuilder.BuildPropertyMappingExpression(typeof(Source), typeof(PartialTarget));
        var compiled = (Func<Source, PartialTarget>)expression.Compile();

        var source = new Source { Id = 456, Name = "Test" };
        var result = compiled(source);

        result.Id.Should().Be(456);
        result.Value.Should().Be(0d);
    }

    [Fact]
    public void BuildExpression_NoDefaultConstructor_ThrowsCompilationException()
    {
        var act = () => ExpressionBuilder.BuildPropertyMappingExpression(typeof(Source), typeof(NoConstructorTarget));

        act.Should().Throw<MappingCompilationException>()
            .WithMessage("*lacks parameterless constructor*");
    }

    [Fact]
    public void BuildExpression_CaseInsensitivePropertyMatch_Works()
    {
        var sourceType = typeof(CaseInsensitiveSource);
        var targetType = typeof(CaseInsensitiveTarget);

        var expression = ExpressionBuilder.BuildPropertyMappingExpression(sourceType, targetType);
        var compiled = expression.Compile();

        var source = new CaseInsensitiveSource { name = "Alice" };
        var result = (CaseInsensitiveTarget)compiled.DynamicInvoke(source)!;

        result.NAME.Should().Be("Alice");
    }

    private class CaseInsensitiveSource
    {
        public string name { get; set; } = string.Empty;
    }

    private class CaseInsensitiveTarget
    {
        public string NAME { get; set; } = string.Empty;
    }
}
