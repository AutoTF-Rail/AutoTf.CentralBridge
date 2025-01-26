using OtpNet;

namespace AutoTf.CentralBridgeOS.Services;

public class CodeValidator
{
	private readonly FileManager _fileManager;

	public CodeValidator(FileManager fileManager)
	{
		_fileManager = fileManager;
	}
	
	public bool ValidateCode(string code, string keySerialNumber, DateTime timeOfCode)
	{
		// KeySerialNum:secret
		string[] allKeys = _fileManager.ReadAllLines("keys", "[]");
		string? secret = allKeys.FirstOrDefault(x => x.StartsWith($"{keySerialNumber}:"));
		
		if (string.IsNullOrEmpty(secret))
			return false;

		secret = secret.Replace($"{keySerialNumber}:", "");
		
		byte[] secretBytes = Base32Encoding.ToBytes(secret);

		Totp totp = new Totp(secretBytes, 15, OtpHashMode.Sha256);
		
		return totp.ComputeTotp(timeOfCode) == code;
	}
}