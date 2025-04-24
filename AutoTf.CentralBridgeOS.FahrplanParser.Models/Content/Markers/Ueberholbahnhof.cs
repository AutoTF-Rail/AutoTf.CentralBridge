using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

/// <summary>
/// Üst &lt;Name&gt;
/// </summary>
public class Ueberholbahnhof : RowContent
{
	private const string Tag = "Übf";

	private Ueberholbahnhof(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"{Tag} {Name}";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;

		if (!additionalText.Contains(Tag))
			return false;

		additionalText = additionalText.Replace(Tag, "").Trim();

		content = new Ueberholbahnhof(additionalText);
		return true;
	}
}