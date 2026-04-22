//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Text;

//namespace KafkaWorkflow.AppHost
//{
//    internal static class CommandResourceBuilderExtensions
//    {
//        public static IResourceBuilder<ProjectResource> WithPlaywrightRunCommand(this IResourceBuilder<ProjectResource> builder, int repeatCount = 25)
//        {
//            var commandOptions = new CommandOptions
//            {
//                IconName = "ArrowRepeatAll",
//                IsHighlighted = true,
//            };

//            builder.WithCommand(
//                name: "run-playwright-tests",
//                displayName: "Run Playwright Tests",
//                executeCommand: async (context) => {
//                    // Available from Aspire 9.4.0
//                    //var interactionService = context.ServiceProvider.GetRequiredService<IInteractionService>();
//                    //var prompt = await interactionService.PromptInputAsync("Repetition", "How many times do you want to repeat the Playwright tests?", new InteractionInput
//                    //{
//                    //    Label = "Repetition Count",
//                    //    Description = "Enter the number of times to repeat the Playwright tests.",
//                    //    InputType = InputType.Number,
//                    //    Required = true,
//                    //    Placeholder = "25",
//                    //});

//                    //if (prompt.Canceled)
//                    //{
//                    //    return CommandResults.Success();
//                    //}
//                    return await OnRunCommand(builder, context, $"dotnet test")
//            },
//                commandOptions: commandOptions);

//            return builder;
//        }

//        private static async Task<ExecuteCommandResult> OnRunCommand(IResourceBuilder<ProjectResource> builder, ExecuteCommandContext context, string command)
//        {
//            var loggerService = context.ServiceProvider.GetRequiredService<ResourceLoggerService>();
//            var logger = loggerService.GetLogger(context.ResourceName);

//            var processStartInfo = new ProcessStartInfo()
//            {
//                FileName = "cmd",
//                RedirectStandardOutput = true,
//                RedirectStandardInput = true,
//                WorkingDirectory = builder.Resource.WorkingDirectory
//            };

//            var process = Process.Start(processStartInfo) ?? throw new InvalidOperationException("Failed to start process");
//            await process.StandardInput.WriteLineAsync($"{command} & exit");

//            string? line;
//            while ((line = await process.StandardOutput.ReadLineAsync()) != null)
//            {
//                logger.LogInformation("{Line}", line);
//            }

//            // Ensure process has exited before returning (optional but recommended)

//            await process.WaitForExitAsync();

//            //while (!process.StandardOutput.EndOfStream)
//            //{
//            //    string line = await process.StandardOutput.ReadLineAsync() ?? string.Empty;
//            //    logger.LogInformation("{Line}", line);
//            //}

//            return CommandResults.Success();
//        }
//    }
//}
