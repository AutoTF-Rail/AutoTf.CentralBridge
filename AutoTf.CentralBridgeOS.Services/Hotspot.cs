using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class Hotspot
{
    private readonly Logger _logger = Statics.Logger;
    
	public void StartWifi(string interfaceName, string ssid, string password)
    {
        string configPath = "/etc/hostapd/hostapd.conf";
        string defaultConfigPath = "hostapd.conf.default";

        CheckDependencies();

        if (!File.Exists(configPath))
        {
            _logger.Log("Hostapd config not found. Creating it from default...");
            
            if (!File.Exists(defaultConfigPath))
                throw new FileNotFoundException("Default hostapd config not found in program directory!");
            
            File.Copy(defaultConfigPath, configPath, true);
            _logger.Log($"Default config copied to {configPath}");
        }

        string hostapdConfig = $"interface={interfaceName}\n" +
                               "driver=nl80211\n" +
                               $"ssid={ssid}\n" +
                               "hw_mode=g\n" +
                               "channel=6\n" +
                               "wpa=2\n" +
                               "ignore_broadcast_ssid=1\n" +
                               $"wpa_passphrase={password}\n" +
                               "wpa_key_mgmt=WPA-PSK\n" +
                               "rsn_pairwise=CCMP\n";
        
        File.WriteAllText(configPath, hostapdConfig);
        _logger.Log("Hostapd config updated successfully!");

        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        CommandExecuter.ExecuteSilent("sudo killall hostapd", true);
        CommandExecuter.ExecuteSilent("sudo systemctl start hostapd", false);
    }

    public void StopWifi()
    {
        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        _logger.Log("WiFi hotspot stopped.");
    }

    private void CheckDependencies()
    {
        string[] requiredTools = { "hostapd", "iw" };
        foreach (string tool in requiredTools)
        {
            _logger.Log($"Checking for {tool}...");
            try
            {
                CommandExecuter.ExecuteSilent($"which {tool}", false);
            }
            catch
            {
                throw new Exception($"{tool} is not installed. Please install it using 'sudo apt-get install {tool}'.");
            }
        }
        _logger.Log("All dependencies are installed.");
    }
}