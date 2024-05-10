using MermaidDotNet;
using MermaidDotNet.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text;

namespace proj_graph;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Dictionary<string, string?> argsNamed = args.ParseArgs(':');
        argsNamed.TryGetValue("name-exclude", out string? nameExclude);

        if (!argsNamed.TryGetValue("path-sln", out string? pathSln)
            || pathSln is null
            || !Path.Exists(pathSln))
        {
            Console.Error.WriteLine("Must specify solution path like 'proj-graph path-sln:some.awsome.sln' ");
            return;
        }

        if (!argsNamed.TryGetValue("path-diagram", out string? pathDiagram)
            || pathDiagram is null)
        {
            Console.Error.WriteLine("Must specify diagram path 'proj-graph path-diagram:diagram.html' ");
            return;
        }

        MSBuildWorkspace workspace = MSBuildWorkspace.Create();
        workspace.SkipUnrecognizedProjects = true;
        workspace.WorkspaceFailed += OnWorkspaceFailed;

        Solution solution = await workspace
            .OpenSolutionAsync(pathSln, new ConsoleOutProgress());

        string direction = "LR";
        List<Node> nodes = [];
        List<Link> links = [];

        foreach (Project project in solution.Projects)
        {
            if (nameExclude is not null && project.AssemblyName.Contains(nameExclude))
            {
                continue;
            }

            if (!nodes.Exists(n => n.Name == project.AssemblyName))
            {
                nodes.Add(new Node(project.AssemblyName, project.AssemblyName, Node.ShapeType.Rounded));
            }

            foreach (Project reference in project
                .ProjectReferences
                .Select(r => solution
                    .Projects
                    .First(p => p.Id == r.ProjectId)))
            {
                if (nameExclude is not null && reference.AssemblyName.Contains(nameExclude))
                {
                    continue;
                }

                links.Add(new Link(project.AssemblyName, reference.AssemblyName));
            }
        }

        Flowchart flowchart = new(
            direction,
            nodes,
            links);

        string mermaidChart = flowchart.CalculateFlowchart();

        var sb = new StringBuilder();
        sb.Append("<h2>");
        sb.Append(solution.FilePath);
        sb.Append("""
        </h2>
        <body>
            Project references:
            <pre class="mermaid">
        """);
        sb.Append(mermaidChart);
        sb.Append("""
            </pre>
            <script type="module">
                import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
                mermaid.initialize({ "startOnLoad": true, "maxTextSize": 10000000, "maxEdges": 5000 });
            </script>
        </body>
        """);

        File.WriteAllText(pathDiagram, sb.ToString());
    }

    private static void OnWorkspaceFailed(object? sender, WorkspaceDiagnosticEventArgs e)
    {
        if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
        {
            Console.Error.WriteLine(e.Diagnostic.Message);
        }
    }
}