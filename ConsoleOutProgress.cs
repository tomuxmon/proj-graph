using Microsoft.CodeAnalysis.MSBuild;

namespace proj_graph;

public class ConsoleOutProgress : IProgress<ProjectLoadProgress>
{
    public void Report(ProjectLoadProgress value)
    {
        Console.Out.WriteLine($"{value.Operation} {value.FilePath}");
    }
}