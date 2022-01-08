using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using Godot;

namespace Picalines.Godot.RequireChild
{
    internal sealed class RequireChildHandler : Node
    {
        private static readonly Dictionary<Type, IEnumerable<MemberInfo>> _TargetMemberInfos = new();

        static RequireChildHandler()
        {
            ScanAssemblyForTargetMembers();
        }

        public override void _EnterTree()
        {
            GetTree().Connect("node_added", this, nameof(TryAssignRequiredChildren));
        }

        private void TryAssignRequiredChildren(Node node)
        {
            if (node.GetScript() is null)
            {
                return;
            }

            if (!_TargetMemberInfos.TryGetValue(node.GetType(), out var targetMembers))
            {
                return;
            }

            foreach (var member in targetMembers)
            {
                var path = member.GetCustomAttribute<RequireChildAttribute>().ChildPath;

                var requiredChild = node.GetNodeOrNull(path);

                var memberType = GetMemberType(member)!;

                if (requiredChild is null)
                {
                    throw new NullReferenceException($"{node.GetType()} requirers child node of type {memberType} at path {path}");
                }

                if (!memberType.IsAssignableFrom(requiredChild.GetType()))
                {
                    throw new InvalidCastException($"{node.GetType()} requires its child {path} to be assignable to {memberType}");
                }

                node.Set(member.Name, requiredChild);
            }
        }

        private static void ScanAssemblyForTargetMembers()
        {
            var validTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => type.Assembly.GetName().Name != "GodotSharp" && !type.IsDefined(typeof(CompilerGeneratedAttribute)))
                .Where(type => type.IsSubclassOf(typeof(Node)));

            foreach (var validType in validTypes)
            {
                RegisterTargetMembers(validType);
            }
        }

        private static void RegisterTargetMembers(Type validType)
        {
            static bool memberFilter(MemberInfo member, object _)
            {
                if (member.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    return false;
                }

                if (member.IsDefined(typeof(RequireChildAttribute)) && member is PropertyInfo property)
                {
                    if (!(property is { CanRead: true, CanWrite: true } && property.GetGetMethod(nonPublic: true).IsDefined(typeof(CompilerGeneratedAttribute), inherit: true)))
                    {
                        throw new InvalidOperationException($"{nameof(RequireChildAttribute)} can be used only on auto properties or fields ({member.DeclaringType}.{member.Name})");
                    }
                }

                return GetMemberType(member)?.IsSubclassOf(typeof(Node)) ?? false;
            }

            var members = validType.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                memberFilter,
                null);

            if (members.Any())
            {
                _TargetMemberInfos[validType] = members;
            }
        }

        private static Type? GetMemberType(MemberInfo member) => member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => null,
        };
    }
}
