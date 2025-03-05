using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services;

public class HotspotService : IHostedService
{
    private const string DhcpConfigPath = "/etc/dnsmasq.conf";
    
    private readonly FileManager _fileManager;
    private readonly TrainSessionService _trainSessionService;
    private readonly Logger _logger = Statics.Logger;

    public HotspotService(FileManager fileManager, TrainSessionService trainSessionService)
    {
        _fileManager = fileManager;
        _trainSessionService = trainSessionService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Configure();
        return Task.CompletedTask;
    }
    
    private void Configure()
    {
        _logger.Log("HOTSPOT: Configuring network");
		
        string interfaceName = "wlan1";
        
        _logger.Log($"Starting with SSID: {_trainSessionService.Ssid}");
		
        string password = "CentralBridgePW";
        
        try
        {
            string ipEnding = "1";
            if (_trainSessionService.LocalServiceState != BridgeServiceState.Master)
                ipEnding = "2";
            
            string ownIp = "192.168.0." + ipEnding;
            
            NetworkConfigurator.SetStaticIpAddress(ownIp, "24");
            NetworkConfigurator.SetStaticIpAddress("192.168.1.1", "24", "wlan1");
            
            _logger.Log("HOTSPOT: Successfully set local IP.");
			
            // TODO: Check if this creates conflicts/transfers are seamless when moving between Bridges
            StartWifi(interfaceName, password);
            
            if(ipEnding == "1")
                SetupDhcpConfig(interfaceName);
			
            _logger.Log($"HOTSPOT: Started WIFI as: {_trainSessionService.Ssid} with LAN IP {ownIp}.");
        }
        catch (Exception ex)
        {
            _logger.Log("HOTSPOT: ERROR: Could not configure network");
            _logger.Log(ex.ToString());
        }
    }

    // Only call this once the NetworkManager has tried to sync. Due to MAC Addresses maybe still being synced.
    private void StartWifi(string interfaceName, string password)
    {
        string configPath = "/etc/hostapd/hostapd.conf";

        // Create it, if it doesn't exist
        _fileManager.ReadAllLines("/etc/hostapd/accepted_macs.txt");

        CheckDependencies();

        string hostapdConfig = $"interface={interfaceName}\n" +
                               "driver=nl80211\n" +
                               $"ssid={_trainSessionService.Ssid}\n" +
                               "hw_mode=g\n" +
                               "channel=6\n" +
                               "wpa=2\n" +
                               "ctrl_interface=/var/run/hostapd\n" +
                               "ctrl_interface_group=0\n" +
                               "ignore_broadcast_ssid=2\n" +
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

    private void CheckDependencies()
    {
        string[] requiredTools = { "hostapd", "iw", "dnsmasq" };
        foreach (string tool in requiredTools)
        {
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _logger.Log("HOTSPOT: Shutting down.");
        CommandExecuter.ExecuteSilent("sudo systemctl stop hostapd", true);
        CommandExecuter.ExecuteSilent("sudo systemctl stop dnsmasq", true);
        CommandExecuter.ExecuteSilent("sudo killall hostapd", true);
    }
}