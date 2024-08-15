using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using static Google.Rpc.Context.AttributeContext.Types;
using PromptExecutor;

class Program
{
    private static IConfigurationRoot Configuration;

    static async Task Main(string[] args)
    {
        InitializeConfiguration();

        string inputPath = Configuration["FilePaths:Input"];
        string outputPath = Configuration["FilePaths:Output"];
        string outputFileName = Configuration["FilePaths:OutputFileName"];
        
        args = Configuration["PromptExecutor:Execute"].Split(';');
        IDictionary<string, string> dict = new Dictionary<string, string>();

        try
        {
            string[] pdfFiles = Directory.GetFiles(inputPath, "*.pdf");

            foreach (string pdfFileName in pdfFiles)
            {
                Console.WriteLine($"Processing file {pdfFileName}");

                foreach (var arg in args)
                {
                    var response = await new MicrosoftOpenAIPromptExecutor().SendPromptToLLM(Configuration, pdfFileName, arg.ToLower());
                    //response.SaveToCsv(outputFileName);
                }
                
                //string destFileName = outputPath + Path.GetFileName(pdfFileName);
               // File.Move(pdfFileName, destFileName);
                
                Console.WriteLine($"Completed processing {Path.GetFileName(pdfFileName)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void InitializeConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();
    }
    
}
