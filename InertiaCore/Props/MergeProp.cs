using InertiaCore.Props;

namespace InertiaCore.Utils;

public class MergeProp : InvokableProp, Mergeable
{
    public bool merge { get; set; } = true;
    public string[]? mergeStrategies { get; set; }

    public MergeProp(object? value) : base(value)
    {
        merge = true;
    }

    public MergeProp(object? value, string[]? strategies) : base(value)
    {
        merge = true;
        mergeStrategies = strategies;
    }

    public MergeProp(object? value, string strategy) : base(value)
    {
        merge = true;
        mergeStrategies = new[] { strategy };
    }

    internal MergeProp(Func<object?> value) : base(value)
    {
        merge = true;
    }

    internal MergeProp(Func<object?> value, string[]? strategies) : base(value)
    {
        merge = true;
        mergeStrategies = strategies;
    }

    internal MergeProp(Func<object?> value, string strategy) : base(value)
    {
        merge = true;
        mergeStrategies = new[] { strategy };
    }

    internal MergeProp(Func<Task<object?>> value) : base(value)
    {
        merge = true;
    }

    internal MergeProp(Func<Task<object?>> value, string[]? strategies) : base(value)
    {
        merge = true;
        mergeStrategies = strategies;
    }

    internal MergeProp(Func<Task<object?>> value, string strategy) : base(value)
    {
        merge = true;
        mergeStrategies = new[] { strategy };
    }
}


