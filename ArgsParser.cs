namespace proj_graph;

public record struct NamedArg(string Name, string? Value);

public static class ArgsParser
{
    public static Dictionary<string, string?> ParseArgs(this string[] args, char separator)
        => args
            .Select(a => a.ParseArg(separator))
            .ToDictionary(n => n.Name, n => n.Value);

    private static NamedArg ParseArg(this string arg, char separator)
    {
        int i = arg.IndexOf(separator);
        return i != -1
            ? new(arg.Substring(0, i), arg.Substring(i + 1, arg.Length - i - 1))
            : new(arg, null);
    }

    public static string ValueOrElse(this Dictionary<string, string?> dict, string key, string defaultValue)
        => dict.TryGetValue(key, out string? value) ? value ?? defaultValue : defaultValue;
}
