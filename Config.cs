namespace MicroHIDReturn;

public class Config
{
    public string NoMicroHID { get; set; } = "<color=red>You dont have Micro HID to place.</color>";
    public string NotInHands { get; set; } = "<color=yellow>Equip Micro HID.</color>";
    public string PlacedMicroHID { get; set; } = "<color=green>You placed Micro HID on pedestal.</color>";
    public string PlacedMicroHIDCharging { get; set; } = "<color=green>You placed Micro HID on pedestal. It start charging.</color>";
    public bool ChargeOnPedestal {get; set;} = true;
    public bool PlayChargingSound {get; set;} = true; //Not working if ChargeOnPedestal = false
    public float ChargeStep {get; set;} = 0.005f; //Charge per second. 1=100% ; 0.01=1% ; 0.005=0.5%
}