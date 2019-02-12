using System.IO;
using GFFramework;
using GFFramework.Helper;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

[M_UI(1, "UI/LoadingPanel")]
public class Windows_PreConfig : M_WindowBase
{
    [M_TransformPath("InputField")] private InputField ifd_InputField;

    [M_TransformPath("Progress")]
    private Text txt_DownloadProcess;

    [M_TransformPath("DownLoadSlider")]
    private Slider sld_Slider;

    [M_TransformPath("Download")] private Button btn_Download;

    [M_TransformPath("StartImmediate")] private Button btn_StartImmediate;

    //
    public Windows_PreConfig(string path) : base(path)
    {
    }

    public Windows_PreConfig(Transform transform) : base(transform)
    {
    }


    public override void Init()
    {
        base.Init();

        this.btn_StartImmediate.onClick.AddListener(Onclick_Pass);
        this.btn_Download.onClick.AddListener(Onclick_DownLoad);
        ifd_InputField.text = "192.168.1.92:8080/ftpserver";
    }


    void Onclick_Pass()
    {
        //直接启动
        GameObject.Find("GFFrame").GetComponent<GFLauncher>().Launch();
    }


    private void Onclick_DownLoad()
    {
        //删除本地的文件
        var cachedir = IPath.Combine(Application.persistentDataPath, Utils.GetPlatformPath(Application.platform));
        if (Directory.Exists(cachedir))
        {
            Directory.Delete(cachedir, true);
        }

        var url = "http://" + this.ifd_InputField.text;
        VersionContorller.Start(url, Application.persistentDataPath,
        (i, j) =>
        {
            this.txt_DownloadProcess.text = string.Format("{0}/{1}", i, j);
            this.sld_Slider.value = (i + 1) * 1f / j;
            //下载完毕
            if (i == j)
            {
                this.txt_DownloadProcess.text = "下载完毕";
                //启动
                GameObject.Find("GFFrame").GetComponent<GFLauncher>().Launch();
            }
        },
        (e) =>
        {
            this.txt_DownloadProcess.text = e; 
        });
    }
}