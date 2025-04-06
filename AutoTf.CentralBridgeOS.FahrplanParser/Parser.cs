using System.Drawing;
using AutoTf.CentralBridgeOS.FahrplanParser.Extensions;
using AutoTf.CentralBridgeOS.FahrplanParser.Models;
using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridgeOS.FahrplanParser;

public class Parser : InfoParser
{
	public Parser(Tesseract engine) : base(engine) { }

	public void ReadPage(Mat mat, ref List<KeyValuePair<string, RowContent>> rows, ref List<KeyValuePair<string, string>> speedChanges)
	{
		List<Rectangle> rowsRoi = [..RegionMappings.Rows];
		rowsRoi.Reverse();

		List<RowContent> additionalContent = new List<RowContent>();
		
		foreach (Rectangle row in rowsRoi)
		{
			string hektometer = Hektometer(mat, row);
			string additionalText = AdditionalText(mat, row);
			string arrivalTime = Arrival(mat, row);
			string departureTime = Departure(mat, row);
			
			// If we don't have a hektometer, we will add it's info to the next one
			if (string.IsNullOrWhiteSpace(hektometer))
			{
				RowContent? content = ResolveContent(additionalText, arrivalTime, departureTime);
				
				if(content == null)
					continue;
				
				content = CheckForDuplicateContent(content, hektometer, rows);
				
				additionalContent.Add(content!);
			}
			else
			{
				rows.AddRange(additionalContent.Select(x => new KeyValuePair<string, RowContent>(hektometer, x)));
				additionalContent.Clear();
				
				string speedLimit = SpeedLimit(mat, row);
					
				if (!string.IsNullOrWhiteSpace(speedLimit))
				{
					// Skip if yellow (repeating)
					if (!mat.ContainsYellow(RegionMappings.YellowArea(row)))
					{
						// Skip if already contained
						if (speedChanges.Any())
						{
							if(speedChanges.TakeLast(3).All(x => x.Key != hektometer))
								speedChanges.Add(new KeyValuePair<string, string>(hektometer, speedLimit));
						}
						else
							speedChanges.Add(new KeyValuePair<string, string>(hektometer, speedLimit));
					}
				}

				RowContent? content = null;
				
				// We need to save this, because tunnelContent could return to being null, if it's duplicate, but in the check after the tunnel parsing, we need to know if we had a tunnel. (And which type)
				TunnelType tunnelType = TunnelType.None;
				
				if (TryParseTunnel(mat, row, additionalText, out RowContent? tunnelContent))
				{
					if(tunnelContent is TunnelContent tunnel)
						tunnelType = tunnel.GetTunnelType();
					
					// TODO: Different list?
					tunnelContent = CheckForDuplicateContent(tunnelContent!, hektometer, rows);
					
					if (tunnelContent != null)
						rows.Add(new KeyValuePair<string, RowContent>(hektometer, tunnelContent));
				}

				if (tunnelType != TunnelType.End)
				{
					if (string.IsNullOrWhiteSpace(additionalText))
					{
						if (TryParseIcon(mat, row, out RowContent? result))
							content = result;
					}
					else
					{
						if (tunnelType != TunnelType.Start)
							content = ResolveContent(additionalText, arrivalTime, departureTime);
						
						// No need for a null check, since the method does it
						// if(!string.IsNullOrWhiteSpace(arrivalTime))
						// 	content = parser.CheckForDuplicateStation(content, arrivalTime, additionalText, rows);
					}
					
				}
				if(content != null)
					content = CheckForDuplicateContent(content, hektometer, rows);

				if (content == null)
					continue;

				rows.Add(new KeyValuePair<string, RowContent>(hektometer, content));
			}
		}
	}
}