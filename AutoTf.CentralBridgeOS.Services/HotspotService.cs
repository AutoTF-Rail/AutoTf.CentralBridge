using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class HotspotService : IDisposable
{
    private readonly Logger _logger = Statics.Logger;
    private readonly string dhcpConfigPath = "/etc/dnsmasq.conf";

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
        CommandExecuter.ExecuteSilent("sudo systemctl restart hostapd", false);
    }

    public void SetupDhcpConfig(string interfaceName)
    {
        string dhcpConfig = $"interface={interfaceName}\n" +
                            "dhcp-range=192.168.1.100,192.168.1.200,255.255.255.0,24h\n" +
                            "dhcp-option=3,192.168.1.1\n" +
                            "dhcp-option=6,192.168.1.1\n";

        File.WriteAllText(dhcpConfigPath, dhcpConfig);
        CommandExecuter.ExecuteSilent("sudo systemctl restart dnsmasq", false);
        _logger.Log("DHCP server configuration updated successfully!");
    }

    public void StopWifi()
    {
        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        _logger.Log("WiFi hotspot stopped.");
    }

    private void CheckDependencies()
    {
        string[] requiredTools = { "hostapd", "iw", "dnsmasq" };
        foreach (string tool in requiredTools)
        {
            _logger.Log($"Checking for {tool}...");
            try
            {
                if (CommandExecuter.ExecuteCommand($"which {tool}") == "")
                    throw new Exception();
            }
            catch
            {
                throw new Exception($"{tool} is not installed. Please install it using 'sudo apt-get install {tool}'.");
            }
        }
        _logger.Log("All dependencies are installed.");
    }

    public void Dispose()
    {
        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        CommandExecuter.ExecuteSilent("sudo systemctl stop dnsmasq", true);
        CommandExecuter.ExecuteSilent("sudo killall hostapd", true);
    }
}