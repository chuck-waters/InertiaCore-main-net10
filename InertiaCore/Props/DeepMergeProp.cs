using InertiaCore.Utils;

namespace InertiaCore.Props;

public class DeepMergeProp : InvokableProp, Mergeable
{
    public bool merge { get; set; } = true;
    public string[]? mergeStrategies { get; set; }
    public bool deepMerge { get; set; } = true;

    public DeepMergeProp(object? value) : base(value)
    {
        merge = true;
        deepMerge = true;
    }

    public DeepMergeProp(object? value, string[]? strategies) : base(value)
    {
        merge = true;
        deepMerge = true;
        mergeStrategies = strategies;
    }

    public DeepMergeProp(object? value, string strategy) : base(value)
    {
        merge = true;
        deepMerge = true;
        mergeStrategies = new[] { strategy };
    }

    internal DeepMergeProp(Func<object?> value) : base(value)
    {
        merge = true;
        deepMerge = true;
    }

    internal DeepMergeProp(Func<object?> value, string[]? strategies) : base(value)
    {
        merge = true;
        deepMerge = true;
        mergeStrategies = strategies;
    }

    internal DeepMergeProp(Func<object?> value, string strategy) : base(value)
    {
        merge = true;
        deepMerge = true;
        mergeStrategies = new[] { strategy };
    }

    internal DeepMergeProp(Func<Task<object?>> value) : base(value)
    {
        merge = true;
        deepMerge = true;
    }

    internal DeepMergeProp(Func<Task<object?>> value, string[]? strategies) : base(value)
    {
        merge = true;
        deepMerge = true;
        mergeStrategies = strategies;
    }

    internal DeepMergeProp(Func<Task<object?>> value, string strategy) : base(value)
    {
        merge = true;
        deepMerge = true;
        mergeStrategies = new[] { strategy };
    }

    public bool ShouldDeepMerge() => deepMerge;
}