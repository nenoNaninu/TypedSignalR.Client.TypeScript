using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Tapper;

namespace TypedSignalR.Client.TypeScript;

public class App : ConsoleAppBase
{
    private readonly ILogger<App> _logger;

    public App(ILogger<App> logger)
    {
        _logger = logger;
    }

    [RootCommand]
    public async Task Transpile(
        [Option("p", "Path to the project file (XXX.csproj)")] string project,
        [Option("o", "Output directory")] string output,
        [Option("eol", "lf / crlf / cr")] string newLine = "lf",
        [Option("asm", "Flag whether to extend the transpile target to the referenced assembly.")] bool assemblies = false,
        [Option("s", "Json / MessagePack : The output type will be suitable for the selected serializer.")] string serializer = "json",
        [Option("n", "PascalCase / camelCase / none (The name in C# is used as it is.)")] string namingStyle = "camelCase",
        [Option("en", "PascalCase / camelCase / none (The name in C# is used as it is.)")] string enumNamingStyle = "PascalCase")
    {
        newLine = newLine switch
        {
            "crlf" => "\r\n",
            "lf" => "\n",
            "cr" => "\r",
            _ => throw new ArgumentException($"{newLine} is not supported.")
        };

        _logger.Log(LogLevel.Information, "Start loading the csproj of {path}.", Path.GetFullPath(project));

        output = Path.GetFullPath(output);

        if (!Enum.TryParse<SerializerOption>(serializer, true, out var serializerOption))
        {
            _logger.Log(LogLevel.Information, "Only json or messagepack can be selected for serializer. {type} is not supported.", serializer);
            return;
        }

        if (!Enum.TryParse<NamingStyle>(namingStyle, true, out var style))
        {
            _logger.Log(LogLevel.Information, "The naming style can only be selected from None, CamelCase, or PascalCase. {style} is not supported.", namingStyle);
            return;
        }

        if (!Enum.TryParse<EnumNamingStyle>(enumNamingStyle, true, out var enumStyle))
        {
            _logger.Log(LogLevel.Error, "The enum naming style can only be selected from None, CamelCase, or PascalCase. {style} is not supported.", enumNamingStyle);
        }

        try
        {
            var compilation = await this.CreateCompilationAsync(project);

            await TranspileCore(compilation, output, newLine, 4, assemblies, serializerOption, style, enumStyle);

            _logger.Log(LogLevel.Information, "======== Transpilation is completed. ========");
            _logger.Log(LogLevel.Information, "Please check the output folder: {output}", output);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Information, "======== Exception ========");
            _logger.Log(LogLevel.Error, "{ex}", ex);
        }
    }

    private async Task<Compilation> CreateCompilationAsync(string projectPath)
    {
        var logger = new ConsoleLogger(LoggerVerbosity.Quiet);
        using var workspace = MSBuildWorkspace.Create();

        var msBuildProject = await workspace.OpenProjectAsync(projectPath, logger, null, this.Context.CancellationToken);

        _logger.Log(LogLevel.Information, "Create Compilation...");
        var compilation = await msBuildProject.GetCompilationAsync(this.Context.CancellationToken);

        if (compilation is null)
        {
            throw new InvalidOperationException("Failed to create compilation.");
        }

        return compilation;
    }

    private async Task TranspileCore(
        Compilation compilation,
        string outputDir,
        string newLine,
        int indent,
        bool referencedAssembliesTranspilation,
        SerializerOption serializerOption,
        NamingStyle namingStyle,
        EnumNamingStyle enumNamingStyle)
    {
        // Tapper
        var transpiler = new Transpiler(compilation, newLine, indent, referencedAssembliesTranspilation, serializerOption, namingStyle, enumNamingStyle, _logger);

        var generatedSourceCodes = transpiler.Transpile();

        // TypedSignalR.Client.TypeScript
        var signalrCodeGenerator = new TypedSignalRCodeGenerator(compilation, serializerOption, namingStyle, enumNamingStyle, referencedAssembliesTranspilation, _logger);

        var generatedSignalRSourceCodes = signalrCodeGenerator.Generate();

        await OutputToFiles(outputDir, generatedSourceCodes.Concat(generatedSignalRSourceCodes), newLine);
    }

    private async Task OutputToFiles(string outputDir, IEnumerable<GeneratedSourceCode> generatedSourceCodes, string newLine)
    {
        if (Directory.Exists(outputDir))
        {
            var tsFiles = Directory.GetFiles(outputDir, "*.ts");

            _logger.Log(LogLevel.Information, "Cleanup old files...");

            foreach (var tsFile in tsFiles)
            {
                File.Delete(tsFile);
            }

            var signalrDir = Path.Join(outputDir, "TypedSignalR.Client");

            if (Directory.Exists(signalrDir))
            {
                var tsSignalRFiles = Directory.GetFiles(signalrDir, "*.ts");

                foreach (var tsFile in tsSignalRFiles)
                {
                    File.Delete(tsFile);
                }
            }
            else
            {
                Directory.CreateDirectory(Path.Join(outputDir, "TypedSignalR.Client"));
            }
        }
        else
        {
            Directory.CreateDirectory(outputDir);
            Directory.CreateDirectory(Path.Join(outputDir, "TypedSignalR.Client"));
        }

        foreach (var generatedSourceCode in generatedSourceCodes)
        {
            using var fs = File.Create(Path.Join(outputDir, generatedSourceCode.SourceName));
            await fs.WriteAsync(Encoding.UTF8.GetBytes(generatedSourceCode.Content.NormalizeNewLines(newLine)));
        }
    }
}
