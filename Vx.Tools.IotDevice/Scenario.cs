namespace Vx.Tools.IotDevice;

internal class Scenario
{
    private readonly VxDevice _device;
    internal Scenario(VxDevice device)
    {
        _device = device;
    }
   
    internal void ExecScenarioC1()
    {
        Sequence sequence = new(_device);
        sequence.ExecSequenceEVNT();
    }
   
}
