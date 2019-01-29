using Game.Data;
using GF.Debug;
using GFFramework.ResourceMgr;
using GFFramework.Sql;
using GFFramework.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 这个是ui的标签，
/// index 
/// resource 目录
/// </summary>
[UI((int)WinEnum.Win_Menu, "Windows/UIMenuPanel")]
public class CUIMenuWindow : WindowBase
{
    [TransformPath("Setting")] private Button btn_Setting;
    [TransformPath("More")] private Button btn_More;
    [TransformPath("Scroll View/ViewPort/Content")] private Transform tf_Content;
    public CUIMenuWindow(string path) : base(path)
    {

    }

    public override void Init()
    {
        base.Init();
        loadIcons();
        RegisterAction("AddDial", OnMsg_GetData);
        this.btn_More.onClick.AddListener(() =>
        {
            Debugger.Log("点击了More按钮");
        });

        this.btn_Setting.onClick.AddListener(() =>
        {
            Debugger.Log("点击了Setting按钮");
        });
    }

    public void OnMsg_GetData(WindowData data)
    {
        if (data != null)
        {
            var dial = data.GetData<Dial>("key");
            Debugger.Log("新增新转盘");
            Debugger.Log(dial);
        }
    }

    private void loadIcons()
    {
        //加载数据
        var dialTable = SqliteHelper.DB.GetTableRuntime().ToSearch<Dial>();
        foreach (var d in dialTable)
        {
            //Debug.LogError(JsonMapper.ToJson(d));
            var o = CResources.Load<GameObject>("Views/EnteranceIcon");
            var go = GameObject.Instantiate(o);
            go.transform.parent = tf_Content;
            go.transform.localScale = Vector3.one;
            var subIcon = new CUIIcon(go.transform);
            subIcon.Init();
            var data = WindowData.Create("IconsData");
            data.AddData("IconsData", d);
            subIcon.Open(data);
        }
    }
}
