﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SpringCaching.Reflection
{
#if DEBUG && NET45
    public
#endif
    class DynamicAssembly
    {
        private AssemblyBuilder? _assemblyBuilder;
        private ModuleBuilder? _moduleBuilder;
        private string _guid = Guid.NewGuid().ToString("N").ToUpper();

#if DEBUG&&NET45
        public bool DEBUG_MODE = false;
        public string AssemblyName = "SpringCaching.Debug.dll";
#endif

        public AssemblyBuilder AssemblyBuilder
        {
            get
            {
                EnsureAssemblyBuilder();
                return _assemblyBuilder!;
            }
        }
        public ModuleBuilder ModuleBuilder
        {
            get
            {
                EnsureModuleBuilder();
                return _moduleBuilder!;
            }
        }
        private void EnsureAssemblyBuilder()
        {
            if (_assemblyBuilder == null)
            {
#if DEBUG&&NET45
                if (DEBUG_MODE)
                {
                    _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.RunAndSave);
                }
                else
                {
                    _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.Run);
                }
#else
                _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.Run);
#endif
            }
        }
        private void EnsureModuleBuilder()
        {
            EnsureAssemblyBuilder();
            if (_moduleBuilder == null)
            {
#if DEBUG&&NET45
                if (DEBUG_MODE)
                {
                    _moduleBuilder = _assemblyBuilder!.DefineDynamicModule("MainModule", AssemblyName);
                }
                else
                {
                    _moduleBuilder = _assemblyBuilder!.DefineDynamicModule("MainModule");
                }
#else
                _moduleBuilder = _assemblyBuilder!.DefineDynamicModule("MainModule");
#endif
            }
        }

    }
}