namespace AutoMapper.Data
{
    using System;
    using System.Reflection;

    internal static class ReflectionExtensions
    {
        public static bool IsEnum(this Type type)
        {
#if DOTNET
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if DOTNET
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if DOTNET
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }
    }
}