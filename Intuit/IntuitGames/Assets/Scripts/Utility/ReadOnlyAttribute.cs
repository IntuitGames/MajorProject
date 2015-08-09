using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;

/// <summary>
/// Property is grayed out and is only shown NOT editable.
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute
{
    public bool EditableWhilePlaying { get; set; }
    public bool EditableInEditor { get; set; }
}