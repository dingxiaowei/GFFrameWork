using System;
using GFFramework.Mgr;

namespace GFFramework.ScreenView
{
    public class ScreenViewAttribute: ManagerAtrribute
    {

        public bool IsDefault { get; private set; }
        public ScreenViewAttribute(string name, bool isDefault =false) :base(name)
        {
            this.IsDefault = isDefault;
        }
    }
}