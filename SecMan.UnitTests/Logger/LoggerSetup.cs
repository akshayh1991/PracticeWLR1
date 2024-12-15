using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;
using Xunit.Sdk;

namespace SecMan.UnitTests.Logger
{
    public class CustomLoggingAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

            // Get log file path from configuration
            string? logFilePath = configuration["UnitTestCasesPath:Path"];
            // Get the class name and method name for unique log file naming
            string? className = methodUnderTest.DeclaringType?.Name;
            string methodName = methodUnderTest.Name;
            logFilePath = Path.Combine(logFilePath, className, $"{methodName}_{DateTime.Now:HH-mm-ss}.txt");

            // Configure and initialize the logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Infinite)
                .CreateLogger();

            Log.Information($"Starting test: {className}.{methodName}");
        }

        public override void After(MethodInfo methodUnderTest)
        {
            Log.Information($"Completed test: {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");

            // Ensure the logger is closed and flushed
            Log.CloseAndFlush();
        }
    }
}
