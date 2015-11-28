using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}