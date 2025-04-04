using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content;

public class UnknownContent : RowContent
{
	public UnknownContent(string content)
	{
		Content = content;
	}

	public string Content { get; set; }
	
	public override string GetPrint()
	{
		return $"Unknown: {Content}";
	}
}