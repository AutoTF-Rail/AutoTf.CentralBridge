using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class HotspotService : IDisposable
{
    private const string DhcpConfigPath = "/etc/dnsmasq.conf";
    
    private readonly FileManager _fileManager;
    private readonly Logger _logger = Statics.Logger;

    public HotspotService(FileManager fileManager)
    {
        _fileManager = fileManager;
        Statics.ShutdownEvent += StopWifi;
    }
    
    public bool Configure()
    {
        _logger.Log("HOTSPOT: Configuring network");
		
        string interfaceName = "wlan1";
        string ssid = "CentralBridge-" + _fileManager.ReadFile("trainId", Statics.GenerateRandomString());
		
        Statics.CurrentSsid = _fileManager.ReadFile("trainId");
		
        string password = "CentralBridgePW";
        try
        {
            NetworkConfigurator.SetStaticIpAddress("192.168.0.1", "24");
            NetworkConfigurator.SetStaticIpAddress("192.168.1.1", "24", "wlan1");
            _logger.Log("HOTSPOT: Successfully set local IP.");
			
            StartWifi(interfaceName, ssid, password);
            SetupDhcpConfig(interfaceName);
			
            _logger.Log($"HOTSPOT: Started WIFI as: {ssid}");
        }
        catch (Exception ex)
        {
            _logger.Log("HOTSPOT: ERROR: Could not configure network");
            _logger.Log($"HOTSPOT: ERROR: {ex.Message}");
            return false;
        }

        return true;
    }
    
    // Only call this once the NetworkManager has tried to sync. Due to MAC Addresses maybe still being synced.
    private void StartWifi(string interfaceName, string ssid, string password)
    {
        string configPath = "/etc/hostapd/hostapd.conf";
        string defaultConfigPath = "hostapd.conf.default";

        // Create it, if it doesn't exist
        _fileManager.ReadAllLines("/etc/hostapd/accepted_macs.txt");

        CheckDependencies();

        if (!File.Exists(configPath))
        {
            _logger.Log("HOTSPOT: Config not found.");
            
            if (!File.Exists(defaultConfigPath))
                throw new FileNotFoundException("HOTSPOT: ERROR: Default hostapd config not found in program directory!");
            
            File.Copy(defaultConfigPath, configPath, true);
            _logger.Log($"HOTSPOT: Default config copied to {configPath}");
        }

        string hostapdConfig = $"interface={interfaceName}\n" +
                               "driver=nl80211\n" +
                               $"ssid={ssid}\n" +
                               "hw_mode=g\n" +
                               "channel=6\n" +
                               "wpa=2\n" +
                               "wpa_strict_rekey=1\n" +
                               "ctrl_interface=/var/run/hostapd\n" +
                               "ctrl_interface_group=0\n" +
                               "ignore_broadcast_ssid=1\n" +
                               $"wpa_passphrase={password}\n" +
                               "wpa_key_mgmt=WPA-PSK\n" +
                               "rsn_pairwise=CCMP\n" + 
                               "macaddr_acl=1\n" + 
                               "ht_capab=[HT40+][SHORT-GI-20][SHORT-GI-40][RX-STBC1]\n" + 
                               "ieee80211n=1\n" + 
                               "tx_queue_data3_aifs=1\n" + 
                               "tx_queue_data3_cwmin=3\n" + 
                               "tx_queue_data3_cwmax=7\n" + 
                               "rts_threshold=2347\n" + 
                               "fragm_threshold=2346\n" + 
                               "accept_mac_file=/etc/hostapd/accepted_macs.txt\n";
        
        File.WriteAllText(configPath, hostapdConfig);
        
        _logger.Log("HOTSPOT: Hostapd config updated successfully!");
        CommandExecuter.ExecuteSilent("sudo systemctl restart hostapd", false);
    }

    public void SetupDhcpConfig(string interfaceName)
    {
        string dhcpConfig = $"interface={interfaceName}\n" +
                            "dhcp-range=192.168.1.100,192.168.1.200,255.255.255.0,24h\n" +
                            "dhcp-option=3,192.168.1.1\n" +
                            "dhcp-option=6,192.168.1.1\n";

        File.WriteAllText(DhcpConfigPath, dhcpConfig);
        
        CommandExecuter.ExecuteSilent("sudo systemctl restart dnsmasq", false);
        _logger.Log("HOTSPOT: DHCP server configuration updated successfully!");
    }

    public void StopWifi()
    {
        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        _logger.Log("HOTSPOT: WiFi hotspot stopped.");
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
                throw new Exception($"HOTSPOT: {tool} is not installed. Please install it using 'sudo apt-get install {tool}'.");
            }
        }
        
        _logger.Log("HOTSPOT: All dependencies are installed.");
    }

    public void Dispose()
    {
        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        CommandExecuter.ExecuteSilent("sudo systemctl stop dnsmasq", true);
        CommandExecuter.ExecuteSilent("sudo killall hostapd", true);
    }
}