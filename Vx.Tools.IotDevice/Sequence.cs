using Vx.Core.Domain;
namespace Vx.Tools.IotDevice;

internal class Sequence
{
    private readonly VxDevice _device;
    private readonly string pipelineObjectId = Env.GetParam("FUNCT_PIPELINE_CODE");
    readonly string correlationId = Guid.NewGuid().ToString();
  
    internal Sequence(VxDevice device)
    {
        _device = device;
    }
    internal void ExecSequenceEVNT()
    {
        string processId = Guid.NewGuid().ToString();
        _device.SendMessage(processId, correlationId, MessType.EVNT).GetAwaiter().GetResult();
    }
   
}
