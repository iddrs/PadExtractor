using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using PadExtractor;
using PadExtractor.Layout;
using PadExtractor.Process;
using PadExtractor.Source;
using PadExtractor.Writer;

namespace PadExtractorTerminal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
            .Build();

            Console.WriteLine("Diretórios de origem:");
            string[] sourceDirs = [.. config.GetSection("Path:SourceDirs").GetChildren().Select(child => child.Value)];
            foreach (var dir in sourceDirs)
            {
                Console.WriteLine(dir);
            }

            Console.WriteLine("Diretório do layout:");
            Console.WriteLine(config["Path:LayoutDir"]);

            Console.WriteLine("ConnectionStr:");
            Console.WriteLine(config["Db:ConnectionStr"]);

            ProgressMonitor monitor = new ProgressMonitor();

            SourceRepository sourceRepository = new PadExtractor.Source.SourceRepository(sourceDirs);
            LayoutRepository layoutRepository = new PadExtractor.Layout.LayoutRepository(config["Path:LayoutDir"]);
            WriterRepository writerRepository = new WriterRepository(config["Db:ConnectionStr"]);
            Processor processor = new Processor(sourceRepository, layoutRepository, writerRepository, monitor);
            processor.Process();
        }
    }
}
