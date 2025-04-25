using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Content;
using AutoTf.FahrplanParser.Content.Content.Icons;
using AutoTf.FahrplanParser.Content.Content.Icons.Tunnels;
using AutoTf.FahrplanParser.Content.Content.Markers;
using AutoTf.FahrplanParser.Content.Content.Signals;
using AutoTf.FahrplanParser.Content.Content.Signals.Vorsignal;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.FahrplanParser;

internal static class ContentResolver
{
	public static bool TryParseTunnel(Mat mat, Rectangle row, string additionalText, out IRowContent? content)
	{
		content = null;
		
		Mat tunnelArea = GetTunnelArea(mat, row);
		
		if (TunnelStart.TryParseIcon(tunnelArea))
			content = new TunnelStart(additionalText);
		else if (TunnelPart.TryParseIcon(tunnelArea))
			content = new TunnelPart();
		else if (TunnelEnd.TryParseIcon(tunnelArea))
			content = new TunnelEnd();

		tunnelArea.Dispose();

		return content != null;
	}

	public static bool TryParseIcon(Mat mat, Rectangle row, out IRowContent? content)
	{
		content = null;
		
		Mat iconArea = GetIconArea(mat, row);

		if (LzbStart.TryParseIcon(iconArea))
			content = new LzbStart();
		else if (LzbEnd.TryParseIcon(iconArea))
			content = new LzbEnd();
		else if (YenMarker.TryParseIcon(iconArea))
			content = new YenMarker();
		else if (Stumpfgleis.TryParseIcon(iconArea))
			content = new Stumpfgleis();

		iconArea.Dispose();

		return content != null;
	}
	
	public static bool TryParseStation(string additionalText, string arrival, string departure, out IRowContent? content)
	{
		content = null;
		
		try
		{
			if (Station.TryParse(additionalText, arrival, departure, out content))
				return true;
			
			if (NoStopStation.TryParse(additionalText, arrival, departure, out content))
				return true;
			
			return false;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TryParseSignal(string additionalText, out IRowContent? content)
	{
		if (AusfahrVorsignal.TryParse(additionalText, out content))
			return true;
		
		if (BlockVorsignal.TryParse(additionalText, out content))
			return true;
		
		if (AusfahrSignal.TryParse(additionalText, out content))
			return true;
		
		if (BlockSignal.TryParse(additionalText, out content))
			return true;
		
		if (EinfahrSignal.TryParse(additionalText, out content))
			return true;
		
		if (SelbstBlockSignal.TryParse(additionalText, out content))
			return true;
		
		if (ZwischenSignal.TryParse(additionalText, out content))
			return true;

		return false;
	}


	public static bool TryParseMarker(string additionalText, out IRowContent? content)
	{
		content = null;

		try
		{
			if (Abzweigung.TryParse(additionalText, out content))
				return true;
			
			if (Anschlussstelle.TryParse(additionalText, out content))
				return true;
			
			if (Ausweichanschlussstelle.TryParse(additionalText, out content))
				return true;
			
			if (Bahnuebergang.TryParse(additionalText, out content))
				return true;
			
			if (SwitchSide.TryParse(additionalText, out content))
				return true;
			
			if (Ueberholbahnhof.TryParse(additionalText, out content))
				return true;
			
			if (Ueberleitstelle.TryParse(additionalText, out content))
				return true;
			
			if (ZugFunk.TryParse(additionalText, out content))
				return true;
			
			if (LzbBlock.TryParse(additionalText, out content))
				return true;

			return false;
		}
		catch
		{
			return false;
		}
	}

	private static Mat GetIconArea(Mat mat, Rectangle row)
	{
		Rectangle roi = new Rectangle(row.X + 386, row.Y, 60, 44);
		Mat area = new Mat(mat, roi);
		CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);

		if (area.NumberOfChannels != 1)
			CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);

		return area;
	}

	private static Mat GetTunnelArea(Mat mat, Rectangle row)
	{
		Rectangle roi = new Rectangle(row.X + 348, row.Y, 35, 44);
		Mat area = new Mat(mat, roi);
		CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);

		if (area.NumberOfChannels != 1)
			CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);

		return area;
	}
}