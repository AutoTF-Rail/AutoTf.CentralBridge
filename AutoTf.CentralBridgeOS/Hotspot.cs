using System.Diagnostics;

namespace AutoTf.CentralBridgeOS;

public class Hotspot
{
	public void StartWifi(string interfaceName, string ssid, string password)
    {
        string configPath = "/etc/hostapd/hostapd.conf";
        string defaultConfigPath = "hostapd.conf.default";

        CheckDependencies();

        if (!File.Exists(configPath))
        {
            Console.WriteLine("Hostapd config not found. Creating it from default...");
            
            if (!File.Exists(defaultConfigPath))
                throw new FileNotFoundException("Default hostapd config not found in program directory!");
            
            File.Copy(defaultConfigPath, configPath, true);
            Console.WriteLine($"Default config copied to {configPath}");
        }

        string hostapdConfig = $"interface={interfaceName}\n" +
                               "driver=nl80211\n" +
                               $"ssid={ssid}\n" +
                               "hw_mode=g\n" +
                               "channel=6\n" +
                               "wpa=2\n" +
                               $"wpa_passphrase={password}\n" +
                               "wpa_key_mgmt=WPA-PSK\n" +
                               "rsn_pairwise=CCMP\n";
        
        File.WriteAllText(configPath, hostapdConfig);
        Console.WriteLine("Hostapd config updated successfully!");

        ExecuteCommand("sudo systemctl stop hostapd");
        ExecuteCommand("sudo systemctl start hostapd");
    }

    public void StopWifi()
    {
        ExecuteCommand("sudo systemctl stop hostapd");
        Console.WriteLine("WiFi hotspot stopped.");
    }

    private void CheckDependencies()
    {
        string[] requiredTools = { "hostapd", "iw" };
        foreach (string tool in requiredTools)
        {
            Console.WriteLine($"Checking for {tool}...");
            try
            {
                ExecuteCommand($"which {tool}");
            }
            catch
            {
                throw new Exception($"{tool} is not installed. Please install it using 'sudo apt-get install {tool}'.");
            }
        }
        Console.WriteLine("All dependencies are installed.");
    }

    private void ExecuteCommand(string command)
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error) && !command.Contains("which"))
        {
            throw new Exception($"Error: {error}");
        }

        Console.WriteLine(output);
    }
}