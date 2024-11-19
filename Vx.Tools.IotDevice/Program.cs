using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vx.Core.Domain;
using Vx.Core.Logging;
using Vx.Sdk.Domain;

namespace Vx.Tools.IotDevice;
internal class Program
{
    private static IServiceProvider? ServiceProvider;
    /// <summary>
    /// 
    /// </summary>
    public static async Task<int> Main()
    {
        RegisterServices();
        Console.WriteLine("IoT Hub simulator #1 - Simulated device.");

        if (ServiceProvider is not null)
        {

            Console.WriteLine("Done.");

            Scenario scenarioEvent = new(new("HostName=babeldev-iothub.azure-devices.net;DeviceId=Scale123;SharedAccessKey=1ClwDq4thYpqKgrxx2qBSn6x0XtZ4YNMfDRT4hBisbc=", "Scale123"));
            scenarioEvent.ExecScenarioC1();
          
        }
        Console.WriteLine("Device simulator finished.");
        DisposeServices();
        return 0;
    }
    static void RegisterServices()
    {
        var environment = Env.GetParam("ASPNETCORE_ENVIRONMENT");
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", true, true)
            .AddEnvironmentVariables()
        .Build();

        var services = new ServiceCollection();
        ServiceProvider = services.BuildServiceProvider(true);

    }
    private static void DisposeServices()
    {
        if (ServiceProvider == null)
        {
            return;
        }
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}