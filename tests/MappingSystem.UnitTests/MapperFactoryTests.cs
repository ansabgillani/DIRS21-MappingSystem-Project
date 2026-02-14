using System.Linq.Expressions;
using FluentAssertions;
using MappingSystem.Core;
using MappingSystem.Core.Exceptions;
using MappingSystem.Implementation;
using MappingSystem.Implementation.Conventions;
using Microsoft.Extensions.Logging.Abstractions;

namespace MappingSystem.UnitTests;

public class MapperFactoryTests
{
    [Fact]
    public void CreateMapper_WithMatchingConvention_ReturnsCompiledDelegate()
    {
        var conventions = new IMappingConvention[] { new PropertyConvention() };
        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);

        var mapper = factory.CreateMapper<SimpleSource, SimpleTarget>();
        var result = mapper(new SimpleSource { Value = 42 });

        mapper.Should().NotBeNull();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void CreateMapper_NoMatchingConvention_ThrowsMappingNotFoundException()
    {
        var conventions = Array.Empty<IMappingConvention>();
        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);

        var act = () => factory.CreateMapper<SimpleSource, NoConstructor>();

        act.Should().Throw<MappingNotFoundException>()
            .WithMessage("*No mapping found*");
    }

    [Fact]
    public void CreateMapper_MultipleConventions_UsesFirstMatch()
    {
        var firstConvention = new AlwaysMatchConvention("First");
        var secondConvention = new AlwaysMatchConvention("Second");
        var conventions = new IMappingConvention[] { firstConvention, secondConvention };
        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);

        var mapper = factory.CreateMapper<SimpleSource, SimpleTarget>();
        var result = mapper(new SimpleSource());

        result.Value.Should().Be(1);
    }

    [Fact]
    public void Constructor_NullConventions_ThrowsArgumentNullException()
    {
        var act = () => new MapperFactory(null!, NullLogger<MapperFactory>.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateMapper_RuntimeTypes_ReturnsCompiledMapperObject()
    {
        var conventions = new IMappingConvention[] { new PropertyConvention() };
        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);

        var mapper = factory.CreateMapper(typeof(SimpleSource), typeof(SimpleTarget));

        mapper.Should().BeOfType<CompiledMapper<SimpleSource, SimpleTarget>>();
        var result = ((CompiledMapper<SimpleSource, SimpleTarget>)mapper).Map(new SimpleSource { Value = 7 });
        result.Value.Should().Be(7);
    }

    [Fact]
    public void CreateMapper_RuntimeTypes_SecondCall_UsesMappingCache()
    {
        var convention = new CountingConvention();
        var factory = new MapperFactory(new[] { convention }, NullLogger<MapperFactory>.Instance, new MappingCache());

        _ = factory.CreateMapper(typeof(SimpleSource), typeof(SimpleTarget));
        _ = factory.CreateMapper(typeof(SimpleSource), typeof(SimpleTarget));

        convention.CanMapCalls.Should().Be(1);
        convention.BuildExpressionCalls.Should().Be(1);
    }

    private class SimpleSource
    {
        public int Value { get; set; }
    }

    private class SimpleTarget
    {
        public int Value { get; set; }
    }

    private class NoConstructor
    {
        public NoConstructor(int required)
        {
            _ = required;
        }
    }

    private class AlwaysMatchConvention : IMappingConvention
    {
        private readonly string _name;

        public AlwaysMatchConvention(string name)
        {
            _name = name;
        }

        public bool CanMap(Type sourceType, Type targetType)
        {
            _ = sourceType;
            _ = targetType;
            return true;
        }

        public LambdaExpression BuildExpression(Type sourceType, Type targetType)
        {
            var param = Expression.Parameter(sourceType, "source");
            var newTarget = Expression.New(targetType);
            var valueProperty = targetType.GetProperty("Value")!;
            var setValue = Expression.Bind(valueProperty, Expression.Constant(_name == "First" ? 1 : 2));
            var memberInit = Expression.MemberInit(newTarget, setValue);
            return Expression.Lambda(memberInit, param);
        }
    }

    private class CountingConvention : IMappingConvention
    {
        public int CanMapCalls { get; private set; }

        public int BuildExpressionCalls { get; private set; }

        public bool CanMap(Type sourceType, Type targetType)
        {
            _ = sourceType;
            _ = targetType;
            CanMapCalls++;
            return true;
        }

        public LambdaExpression BuildExpression(Type sourceType, Type targetType)
        {
            BuildExpressionCalls++;
            var param = Expression.Parameter(sourceType, "source");
            var sourceValue = sourceType.GetProperty(nameof(SimpleSource.Value))!;
            var targetValue = targetType.GetProperty(nameof(SimpleTarget.Value))!;
            var newTarget = Expression.New(targetType);
            var bind = Expression.Bind(targetValue, Expression.Property(param, sourceValue));
            var memberInit = Expression.MemberInit(newTarget, bind);
            return Expression.Lambda(memberInit, param);
        }
    }
}
