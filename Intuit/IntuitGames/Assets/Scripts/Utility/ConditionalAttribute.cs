using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;
using System;

/// <summary>
/// Makes the property read-only if the condition is not met.
/// </summary>
public class ConditionalAttribute : PropertyAttribute
{
    public string PropertyName { get; set; }
    public object ConditionalValue { get; set; }

    public ConditionalAttribute(string propertyName, object conditionalValue)
    {
        PropertyName = propertyName; ConditionalValue = conditionalValue;
    }
}