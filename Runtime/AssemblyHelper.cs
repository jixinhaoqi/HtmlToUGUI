using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 程序集反射辅助工具，用于动态发现和实例化类型。
    /// </summary>
    public static class AssemblyHelper
    {
        private static List<string> _assemblyNames;
        private static List<Assembly> _assembly;

        /// <summary>
        /// 获取要扫描的程序集名称列表。
        /// </summary>
        public static List<string> GetAssemblyNames()
        {
            if (_assemblyNames == null)
                _assemblyNames = new List<string>() { "Xxhq.Htmltougui", "Xxhq.Htmltougui.Editor", "Assembly-CSharp" };
            return _assemblyNames;
        }
        /// <summary>
        /// 获取要扫描的已加载程序集列表。
        /// </summary>
        public static List<Assembly> GetAssemblies()
        {
            if(_assembly == null|| _assembly.Count != GetAssemblyNames().Count)
            {
                if(_assembly != null)
                    _assembly.Clear();
                else
                    _assembly = new List<Assembly>();
                foreach (var assemblyName in GetAssemblyNames())
                {
                    if(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(i=>i.GetName().Name == assemblyName) is Assembly assembly)
                        _assembly.Add(assembly);
                }
            }
            return _assembly;
        }

        /// <summary>
        /// 扫描所有程序集，创建指定基类型或接口的子类实例，排除自身的基类。
        /// </summary>
        /// <typeparam name="T">基类型或接口</typeparam>
        /// <returns>子类实例列表</returns>
        public static List<T> CreateSupTypeInstances<T>() where T : class
        {
            var list = new List<T>();
            var listType = new List<Type>();
            Type type = typeof(T);
            bool isInterface = type.IsInterface;
            foreach (var t in GetAssemblies().SelectMany(i => i.GetTypes()))
            {
                if (t.IsInterface || t.IsAbstract) continue;
                if (isInterface ? (!type.IsAssignableFrom(t)) : (!t.IsSubclassOf(type))) continue;
                listType.Add(t);
            }
            var remove = new HashSet<Type>();
            foreach (var i in listType)
            {
                foreach (var j in listType)
                {
                    if (i.IsSubclassOf(j))
                        remove.Add(j);
                }
            }
            foreach (var i in remove)
            {
                listType.Remove(i);
            }
            foreach (var i in listType)
            {
                list.Add((T)Activator.CreateInstance(i));
            }
            return list;
        }

        /// <summary>
        /// 向扫描列表中添加程序集名称。
        /// </summary>
        /// <param name="assemblyName">程序集的名称</param>
        public static void AddAssemblyName(string assemblyName)
        {
            if(!GetAssemblyNames().Contains(assemblyName))
            _assemblyNames.Add(assemblyName);
        }
    }
}
