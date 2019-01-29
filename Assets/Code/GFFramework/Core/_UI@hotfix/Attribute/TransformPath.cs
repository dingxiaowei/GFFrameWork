using System;

namespace GFFramework.UI
{
    public class TransformPath : Attribute
    {
        public string Path;

        public TransformPath(string path)
        {
            this.Path = path;
        }
    }
}