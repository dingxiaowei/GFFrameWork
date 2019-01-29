using Game.Data;
using GF.Debug;
using GFFramework.ResourceMgr;
using GFFramework.ScreenView;
using GFFramework.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[UI((int)WinEnum.Win_Custom, "Windows/UICustomPanel")]
public class CUICustomWindow : WindowBase
{
    [TransformPath("Title")] private Text txt_Title;
    [TransformPath("Back")] private Button btn_Back;
    [TransformPath("Yes")] private Button btn_Yes;
    [TransformPath("Input Field/Text")] private InputField ifd_InputName;
    [TransformPath("Center/LeftBtn")] private Button btn_Del;
    [TransformPath("Center/RightBtn")] private Button btn_Add;
    [TransformPath("Scroll View/ViewPort/Content")] private Transform tf_Content;
    [TransformPath("Center/CenterNode/Text")] private Text txt_Num;

    Dial tempDialData;
    List<CUIInput> m_InputContents = new List<CUIInput>();
    public CUICustomWindow(string path) : base(path)
    {

    }

    public override void Init()
    {
        base.Init();
        txt_Title.text = "";
        tempDialData = null;
        this.ifd_InputName.onEndEdit.AddListener((value) =>
        {
            if (tempDialData == null)
            {
                tempDialData = new Dial();
            }
            tempDialData.Name = value;
        });

        this.btn_Back.onClick.AddListener(() =>
        {
            Debugger.Log("点击了Back按钮");
            goBack();
        });

        this.btn_Yes.onClick.AddListener(() =>
        {
            Debugger.Log("点击了Yes按钮");
            save();
            goBack();
        });

        this.btn_Del.onClick.AddListener(() =>
        {
            Debugger.Log("点击了Del按钮");
            modifyInputContent(false);
        });

        this.btn_Add.onClick.AddListener(() =>
        {
            Debugger.Log("点击了Add按钮");
            modifyInputContent(true);
        });
    }

    private void save()
    {
        if (tempDialData == null)
            return;
        var data = WindowData.Create("AddDial");
        data.AddData("key", tempDialData);
        UIManager.Inst.SendMessage((int)WinEnum.Win_Menu, data);
    }

    private void goBack()
    {
        this.Close();
        ScreenViewManager.Inst.MainLayer.BeginNavTo("main");
    }

    /// <summary>
    /// 增加或者减少输入框数量
    /// </summary>
    /// <param name="isAdd"></param>
    private void modifyInputContent(bool isAdd = true)
    {
        if (tempDialData == null)
            tempDialData = new Dial();
        if((tempDialData.Count == 12 && isAdd) || tempDialData.Count == 0 && !isAdd)
        {
            return;
        }

        if (isAdd)
        {
            tempDialData.Count++;
        }
        else
        {
            tempDialData.Count--;
        }
        showInputContents();
        txt_Num.text = tempDialData.Count.ToString();
    }

    private void showInputContents()
    {
        if (m_InputContents.Count == tempDialData.Count)
            return;
        while (m_InputContents.Count > tempDialData.Count)
        {
            GameObject.DestroyImmediate(m_InputContents[m_InputContents.Count - 1].Transform.gameObject);
            m_InputContents.RemoveAt(m_InputContents.Count - 1);
        }
        txt_Num.text = tempDialData.Count.ToString();
        IEnumeratorTool.StartCoroutine(createInputContent());
    }

    private System.Collections.IEnumerator createInputContent()
    {
        int index = 0;
        for (int i = 0; i < tempDialData.Count; i++)
        {
            createInputContent(index++);
            if (index % 10 == 0)
                yield return null;
        }
    }

    private void createInputContent(int index)
    {
        CUIInput inputContentWidget = null;
        if (m_InputContents.Count > index)
        {
            inputContentWidget = m_InputContents[index];
        }
        else
        {
            var o = CResources.Load<GameObject>("Views/InputContent");
            if (o == null)
            {
                Debugger.LogError("加载InputContent失败");
                return;
            }
            var obj = GameObject.Instantiate(o);
            obj.transform.parent = tf_Content.transform;
            obj.transform.localScale = Vector3.one;
            inputContentWidget = new CUIInput(obj.transform);
            inputContentWidget.Init();
            m_InputContents.Add(inputContentWidget);
        }
    }
}
