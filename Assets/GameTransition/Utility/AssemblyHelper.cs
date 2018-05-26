using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameTransition.Utility {
    public static class AssemblyHelper {
        static readonly Dictionary<string, Assembly> assemblyMapping = new Dictionary<string, Assembly>();

        static bool initialized;
        public static void Initialize() {
            if( initialized )
                return;

            initialized = true;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies ) {
                assemblyMapping.Add( assembly.FullName, assembly );
            }
        }

        public static Assembly GetAssembly(string name) {
            Initialize();

            Assembly assembly = null;
            assemblyMapping.TryGetValue( name, out assembly );
            return assembly;
        }
    }
}
