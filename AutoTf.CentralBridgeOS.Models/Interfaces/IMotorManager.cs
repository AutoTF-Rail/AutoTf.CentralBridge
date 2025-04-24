using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Models.Interfaces;

public interface IMotorManager : IHostedService
{
    /// <summary>
    /// This is a bool given from the actual i2C connection.
    /// </summary>
    public bool AreMotorsAvailable { get; }

    /// <summary>
    /// This is like a overall block, to turn off all motors. If you set this to true, motors will be turned off by setting their pwm to 0.
    /// They can then not be moved again until this value is set to false.
    /// When turned back on, their pwm is set to 4096
    /// </summary>
    public bool AreMotorsReleased { get; set; }
    
    public void SetMotorAngle(int channel, double angle);
    
    public void MoveToMiddle();
    
    public double GetMotorAngle(int channel);

    public void TurnOffMotor(int channel);

    public void TurnOnMotor(int channel);
}