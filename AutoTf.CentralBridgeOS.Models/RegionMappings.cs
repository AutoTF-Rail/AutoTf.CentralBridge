using System.Drawing;

namespace AutoTf.CentralBridgeOS.Models;

// Technically part of AutoTf.CentralBridgeOS.FahrplanParser.Models, but we don't want to reference a Models project from a Models project
public abstract class RegionMappings
{
	public abstract List<Rectangle> Rows { get; }
	
	// Absolut
	public abstract List<Rectangle> LocationPoints { get; }
	
	// Absolut
	public abstract List<Rectangle> LocationPointsHektometer { get; }
	
	public abstract Rectangle PlanValidity { get; }
	public abstract Rectangle TrainNumber { get; }
	public abstract Rectangle Delay { get; }
	public abstract Rectangle Time { get; }
	public abstract Rectangle Date { get; }
	public abstract Rectangle NextStop { get; }

	public static Rectangle Hektometer(Rectangle row)
	{
		return new Rectangle(row.X + 148, row.Y, 108, 38);
	}
	
	public static Rectangle Arrival(Rectangle row)
	{
		return new Rectangle(row.X + 756, row.Y, 117, 37);
	}
	
	public static Rectangle Departure(Rectangle row)
	{
		return new Rectangle(row.X + 896, row.Y, 106, 37);
	}
	
	public static Rectangle AdditionalText(Rectangle row)
	{
		return new Rectangle(row.X + 394, row.Y, 283, 37);
	}
	
	public static Rectangle SpeedLimit(Rectangle row)
	{
		return new Rectangle(row.X + 56, row.Y, 46, 37);
	}
	
	public static Rectangle YellowArea(Rectangle row)
	{
		return new Rectangle(row.X + 63, row.Y, 30, 5);
	}
}