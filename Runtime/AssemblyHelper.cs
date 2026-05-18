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
        private static readonly string[] TargetAssemblyNames =
        {
            "Xxhq.Htmltougui",
            "Xxhq.Htmltougui.Editor"
        };
        private static readonly string AssemblyCSharp = "Assembly-CSharp";
        private static HashSet<Assembly> _assembly;
        // 要检测的目标程序集名称（作为被引用方）

        /// <summary>
        /// 获取要扫描的已加载程序集列表。
        /// </summary>
        public static HashSet<Assembly> GetAssemblies()
        {
            if (_assembly != null)
                _assembly.Clear();
            else
                _assembly = new HashSet<Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var referencedAssemblies = assembly.GetReferencedAssemblies();
                    bool referencesTarget = referencedAssemblies.Any(
                        refName => TargetAssemblyNames.Contains(refName.Name));
                    string name = assembly.GetName().Name;
                    if ((TargetAssemblyNames.Contains(name) || AssemblyCSharp == name) && !_assembly.Contains(assembly))
                    {
                        _assembly.Add(assembly);
                    }
                    else
                    {
                        if (referencesTarget)
                        {
                            _assembly.Add(assembly);
                        }
                        else if (_assembly.Contains(assembly))
                        {
                            _assembly.Remove(assembly);
                        }
                    }

                }
                catch
                {
                    // 某些动态程序集可能不支持 GetReferencedAssemblies，跳过即可
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

    }
}
