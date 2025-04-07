using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

public class SwitchSide : RowContent
{
	public override string GetPrint()
	{
		return "Kopf machen";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;

		if (!additionalText.Contains("Kopf machen"))
			return false;
		
		content = new SwitchSide();
		return true;
	}
}