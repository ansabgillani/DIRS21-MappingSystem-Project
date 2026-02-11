namespace MappingSystem.Implementation;

public class CompiledMapper<TSource, TTarget> : ICompiledMapper
{
    private readonly Func<TSource, TTarget> _compiledDelegate;

    public CompiledMapper(Func<TSource, TTarget> compiledDelegate)
    {
        _compiledDelegate = compiledDelegate ?? throw new ArgumentNullException(nameof(compiledDelegate));
    }

    public CompiledMapper(Delegate compiledDelegate)
        : this((Func<TSource, TTarget>)compiledDelegate)
    {
    }

    public Type SourceType => typeof(TSource);

    public Type TargetType => typeof(TTarget);

    public TTarget Map(TSource source)
    {
        return _compiledDelegate(source);
    }

    public object Map(object source)
    {
        return Map((TSource)source)!;
    }
}
