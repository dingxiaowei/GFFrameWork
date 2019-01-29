using GF.Debug;
using GFFramework;
using GFFramework.GameStart;
using UnityEngine;

namespace Game
{
    [GameStartAtrribute(0)]
    public class LocalGameStart : IGameStart
    {
        public void Start()
        {
            Debugger.Log("本地代码启动!");
            Debugger.Log("准备启动热更逻辑!");

            GameObject.Find("GFFrame").GetComponent<GFLauncher>().Launch();
        }

        public void Update()
        {
            
        }

        public void LateUpdate()
        {
            
        }
    }
}