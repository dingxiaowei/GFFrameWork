using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GFFramework.ScreenView;
using GFFramework.UI;

[ScreenView("GamePanel")]
public class ScreenView_Game_Windows : IScreenView
{
    public string Name { get; private set; }
    public bool IsLoad { get; private set; }

    public void BeginInit()
    {
        //一定要设置为true，否则当前是未加载状态
        this.IsLoad = true;
        //打开 Game
        UIManager.Inst.LoadWindows((int)WinEnum.Win_Game);
        UIManager.Inst.ShowWindow((int)WinEnum.Win_Game);
        //
        Debug.Log("进入GamePanel");
    }

    public void BeginExit()
    {
        //退出设置为false，否则下次进入不会调用begininit
        this.IsLoad = false;
        Destory();
    }

    public void Destory()
    {

    }

    public void Update(float delta)
    {

    }


    public void FixedUpdate(float delta)
    {

    }
}
