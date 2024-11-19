using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Text.Json;
using Vx.Core.Domain;

namespace Vx.Tools.IotDevice
{
    internal enum MessType
    {
        BATT = 1,
        ISSU = 2,
        EVNT = 3
    }
    internal class VxDevice
    {
        readonly string _deviceConnectionString;
        readonly string _senderDeviceProp;
        private readonly string pipelineObjectId = Env.GetParam("FUNCT_PIPELINE_CODE");

        internal VxDevice(string deviceConnectionString, string senderDeviceProp)
        {
            _deviceConnectionString = deviceConnectionString;
            _senderDeviceProp = senderDeviceProp;
        }
        internal async Task SendMessage(string requestId, string correlationId, MessType messType)
        {
            string processId = Guid.NewGuid().ToString();
            string message = $"Executing SendMessage(requestId={requestId},correlationId={correlationId},messType={messType})";
            using var deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Amqp);
            try
            {
                switch (messType)
                {
                    case MessType.EVNT:
                        await SendEvnt(deviceClient, correlationId, 10000);
                        break;
                }
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
                /// log error 
            }
            await deviceClient.CloseAsync();
        }
        private async Task SendEvnt(DeviceClient deviceClient, string corrrelationId, int delay = 1000)
        {
            // Delay parametr is only as example to be able to see the result on the console. No need to use in production  
            string messageBody = JsonSerializer.Serialize(
               new
               {
                   MType = "EVNT",
                   Ver = "1.1",
                   Pl = new List<dynamic>()
                     {
                             new {
                                 CT = DateTime.UtcNow.ToString(),
                                 PN = "WT",
                                 SN = "SC1",
                                 PV =  "0.62",
                                 AGR = "ABS"
                             },
                             new {
                                 CT = DateTime.UtcNow.ToString(),
                                 PN = "WT",
                                 SN = "SC2",
                                 PV =  "0.63",
                                 AGR = "ABS"
                             }
                     }
               });
            using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                CorrelationId = corrrelationId,
            };
            message.Properties.Add("SenderDevice", _senderDeviceProp);
            message.Properties.Add("CID", corrrelationId);
            await deviceClient.SendEventAsync(message);
            Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

            await Task.Delay(delay);
        }

    }
}
