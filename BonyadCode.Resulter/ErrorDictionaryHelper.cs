using Microsoft.AspNetCore.Identity;

namespace BonyadCode.Resulter;

/// <summary>
/// Provides utility methods to build standardized error dictionaries for various sources
/// such as IdentityResult, FluentValidation, and exceptions.
/// </summary>
public static class ErrorDictionaryHelper
{
    /// <summary>
    /// Creates a dictionary containing a single key and multiple string values.
    /// </summary>
    public static Dictionary<string, string[]> OneKeyWithMultipleValuesError(string key, IEnumerable<string> valueList)
    {
        return new Dictionary<string, string[]> { { key, valueList.ToArray() } };
    }

    /// <summary>
    /// Creates a dictionary containing a single key and a single string value.
    /// </summary>
    public static Dictionary<string, string[]> OneKeyWithOneValueError(string key, string value)
    {
        return new Dictionary<string, string[]> { { key, [value] } };
    }

    /// <summary>
    /// Creates a dictionary where each key from the list is assigned the same single string value.
    /// </summary>
    public static Dictionary<string, string[]> MultipleKeysWithOneValueError(IEnumerable<string> keyList, string value)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var key in keyList)
        {
            errors[key] = [value];
        }

        return errors;
    }

    /// <summary>
    /// Creates a dictionary where each key is mapped to a corresponding value from the value list.
    /// </summary>
    public static Dictionary<string, string[]> MultipleKeysWithMultipleValuesError(IEnumerable<string> keyList,
        IList<string> valueList)
    {
        var errors = new Dictionary<string, string[]>();
        var i = 0;
        foreach (var key in keyList)
        {
            errors[key] = [valueList[i]];
            i++;
        }

        return errors;
    }

    /// <summary>
    /// Converts an IdentityResult into a dictionary with error codes as keys and their descriptions as values.
    /// </summary>
    public static Dictionary<string, string[]> IdentityError(IdentityResult identityResult)
    {
        return identityResult.Errors.ToDictionary(ie => ie.Code, ie => new[] { ie.Description });
    }

    /// <summary>
    /// Converts a FluentValidation ValidationResult into a dictionary with property names as keys and error messages as values.
    /// </summary>
    public static Dictionary<string, string[]> FluentValidationResultError(
        FluentValidation.Results.ValidationResult validationResult)
    {
        return validationResult.Errors.ToDictionary(
            vf => !vf.PropertyName.Contains('.')
                ? vf.PropertyName
                : vf.PropertyName.Split('.')[vf.PropertyName.Split('.').Length - 1],
            vf => new[] { vf.ErrorMessage });
    }

    /// <summary>
    /// Converts a System.ComponentModel.DataAnnotations.ValidationResult into a dictionary.
    /// </summary>
    public static Dictionary<string, string[]> ValidationResultError(
        System.ComponentModel.DataAnnotations.ValidationResult validationResult)
    {
        return new Dictionary<string, string[]>
        {
            {
                validationResult.ErrorMessage ?? "One or more validation errors occurred.",
                validationResult.MemberNames.ToArray()
            }
        };
    }

    /// <summary>
    /// Creates a dictionary from a property name and a list of invalid items.
    /// </summary>
    public static Dictionary<string, string[]> ValidationError(string propertyName, IList<string> invalidItems)
    {
        return new Dictionary<string, string[]> { { propertyName, invalidItems.ToArray() } };
    }

    /// <summary>
    /// Converts the public properties of an exception into a dictionary with property names and their string representations.
    /// </summary>
    public static Dictionary<string, string[]> ExceptionError(Exception ex)
    {
        var dictionary = new Dictionary<string, string[]>();
        foreach (var property in ex.GetType().GetProperties())
        {
            dictionary[property.Name] = [property.GetValue(ex)?.ToString() ?? string.Empty];
        }

        return dictionary;
    }
}