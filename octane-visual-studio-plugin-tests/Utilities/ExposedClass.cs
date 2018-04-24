using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities
{
    public class ExposedClass : DynamicObject
    {
        private Type m_type;
        private Dictionary<string, Dictionary<int, List<MethodInfo>>> m_staticMethods;

        private ExposedClass(Type type)
        {
            m_type = type;

            m_staticMethods = new Dictionary<string, Dictionary<int, List<MethodInfo>>>();

            Type t = m_type;
            while (t != null)
            {
                var staticMethods = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                        .Where(m => !m.IsGenericMethod)
                        .GroupBy(m => m.Name)
                        .ToDictionary(
                            p => p.Key,
                            p => p.GroupBy(r => r.GetParameters().Length).ToDictionary(r => r.Key, r => r.ToList()));

                foreach (var item in staticMethods)
                {
                    if (!m_staticMethods.ContainsKey(item.Key))
                        m_staticMethods.Add(item.Key, item.Value);
                }

                t = t.BaseType;
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            // Get type args of the call
            Type[] typeArgs = ExposedObjectHelper.GetTypeArgs(binder);
            if (typeArgs != null && typeArgs.Length == 0) typeArgs = null;

            //
            // Try to call a non-generic instance method
            //
            if (typeArgs == null
                    && m_staticMethods.ContainsKey(binder.Name)
                    && m_staticMethods[binder.Name].ContainsKey(args.Length)
                    && ExposedObjectHelper.InvokeBestMethod(args, null, m_staticMethods[binder.Name][args.Length], out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Type t = m_type;
            while (t != null)
            {
                var propertyInfo = t.GetProperty(binder.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(null, value, null);
                    return true;
                }

                var fieldInfo = t.GetField(binder.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(null, value);
                    return true;
                }

                t = t.BaseType;
            }
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Type t = m_type;
            while (t != null)
            {
                var propertyInfo = t.GetProperty(
                    binder.Name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                if (propertyInfo != null)
                {
                    result = propertyInfo.GetValue(null, null);
                    return true;
                }

                var fieldInfo = t.GetField(
                    binder.Name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

                if (fieldInfo != null)
                {
                    result = fieldInfo.GetValue(null);
                    return true;
                }

                t = t.BaseType;
            }
            result = null;
            return false;
        }

        public static dynamic From(Type type)
        {
            return new ExposedClass(type);
        }
    }

}
