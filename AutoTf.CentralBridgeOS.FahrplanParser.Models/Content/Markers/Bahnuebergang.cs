using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

public class Bahnuebergang : RowContent
{
	private Bahnuebergang(string kilometer)
	{
		Kilometer = kilometer;
	}

	public string Kilometer { get; set; }
	
	public override string GetPrint()
	{
		return $"Bü km {Kilometer} ET";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;

		if (!additionalText.Contains("Bü"))
			return false;
		
		// TODO: Is this "ET" of any importance?/Is there a different sign sometimes?
		string kilometer = additionalText.Replace("Bü", "").Replace("km", "").Replace("ET", "");
		
		content = new Bahnuebergang(kilometer);
		return true;
	}
}