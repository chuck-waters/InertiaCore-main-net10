namespace InertiaCore.Utils;

public interface Mergeable
{
    public bool merge { get; set; }
    public string[]? matchOn { get; set; }

    public Mergeable Merge()
    {
        merge = true;

        return this;
    }

    public Mergeable MatchesOn(params string[] keys)
    {
        matchOn = keys;
        return this;
    }

    public bool ShouldMerge() => merge;
    public string[]? GetMatchOn() => matchOn;
}
