using System.Text.RegularExpressions;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

public abstract class SignalContent : RowContent
{
	internal static bool TryParse(string additionalText, string signalType, string speedPrefix, out string speed, out string remainingText)
	{
		speed = "40";
		remainingText = string.Empty;
		
		if (!additionalText.Contains(signalType))
			return false;
		
		remainingText = additionalText.Replace(signalType, "").Trim();
		
		Match match = Regex.Match(remainingText, @$"{speedPrefix}\d+");

		if (!match.Success) 
			return true;
		
		speed = match.Value;
		remainingText = remainingText.Replace(speed, "").Trim();

		return true;
	}
}