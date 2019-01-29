using System;

namespace GFFramework.UI
{
    public class BindModel : Attribute
    {
        public string Name;

        public BindModel(string name)
        {
            this.Name = name;
        }
    }
}