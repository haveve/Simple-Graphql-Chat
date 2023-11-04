using Dapper;
using System.ComponentModel;
using System.Reflection;
using TimeTracker.Models;

namespace WebSocketGraphql.Services
{
    public static class UserMapper
    {
        static string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;

            var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);
            return (attrib?.Description ?? member.Name).ToLower();
        }

        static public void SetUserMapper()
        {
            var map = new CustomPropertyTypeMap(typeof(User), (type, columnName)
            => type.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop) == columnName.ToLower()));
            Dapper.SqlMapper.SetTypeMap(typeof(User), map);
        }
    }
}
