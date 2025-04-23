using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using OtpNet;

namespace AutoTf.CentralBridgeOS.Services;

public class CodeValidator
{
	private readonly IFileManager _fileManager;

	public CodeValidator(IFileManager fileManager)
	{
		_fileManager = fileManager;
	}
	
	public CodeValidationResult ValidateCode(string code, string keySerialNumber, DateTime timeOfCode)
	{
		// KeySerialNum:secret
		string[] allKeys = _fileManager.ReadAllLines("keys", "[]");
		string? secret = allKeys.FirstOrDefault(x => x.StartsWith($"{keySerialNumber}:"));
		
		if (string.IsNullOrEmpty(secret))
			return CodeValidationResult.NotFound;

		secret = secret.Replace($"{keySerialNumber}:", "");
		
		byte[] secretBytes = Base32Encoding.ToBytes(secret);

		Totp totp = new Totp(secretBytes, 15, OtpHashMode.Sha256);
		
		return totp.ComputeTotp(timeOfCode) == code ? CodeValidationResult.Valid : CodeValidationResult.Invalid;
	}
}