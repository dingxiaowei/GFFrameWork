using GFFramework.ScreenView;
using GFFramework.UI;
using UnityEngine;

[ScreenView("main",true)]
public class ScreenView_Main : IScreenView
{
    public string Name { get; private set; }
    public bool IsLoad { get; private set;     }

    public void BeginInit()
    {
        //一定要设置为true，否则当前是未加载状态
        this.IsLoad = true;

        //加载窗口, 0是窗口id,建议自行换成枚举
        UIManager.Inst.LoadWindows((int) WinEnum.Win_Menu);
        UIManager.Inst.ShowWindow((int) WinEnum.Win_Menu);
        Debug.Log("进入主界面");
    }

    public void BeginExit()
    {
        UIManager.Inst.CloseWindow((int) WinEnum.Win_Menu);
    }

    public void Update(float delta)
    {
        
    }

    public void FixedUpdate(float delta)
    {
       
    }
}