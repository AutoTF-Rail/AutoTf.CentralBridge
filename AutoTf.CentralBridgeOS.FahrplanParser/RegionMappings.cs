using System.Drawing;

namespace AutoTf.CentralBridgeOS.FahrplanParser;

public static class RegionMappings
{
	public static readonly List<Rectangle> Rows =
	[
		new Rectangle(74, 89, 998, 37),
		new Rectangle(74, 129, 998, 37),
		new Rectangle(74, 168, 998, 37),
		new Rectangle(74, 208, 998, 37),
		new Rectangle(74, 248, 998, 37),
		new Rectangle(74, 288, 998, 37),
		new Rectangle(74, 327, 998, 37),
		new Rectangle(74, 367, 998, 37),
		new Rectangle(74, 406, 998, 37),
		new Rectangle(74, 445, 998, 37),
		new Rectangle(74, 484, 998, 37),
		new Rectangle(74, 524, 998, 37),
		new Rectangle(74, 562, 998, 37),
		new Rectangle(74, 602, 998, 37),
		new Rectangle(74, 641, 998, 37)
	];
	
	// Absolut
	public static readonly List<Rectangle> LocationPoints =
	[
		new Rectangle(169, 367, 31, 37),
		new Rectangle(169, 405, 31, 37),
		new Rectangle(169, 445, 31, 37),
		new Rectangle(169, 483, 31, 37),
		new Rectangle(169, 523, 31, 37),
		new Rectangle(169, 562, 31, 37),
		new Rectangle(169, 601, 31, 37)
	];
	
	// Absolut
	public static readonly List<Rectangle> LocationPointsHektometer =
	[
		new Rectangle(222, 372, 108, 37),
		new Rectangle(222, 410, 108, 37),
		new Rectangle(222, 450, 108, 37),
		new Rectangle(222, 489, 108, 37),
		new Rectangle(222, 528, 108, 37),
		new Rectangle(222, 567, 108, 37),
		new Rectangle(222, 606, 108, 37)
	];
	
	public static readonly Rectangle PlanValidity = new Rectangle(348, 11, 418, 34);
	public static readonly Rectangle TrainNumber = new Rectangle(16, 11, 129, 34);
	public static readonly Rectangle Delay = new Rectangle(604, 694, 115, 26);
	public static readonly Rectangle Time = new Rectangle(991, 9, 153, 34);
	public static readonly Rectangle Date = new Rectangle(693, 11, 178, 34);
	public static readonly Rectangle NextStop = new Rectangle(713, 57, 363, 29);

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