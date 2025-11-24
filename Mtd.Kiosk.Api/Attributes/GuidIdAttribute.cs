using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Reflection;

namespace Mtd.Kiosk.Api.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
internal class GuidIdAttribute(bool isRequired = false) : Attribute
{
	public bool IsRequired { get; } = isRequired;
}

internal class GuidModelBinder(bool isRequired) : IModelBinder
{
	private readonly bool _isRequired = isRequired;

	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);

		if (valueProviderResult == ValueProviderResult.None)
		{
			if (_isRequired)
			{
				bindingContext.ModelState.AddModelError(bindingContext.FieldName, "The GUID is required.");
			}

			return Task.CompletedTask;
		}

		var value = valueProviderResult.FirstValue;

		if (_isRequired && string.IsNullOrWhiteSpace(value))
		{
			bindingContext.ModelState.AddModelError(bindingContext.FieldName, "The GUID is required.");
			return Task.CompletedTask;
		}

		if (!Guid.TryParse(value, out var guid))
		{
			bindingContext.ModelState.AddModelError(bindingContext.FieldName, "Invalid GUID format.");
			return Task.CompletedTask;
		}

		bindingContext.Result = ModelBindingResult.Success(guid);
		return Task.CompletedTask;
	}
}

internal class GuidModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		// Ensure we are working with a parameter or property that has the GuidId attribute
		if (context.Metadata is DefaultModelMetadata metadata && metadata.ContainerType != null)
		{
			// 1. Check for GuidId attribute on a method parameter
			if (metadata.ParameterName != null)
			{
				// Find the method that contains the parameter with the GuidId attribute
				var methodInfo = metadata.ContainerType
					.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.FirstOrDefault(method => method.GetParameters().Any(p => p.Name == metadata.ParameterName));

				var parameterInfo = methodInfo?.GetParameters().FirstOrDefault(p => p.Name == metadata.ParameterName);

				if (parameterInfo != null)
				{
					// Retrieve the GuidId attribute from the parameter
					var guidIdAttribute = parameterInfo.GetCustomAttribute<GuidIdAttribute>();
					if (guidIdAttribute != null)
					{
						return new GuidModelBinder(guidIdAttribute.IsRequired);
					}
				}
			}

			// 2. Check for GuidId attribute on a property
			if (metadata.PropertyName != null)
			{
				var propertyInfo = metadata.ContainerType.GetProperty(metadata.PropertyName);

				if (propertyInfo != null)
				{
					// Retrieve the GuidId attribute from the property
					var guidIdAttribute = propertyInfo.GetCustomAttribute<GuidIdAttribute>();
					if (guidIdAttribute != null)
					{
						return new GuidModelBinder(guidIdAttribute.IsRequired);
					}
				}
			}
		}

		return null;
	}
}
