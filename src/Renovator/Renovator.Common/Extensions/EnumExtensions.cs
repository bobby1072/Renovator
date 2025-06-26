using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Renovator.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T? GetAttribute<T>(this Enum value) where T: Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            
            return memberInfo.First().GetCustomAttribute<T>();
        }
        public static string GetDisplayName(this Enum value)
        {
            return value.GetAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}