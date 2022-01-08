using System;
using Godot;

namespace Picalines.Godot.RequireChild
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequireChildAttribute : Attribute
    {
        public readonly NodePath ChildPath;

        public RequireChildAttribute(string childNodePath)
        {
            ChildPath = $"./{childNodePath}";
        }
    }
}
