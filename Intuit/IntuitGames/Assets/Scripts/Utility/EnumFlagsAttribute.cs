﻿using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}