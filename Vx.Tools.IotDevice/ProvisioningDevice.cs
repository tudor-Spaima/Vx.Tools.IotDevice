using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Client;
using System.Text;

namespace Vx.Tools.IotDevice;

public class ProvisioningDevice
{
    private readonly string RegistrationId = "35E079FB8DED47A1";
    private readonly string PrimaryKey = "YTZj0DU5ZG4tN2U0NC00MTdlLTg4MWEd";
    private readonly string GroupPrimaryKey = "SPYQ0dZd0VaxuKPwQ5WUK3JemUl3wHy1lgpsFS9eFLZ3KqAoAREmrwv/lAqHQeOSw8rxt5IylghfBV/JLMfR0A==";
    private readonly string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
    private readonly string IdScope = "0ne00B44F49";

    public ProvisioningDevice()
    { 
    }
    public async Task RunProvisioningAsync()
    {
        Console.WriteLine($"Initializing the device provisioning client...");

        using var security = new SecurityProviderSymmetricKey(
                RegistrationId,
                PrimaryKey,
                null);

        using ProvisioningTransportHandler transportHandler = new ProvisioningTransportHandlerHttp();

        var provClient = ProvisioningDeviceClient.Create(
            GlobalDeviceEndpoint,
            IdScope,
            security,
            transportHandler);

        Console.WriteLine("Registering with the device provisioning service...");
        DeviceRegistrationResult result = await provClient.RegisterAsync();

        Console.WriteLine($"Registration status: {result.Status}.");
        if (result.Status != ProvisioningRegistrationStatusType.Assigned)
        {
            Console.WriteLine($"Registration status did not assign a hub, so exiting this sample.");
            return;
        }

        Console.WriteLine($"Device {result.DeviceId} registered to {result.AssignedHub}.");

        Console.WriteLine("Creating symmetric key authentication for IoT Hub...");
        IAuthenticationMethod auth = new DeviceAuthenticationWithRegistrySymmetricKey(
            result.DeviceId,
            security.GetPrimaryKey());

        Console.WriteLine($"Testing the provisioned device with IoT Hub...");
        using var iotClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);

        Console.WriteLine("Sending a telemetry message...");
        using var message = new Message(Encoding.UTF8.GetBytes("TestMessage"));
        await iotClient.SendEventAsync(message);

        await iotClient.CloseAsync();
        Console.WriteLine("Finished.");

    }
}
