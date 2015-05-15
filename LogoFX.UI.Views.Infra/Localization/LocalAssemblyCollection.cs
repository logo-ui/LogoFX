﻿using System.Collections.Generic;
using System.Reflection;

namespace LogoFX.UI.Views.Infra.Localization
{
    public sealed class LocalAssemblyCollection : Dictionary<AssemblyName, ResourceSetCollection>
    {
        public string Path { get; set; }
    }
}
