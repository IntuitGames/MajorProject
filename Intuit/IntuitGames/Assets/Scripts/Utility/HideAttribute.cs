using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Property is grayed out and is only shown NOT editable.
/// </summary>
public class HideAttribute : PropertyAttribute
{
    public bool ShowInPlayMode { get; set; }
    public bool ShowInEditor { get; set; }
}
