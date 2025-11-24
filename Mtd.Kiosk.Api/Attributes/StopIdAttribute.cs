using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Mtd.Kiosk.Api.Attributes;

/// <summary>
/// Custom validation attribute to validate ID patterns.
/// Ensures the stop id starts with a letter, followed by alphanumeric characters, and optionally ends with ":digit".
/// </summary>
/// <param name="required"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]

public class StopIdAttribute(bool required) : ValidationAttribute
{
	private static readonly Regex _idRegex = new(@"^[A-Za-z][A-Za-z0-9]+(:[0-9])?$", RegexOptions.Compiled);

	/// <inheritdoc/>

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value == null)
		{
			if (required)
			{
				// Return a ValidationResult with the error message
				return new ValidationResult("A stop id is required.");
			}

			return ValidationResult.Success;
		}

		if (value is not string stringValue || !_idRegex.IsMatch(stringValue))
		{
			// Return a ValidationResult with the error message
			return new ValidationResult("The stop id format is invalid. It must start with a letter, followed by alphanumeric characters, and optionally end with ':<digit>'.");
		}

		// If valid, return Success
		return ValidationResult.Success;
	}
}
