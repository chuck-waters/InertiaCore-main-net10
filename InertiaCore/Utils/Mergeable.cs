namespace InertiaCore.Utils;

public interface Mergeable
{
    public bool merge { get; set; }
    public bool deepMerge { get; set; }
    public string[]? matchOn { get; set; }

    public Mergeable Merge()
    {
        merge = true;

        return this;
    }

    public Mergeable DeepMerge()
    {
        deepMerge = true;

        merge = true;

        return this;
    }

    public Mergeable MatchesOn(params string[] keys)
    {
        matchOn = keys;
        return this;
    }

    public bool ShouldMerge() => merge;
    public bool ShouldDeepMerge() => deepMerge;
    public string[]? GetMatchOn() => matchOn;
}
