using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;
using System;

/// <summary>
/// Creates a popup for a property. (Currently only supports booleans LOL)
/// </summary>
public class PopupAttribute : PropertyAttribute
{
    public string OverrideName { get; set; }
    public string[] DisplayedOptions { get; set; }

    public PopupAttribute(string[] displayedOptions)
    {
        if (displayedOptions.IsNullOrEmpty())
            throw new NullReferenceException();

        DisplayedOptions = displayedOptions;
    }
}