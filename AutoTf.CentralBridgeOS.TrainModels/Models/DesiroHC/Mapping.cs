using System.Drawing;
using AutoTf.CentralBridgeOS.Models;

namespace AutoTf.CentralBridgeOS.TrainModels.Models.DesiroHC;

public class Mapping : RegionMappings
{
	public override List<Rectangle> Rows { get; } = new List<Rectangle>()
	{
		new Rectangle(67, 84, 930, 35),
		new Rectangle(67, 120, 930, 35),
		new Rectangle(67, 158, 930, 35),
		new Rectangle(67, 196, 930, 35),
		new Rectangle(67, 233, 930, 35),
		new Rectangle(67, 269, 930, 35),
		new Rectangle(67, 306, 930, 35),
		new Rectangle(67, 343, 930, 35),
		new Rectangle(67, 379, 930, 35),
		new Rectangle(67, 416, 930, 35),
		new Rectangle(67, 452, 930, 35),
		new Rectangle(67, 489, 930, 35),
		new Rectangle(67, 525, 930, 35),
		new Rectangle(67, 562, 930, 35),
		new Rectangle(67, 602, 930, 35)
	};
	
	// Absolute
	public override List<Rectangle> LocationPoints { get; } = new List<Rectangle>()
	{
		new Rectangle(151, 343, 29, 35),
		new Rectangle(151, 379, 29, 35),
		new Rectangle(151, 416, 29, 35),
		new Rectangle(151, 452, 29, 35),
		new Rectangle(151, 489, 29, 35),
		new Rectangle(151, 525, 29, 35),
		new Rectangle(151, 562, 29, 35)
	};
	
	// Absolut
	public override List<Rectangle> LocationPointsHektometer { get; } = new List<Rectangle>()
	{
		new Rectangle(205, 343, 101, 35),
		new Rectangle(205, 379, 101, 35),
		new Rectangle(205, 416, 101, 35),
		new Rectangle(205, 452, 101, 35),
		new Rectangle(205, 489, 101, 35),
		new Rectangle(205, 525, 101, 35),
		new Rectangle(205, 562, 101, 35)
	};
	
	public override Rectangle TrainNumber { get; } = new Rectangle(15, 9, 111, 31);
	public override Rectangle PlanValidity { get; } = new Rectangle(245, 7, 284, 31);
	public override Rectangle Date { get; } = new Rectangle(649, 5, 159, 32);
	public override Rectangle Time { get; } = new Rectangle(857, 5, 138, 31);
	
	public override Rectangle NextStop { get; } = new Rectangle(665, 50, 336, 25);
	
	public override Rectangle Delay { get; } = new Rectangle(562, 650, 107, 24);

	public override Rectangle Hektometer(Rectangle row)
	{
		return new Rectangle(row.X + 138, row.Y, 101, 35);
	}
	
	public override Rectangle Arrival(Rectangle row)
	{
		return new Rectangle(row.X + 705, row.Y, 109, 35);
	}
	
	public override Rectangle Departure(Rectangle row)
	{
		return new Rectangle(row.X + 835, row.Y, 99, 35);
	}
	
	public override Rectangle AdditionalText(Rectangle row)
	{
		return new Rectangle(row.X + 364, row.Y, 267, 35);
	}
	
	public override Rectangle SpeedLimit(Rectangle row)
	{
		return new Rectangle(row.X + 53, row.Y, 45, 35);
	}
	
	public override Rectangle YellowArea(Rectangle row)
	{
		return new Rectangle(row.X + 64, row.Y, 23, 5);
	}
}