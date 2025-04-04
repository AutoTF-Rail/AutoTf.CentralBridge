using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

/// <summary>
/// Abzw
/// </summary>
public class Abzweigung : RowContent
{
	private const string Tag = "Abzw";

	private Abzweigung(string name)
	{
		Name = name;
	}
	
	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"{Tag} {Name}";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;

		if (!additionalText.Contains(Tag))
			return false;

		additionalText = additionalText.Replace(Tag, "").Trim();

		content = new Abzweigung(additionalText);
		return true;
	}
}