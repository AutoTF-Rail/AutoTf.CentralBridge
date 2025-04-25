using System.Drawing;

namespace AutoTf.CentralBridge.Models;

// Technically part of AutoTf.FahrplanParser.Models, but we don't want to reference a Models project from a Models project
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

	public abstract Rectangle Hektometer(Rectangle row);

	public abstract Rectangle Arrival(Rectangle row);

	public abstract Rectangle Departure(Rectangle row);

	public abstract Rectangle AdditionalText(Rectangle row);

	public abstract Rectangle SpeedLimit(Rectangle row);

	public abstract Rectangle YellowArea(Rectangle row);
}