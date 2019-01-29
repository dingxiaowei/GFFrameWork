using GFFramework.UI;
using UnityEngine;
using UnityEngine.UI;

public class CUIInput : SubWindow
{
    [TransformPath("Index")]
    private Text txt_Index;

    [TransformPath("Input Field/Text")]
    private InputField ifd_Text;


    public CUIInput(Transform transform) : base(transform)
    {

    }

    public override void Init()
    {
        base.Init();

    }

    public override void Close()
    {
        base.Close();
    }

    public override void Open(WindowData data = null)
    {
        base.Open();

    }

    public override void Destroy()
    {
        base.Destroy();
    }
}
