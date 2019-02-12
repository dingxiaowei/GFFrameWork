using Game.Data;
using GF.Debug;
using GFFramework;
using GFFramework.ScreenView;
using GFFramework.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[UI((int)WinEnum.Win_Game, "Windows/UIGamePanel")]
public class CUIGameWindow : WindowBase
{
    [TransformPath("Title")] private Text txt_Title;
    [TransformPath("Back")] private Button btn_Back;
    [TransformPath("More")] private Button btn_More;
    [TransformPath("Start")] private Button btn_Start;
    [TransformPath("Round")] private Transform tf_Round;
    private Text[] m_TextContents = new Text[12];

    bool isStart = false;
    float speed = 0f;
    /// <summary>
    /// 初速度
    /// </summary>
    float v0 = 10f;
    /// <summary>
    /// 减速度
    /// </summary>
    float a = -1.5f;
    /// <summary>
    /// 减速时间
    /// </summary>
    float t = 0f;
    /// <summary>
    /// Delt_V
    /// </summary>
    float vt = 0f;
    float sumTime = 0;
    int coroutine = -1;
    public CUIGameWindow(string path) : base(path)
    {

    }

    public override void Init()
    {
        base.Init();
        GFLauncher.OnFixUpdate = FixUpdate;
        RegisterAction("defaultAdd", OnMsg_GetData);
        for (int i = 0; i < m_TextContents.Length; i++)
        {
            m_TextContents[i] = tf_Round.Find(string.Format("Text{0}", i)).GetComponent<Text>();
        }

        this.btn_More.onClick.AddListener(() =>
        {
            Debugger.Log("点击了More按钮");
        });

        this.btn_Back.onClick.AddListener(() =>
        {
            this.Close();
            ScreenViewManager.Inst.MainLayer.BeginNavTo("main");
        });

        this.btn_Start.onClick.AddListener(() =>
        {
            //if (coroutine != -1)
            //    IEnumeratorTool.StopCoroutine(coroutine);
            //reset();
            isStart = true;
            //coroutine = IEnumeratorTool.StartCoroutine(startCountDown());
            if (deltT != 0 && deltT < t)
            {
                UberDebug.Log("小主,点击过快,手下留情!!!");
                return;
            }
            v0 = Random.Range(5f, 12f);
            a = Random.Range(-1f, -3f);
            t =  (vt - v0) / a;
            UberDebug.Log(string.Format("<color=yellow>v0:{0} a:{1} rotateTime:{2}</color>", v0, a,t));
        });
    }

    float deltT = 0f;
    private void FixUpdate()
    {
        if (isStart)
        {
            vt = v0 + a * deltT;
            if (vt <= 0)
            {
                vt = 0;
                deltT = 0;
                isStart = false;
            }
            tf_Round.transform.Rotate(Vector3.back, vt, Space.World);
            deltT += Time.deltaTime;
            t -= Time.deltaTime;
        }
    }

    public void OnMsg_GetData(WindowData data)
    {
        if (data != null)
        {
            var dial = data.GetData<Dial>("key");
            txt_Title.text = dial.Name;
            if (dial.Value != null)
            {
                int index = 0;
                for (int i = 0; i < m_TextContents.Length; i++)
                {
                    m_TextContents[i].text = dial.Value[index++];
                    if (index >= dial.Value.Count)
                        index = 0;
                }
            }
            else
            {
                for (int i = 0; i < m_TextContents.Length; i++)
                {
                    m_TextContents[i].text = "";
                }
            }
        }
    }

    public override void Close()
    {
        base.Close();
        if (coroutine != -1)
            IEnumeratorTool.StopCoroutine(coroutine);
    }

    public IEnumerator startCountDown()
    {
        if (isStart)
        {
            t += Time.deltaTime;
            speed = v0 + a * t;
            if (speed <= 0)
            {
                isStart = false;
            }
            tf_Round.transform.Rotate(Vector3.forward, speed);
            yield return new WaitForEndOfFrame();
        }
    }

    public override void Open(WindowData data = null)
    {
        base.Open(data);

    }

    private void reset()
    {
        speed = 0f;
        t = 0f;
        v0 = UnityEngine.Random.Range(20f, 40f);
        a = UnityEngine.Random.Range(-3f, -1f);
        isStart = false;
        tf_Round.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
    }
}
