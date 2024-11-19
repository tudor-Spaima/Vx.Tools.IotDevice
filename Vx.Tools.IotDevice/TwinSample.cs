using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace Vx.Tools.IotDevice;

public class TwinSample
{
    private readonly DeviceClient _deviceClient;

    public TwinSample(DeviceClient deviceClient)
    {
        _deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    }

    public async Task RunSampleAsync(TimeSpan sampleRunningTime)
    {
        Console.WriteLine("Press Control+C to quit the sample.");
        using var cts = new CancellationTokenSource(sampleRunningTime);
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
            Console.WriteLine("Cancellation requested; will exit.");
        };

        await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null);

        Console.WriteLine("Retrieving twin...");
        Twin twin = await _deviceClient.GetTwinAsync();

        Console.WriteLine("\tInitial twin value received:");
        Console.WriteLine($"\t{twin.ToJson()}");

        Console.WriteLine("Sending sample start time as reported property");
        TwinCollection reportedProperties = new TwinCollection();
        reportedProperties["sensors"] = 
            new SensorsArray()
                        {
                        n = new string[] { "CT_1", "CT_2", "CT_3", "CT_4", "CT_5", "CT_6", "SIG_1" },
                        t = new double[] { 0.4 , 0.5, 0.7, 0.4, 0.5, 0.5, 65235.0 },
                        i = new int[] { 100, 250, 100, 100, 100, 100, 41200 },
                        u = new string[] { "A", "A", "A", "A", "A", "A","Db" },
                        s = new int[] { 1, 0, 1, 1, 1, 1, 0 }
                        };
        reportedProperties["device"] =
            new Device()
            {
                po = new Outage()
                {
                    back = 5,
                    kill = 120,
                    Out = 5
                },
                modbus = new Modbus()
                {
                    ext = new Int32[] { 3, 0, 0 },
                    mon = 1,
                },
                ts = 3600
            };
        reportedProperties["info"] =
          new Info()
          {
              cell = new Cell()
             {
                 iccid= "8912230102141574891",
                 imei= "014789000079733",
                 lac = 11654,
                 cid = 31362060,
                 str = -120,
                 mcc = "302",
                 mnc = "220"
             },
              ver = new Ver()
              { 
                  cts  = new Cts() {
                      fw = "5.0.0-rc1",
                      hw = "F_or_older"
                  },
                  mod= new Modem () {
                      fw = "02.012 A-01.000.05",
                      hw = "BP43G",
                      rfw = "FW1.1.12"
                  },
                   ext = new Extension[] {
                           new() {id = "",fw = ""},
                           new() {id = "",fw = ""},
                           new() {id = "",fw = ""}
                   } 
              }
          };

        await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

        var timer = Stopwatch.StartNew();
        Console.WriteLine($"Use the IoT Hub Azure Portal or IoT Explorer utility to change the twin desired properties.");

        Console.WriteLine($"Waiting up to {sampleRunningTime} for receiving twin desired property updates ...");
        while (!cts.IsCancellationRequested
            && (sampleRunningTime == Timeout.InfiniteTimeSpan || timer.Elapsed < sampleRunningTime))
        {
            await Task.Delay(1000);
        }

        // This is how one can unsubscribe a callback for properties using a null callback handler.
        await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(null, null);
    }

    public record Device
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("po")]
        public Outage? po { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("modbus")]
        public Modbus? modbus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ts")]
        public int ts { get; set; }
    }
    public record Outage
    {
        public int back { get; set; }
        public int kill { get; set; }
        public int Out { get; set; }
    }
    public record Modbus
    {
        public int[] ext { get; set; }
        public int mon { get; set; }
    }
    public record Modem
    {
        public string fw { get; set; }
        public string hw { get; set; }
        public string rfw { get; set; }
    }
    public record Extension
    {
        public string id { get; set; }
        public string fw { get; set; }
    }
    public record Cts
    {
        public string fw { get; set; }
        public string hw { get; set; }
    }
    public record Info
    {
        public Cell cell { get; set; }
        public Ver ver { get; set; }
    }
    public record Ver
    {
        public Cts cts { get; set; }
        public Modem mod { get; set; }
        public Extension[] ext { get; set; }
    }
    public record Cell
    {
        public string iccid { get; set; }
        public string imei { get; set; }
        public int lac { get; set; }
        public int cid { get; set; }
        public int str { get; set; }
        public string mcc { get; set; }
        public string mnc { get; set; }
    }
    public record SensorsArray
    {
        /// <summary>
        /// Sensors Id
        /// </summary>
        [JsonPropertyName("n")]
        public string[] n { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Threshold values
        /// </summary>
        [JsonPropertyName("t")]
        public double[] t { get; set; } = Array.Empty<double>();
        /// <summary>
        /// Capture intervals
        /// </summary>
        [JsonPropertyName("i")]
        public int[] i { get; set; } = Array.Empty<int>();
        /// <summary>
        /// Sensors units
        /// </summary>
        [JsonPropertyName("u")]
        public string[] u { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Sensors status enable=1; desable=0
        /// </summary>
        [JsonPropertyName("s")]
        public int[] s { get; set; } = Array.Empty<int>();

    }
    private async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
    {
        var reportedProperties = new TwinCollection();

        Console.WriteLine("\tDesired properties requested:");
        Console.WriteLine($"\t{desiredProperties.ToJson()}");

        // For the purpose of this sample, we'll blindly accept all twin property write requests.
        foreach (KeyValuePair<string, object> desiredProperty in desiredProperties)
        {
            Console.WriteLine($"Setting {desiredProperty.Key} to {desiredProperty.Value}.");
            reportedProperties[desiredProperty.Key] = desiredProperty.Value;
        }

        Console.WriteLine("\tAlso setting current time as reported property");
        reportedProperties["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.UtcNow;

        await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
    }
}
