using Game.UI;
using GF.Debug;
using GFFramework.GameStart;
using System.Reflection;

namespace Game
{
    [GameStartAtrribute(0)]
    public class LocalGameStart : IGameStart
    {
        public void Start()
        {
            Debugger.Log("本地代码启动!");
            Debugger.Log("准备启动热更逻辑!");

            //启动程序
            //GameObject.Find("GFFrame").GetComponent<GFLauncher>().Launch();

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var t in types)
            {
                M_UIManager.Inst.CheckType(t);
            }
            M_UIManager.Inst.Init();

            //加载并显示1号窗口
            M_UIManager.Inst.LoadWindows(1);
            M_UIManager.Inst.ShowWindow(1);
        }

        public void Update()
        {
            
        }

        public void LateUpdate()
        {
            
        }
    }
}