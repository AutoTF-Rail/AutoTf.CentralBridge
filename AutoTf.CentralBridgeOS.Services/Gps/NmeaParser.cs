namespace AutoTf.CentralBridgeOS.Services.Gps;

public static class NmeaParser
{
	public static double ExtractSpeed(string nmea)
	{
		try
		{
			string[] parts = nmea.Split(',');

			if (parts.Length > 7 && double.TryParse(parts[7], out double speedKnots))
			{
				return speedKnots;
			}
		}
		// TODO: Catch?
		catch { }

		return 0;
	}
	
	public static bool ExtractGpsStatus(string nmea)
	{
		try
		{
			string[] parts = nmea.Split(',');

			if (parts.Length > 1)
			{
				string status = parts[2];
				return status == "A";
			}
		}
		catch
		{
			Console.WriteLine("Could not extract status");
		}

		return false; 
	}
	
	public static Tuple<double, double>? ExtractCoordinates(string nmea)
	{
		try
		{
			string[] parts = nmea.Split(',');

			if (parts.Length < 6 || string.IsNullOrEmpty(parts[1]) || string.IsNullOrEmpty(parts[3]))
				return null;

			double latitude = ConvertNmeaCoordinate(parts[1], parts[2]);
			double longitude = ConvertNmeaCoordinate(parts[3], parts[4]);

			return Tuple.Create(latitude, longitude);
		}
		catch
		{
			return null;
		}
	}
	
	private static double ConvertNmeaCoordinate(string value, string direction)
	{
		if (double.TryParse(value, out double coordinate))
		{
			double degrees = Math.Floor(coordinate / 100);
			double minutes = coordinate - (degrees * 100);
			double decimalDegrees = degrees + (minutes / 60);

			if (direction == "S" || direction == "W")
				decimalDegrees *= -1;

			return decimalDegrees;
		}
		return 0;
	}
}