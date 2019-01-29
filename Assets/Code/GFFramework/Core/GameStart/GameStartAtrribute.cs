using System;

namespace GFFramework.GameStart
{
    public class GameStartAtrribute : Attribute
    {
        //游戏序号
        public int Index;

        public GameStartAtrribute(int index)
        {
            this.Index = index;
        }
    }
}