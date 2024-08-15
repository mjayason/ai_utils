using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Azure;
using Azure.AI.OpenAI;
//using OpenAI;

namespace PromptExecutor
{
    public class MicrosoftOpenAIPromptExecutor:PromptExecutorBase
    {
        private static IConfigurationRoot Configuration;
        private static HttpClient HttpClient = new HttpClient();

        /*
        public  async Task<IDictionary<string,string>> SendPromptToLLM(IConfigurationRoot config, string fileName, string promptType)
        {

            IDictionary<string, string> dict = new Dictionary<string, string>();

            Configuration = config;
            string apiKey = Configuration["AzureOpenAI:APIKey"];
            string apiUrl = Configuration["AzureOpenAI:APIUrl"];
            string deploymentName = Configuration["AzureOpenAI:DeploymentName"]; // The deployment name in Azure OpenAI
            float temperature =  float.Parse(Configuration["AzureOpenAI:Temperature"]);
            int maxTokens = int.Parse(Configuration["AzureOpenAI:MaxTokens"]);
            string promptTemplate = Configuration[$"Prompts:{promptType}"];

            string text = await ReadPdfFileAsync(fileName);

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine($"No text extracted from {fileName}");
                return null;
            }

            string formattedPrompt = string.Format(promptTemplate, text);

            // Initialize the OpenAI client
            var client = new OpenAIClient(new Uri(apiUrl), new AzureKeyCredential(apiKey));

            // Set up the completions options
            CompletionsOptions completionsOptions = new CompletionsOptions()
            {
                Prompts = { formattedPrompt },
                Temperature = temperature,
                MaxTokens = maxTokens,
                NucleusSamplingFactor = 0.5f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
            };

            // Get the completions response
            Response<Completions> completionsResponse = await client.GetCompletionsAsync(deploymentName, completionsOptions);

            // Retrieve the completion text
           // string completion = completionsResponse.Value.Choices[0].Text;

            dict.Add("FileName", Path.GetFileName(fileName));
            dict.Add("Action", promptType);
            dict.Add("LLM Response", completionsResponse.Value.Choices.ToList().Join());

            return dict;
        }
        public async Task<IDictionary<string, string>> SendPromptToLLM(IConfigurationRoot config, string fileName, string promptType)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            Configuration = config;
            string apiKey = Configuration["AzureOpenAI:APIKey"];
            string apiUrl = Configuration["AzureOpenAI:APIUrl"];
            string deploymentName = Configuration["AzureOpenAI:DeploymentName"]; // The deployment name in Azure OpenAI
            string outputFileName = Configuration["FilePaths:OutputFileName"];
            float temperature = float.Parse(Configuration["AzureOpenAI:Temperature"]);
            int maxTokens = int.Parse(Configuration["AzureOpenAI:MaxTokens"]);
            string promptTemplate = Configuration[$"Prompts:{promptType}"];

            string text = await ReadPdfFileAsync(fileName);

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine($"No text extracted from {fileName}");
                return null;
            }

            string formattedPrompt = string.Format(promptTemplate, text);

            // Initialize the OpenAI client
            var client = new OpenAIClient(new Uri(apiUrl), new AzureKeyCredential(apiKey));

            // Set up the completions options
            ChatCompletionsOptions completionsOptions = new ChatCompletionsOptions()
            {
                Messages = { new ChatMessage("user", formattedPrompt) }, // Using ChatMessage to specify the role and content
                Temperature = temperature,
                MaxTokens = maxTokens,
                NucleusSamplingFactor = 0.5f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
            };

            // Get the completions response
            Response<ChatCompletions> completionsResponse = await client.GetChatCompletionsAsync(deploymentName, completionsOptions);

            // Retrieve the completion text
            string completionText = completionsResponse.Value.Choices.FirstOrDefault()?.Message.Content;

            dict.Add("FileName", Path.GetFileName(fileName));
            dict.Add("Action", promptType);
            dict.Add("LLM Response", completionText ?? "No response");
            dict.SaveToCsv(outputFileName);

            return dict;
        }
         */

        public async Task<IDictionary<string, string>> SendPromptToLLM(IConfigurationRoot config, string fileName, string promptType)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            Configuration = config;

            string apiKey = Configuration["AzureOpenAI:APIKey"];
            string apiUrl = Configuration["AzureOpenAI:APIUrl"];
            string deploymentName = Configuration["AzureOpenAI:DeploymentName"]; // The deployment name in Azure OpenAI
            string outputFileName = Configuration["FilePaths:OutputFileName"];
            float temperature = float.Parse(Configuration["AzureOpenAI:Temperature"]);
            int maxTokens = int.Parse(Configuration["AzureOpenAI:MaxTokens"]);
            int pagePerProcessCount = int.Parse(Configuration["FilePaths:pagePerProcessCount"]);
            string promptTemplate = Configuration[$"Prompts:{promptType}"];

            List<string> texts = await ReadPdfFileAsyncAsArray(fileName,pagePerProcessCount);
            int pageCount = 0;
            foreach (string text in texts) 
            {
                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine($"No text extracted from {fileName}");
                    return null;
                }

                string formattedPrompt = string.Format(promptTemplate, text);

                // Initialize the OpenAI client
                var client = new OpenAIClient(new Uri(apiUrl), new AzureKeyCredential(apiKey));

                // Set up the completions options
                ChatCompletionsOptions completionsOptions = new ChatCompletionsOptions()
                {
                    // Using ChatMessage to specify the role and content
                    Messages = { new ChatMessage("user", formattedPrompt) }, 
                    Temperature = temperature,
                    MaxTokens = maxTokens,
                    NucleusSamplingFactor = 0.5f,
                    FrequencyPenalty = 0.0f,
                    PresencePenalty = 0.0f,
                };

                // Get the completions response
                Response<ChatCompletions> completionsResponse = await client.GetChatCompletionsAsync(deploymentName, completionsOptions);

                // Retrieve the completion text
                string completionText = completionsResponse.Value.Choices.FirstOrDefault()?.Message.Content;

                dict.Add("FileName", Path.GetFileName(fileName));
                dict.Add("Action", promptType);
                dict.Add("LLM Response", completionText ?? "No response");
                dict.Add("PageBatchNumber", pageCount++.ToString());
                dict.SaveToCsv(outputFileName);

                Console.WriteLine($"Processed {Path.GetFileName(fileName)}, batch count {pageCount}");
                
                dict.Clear();
            }

            return dict;
        }
    }
}

            