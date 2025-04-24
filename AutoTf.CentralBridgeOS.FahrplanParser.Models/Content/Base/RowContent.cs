using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

public abstract class RowContent : IRowContent
{
	public abstract string GetPrint();
}