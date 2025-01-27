using Emgu.CV;

namespace AutoTf.CentralBridgeOS.Extensions;

public static class MatExtensions
{
	public static byte[]? Convert(this Mat? mat, string extension)
	{
		if (mat != null && !mat.IsEmpty)
		{
			return CvInvoke.Imencode(extension, mat);
		}

		return null;
	}
}