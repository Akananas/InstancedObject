using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

namespace InstancedObject
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    public sealed class InstancedObjectAttribute : PropertyAttribute{}
}