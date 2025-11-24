using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Mtd.Kiosk.Api.Attributes;

/// <summary>
/// Custom validation attribute to validate Kiosk ID patterns.
/// Ensures the Kiosk ID is exactly 32 characters long and contains only lowercase letters and digits.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
public class IpAddressAttribute(bool required, bool privateIpsOnly = true) : ValidationAttribute
{
	private static readonly Regex _privateIpRegex = new("^(10\\.(\\d{1,3}\\.){2}\\d{1,3})$|^(172\\.(1[6-9]|2[0-9]|3[0-1])\\.(\\d{1,3}\\.)\\d{1,3})$|^(192\\.168\\.(\\d{1,3}\\.)\\d{1,3})$", RegexOptions.Compiled);
	private static readonly Regex _allIpsRegex = new("^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])(\\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])){3}$", RegexOptions.Compiled);

	/// <inheritdoc/>

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value == null)
		{
			if (required)
			{
				return new ValidationResult("An IP address is required.");
			}

			return ValidationResult.Success;
		}

		if (value is string stringValue)
		{
			if (!_allIpsRegex.IsMatch(stringValue))
			{
				return new ValidationResult("The IP address format is invalid.");
			}

			if (privateIpsOnly && !_privateIpRegex.IsMatch(stringValue))
			{
				return new ValidationResult("The IP address must be a private IP address.");
			}

			return ValidationResult.Success;
		}

		return new ValidationResult("The value must be a string.");
	}
}
