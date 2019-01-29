using Game.Data;
using GFFramework.ResourceMgr;
using GFFramework.ScreenView;
using GFFramework.UI;
using UnityEngine;
using UnityEngine.UI;

public class CUIIcon : SubWindow
{
    private Button btn_Icon;
    private Image img_Icon;
    [TransformPath("Name")]
    private Text txt_Name;
    string panelName = "GamePanel";
    bool isDefaultAdd = false;
    Dial dialInfo;
    public CUIIcon(Transform transform) : base(transform)
    {
    }

    public override void Init()
    {
        base.Init();
        btn_Icon = this.Transform.GetComponent<Button>();
        img_Icon = this.Transform.GetComponent<Image>();
        btn_Icon.onClick.AddListener(() =>
        {
            //点击进入对应的转盘
            ScreenViewManager.Inst.MainLayer.BeginNavTo(panelName);
            var data = WindowData.Create("defaultAdd");
            data.AddData("key", dialInfo);
            if (!isDefaultAdd)
            {
                UIManager.Inst.SendMessage((int)WinEnum.Win_Game, data);
            }
        });
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Open(WindowData data = null)
    {
        base.Open();
        if (data != null)
        {
            foreach (var v in data.DataMap)
            {
                var dial = v.Value as Dial;
                dialInfo = dial;
                img_Icon.sprite = CResources.Load<Sprite>(dial.Icon);
                if (dial.DefaultAdd > 0)
                {
                    isDefaultAdd = true;
                    panelName = "CustomPanel";
                    txt_Name.text = dial.Name;
                }
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}
