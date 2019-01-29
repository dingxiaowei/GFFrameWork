using System;
using System.Reflection;
using GFFramework;
using GFFramework.GameStart;
using SQLite4Unity3d;
using UnityEngine;
using GFFramework.ResourceMgr;
using UnityEngine.Serialization;
using System.Collections;
using GFFramework.Helper;
using System.IO;
using UnityEngine.UI;

namespace GFFramework
{
    public enum AssetLoadPath
    {
        Editor,
        Persistent,
        StreamingAsset
    }

    public class GFLauncher : MonoBehaviour
    {
        public AssetLoadPath CodeRoot = AssetLoadPath.Editor;
        public AssetLoadPath SQLRoot = AssetLoadPath.Editor;
        public AssetLoadPath ArtRoot = AssetLoadPath.Editor;
        public string FileServerUrl = "192.168.1.92";
        public string Port = "8080";
        public string ServerRootName = "ftpserver";
        static public Action OnStart { get; set; }
        static public Action OnUpdate { get; set; }
        static public Action OnLateUpdate { get; set; }
        static public Action OnFixUpdate { get; set; }

        public Slider slider;
        public Text loadTips;
        public GameObject panel;

        private void Awake()
        {
            this.gameObject.AddComponent<IEnumeratorTool>();
        }

        public IEnumerator Start()
        {
            string platform = Utils.GetPlatformPath(Application.platform);
            string localConfigPath = Application.persistentDataPath + "/" + platform + "_Server/" + platform + "_VersionConfig.json";
            if (File.Exists(localConfigPath))
            {
                UberDebug.Log("Resources already copy to persistantDataPath,return!");
                yield return null;
            }
            else
            {
                UberDebug.Log("First Start,Copy Resources!");
                StartCoroutine(CopyStreamAsset2PersistantPath(platform + "_Server/" + platform + "_VersionConfig.json"));

                yield return new WaitForSeconds(0.5f);
                AssetConfig localconf = null;

                if (File.Exists(localConfigPath))
                {
                    localconf = LitJson.JsonMapper.ToObject<AssetConfig>(File.ReadAllText(localConfigPath));
                    UberDebug.Log("version:" + localconf.Version);
                    foreach (var item in localconf.Assets)
                    {
                        if (-1 == item.LocalPath.IndexOf('.'))//非文件不处理
                        {
                            continue;
                        }
                        StartCoroutine(CopyStreamAsset2PersistantPath(platform + "/" + item.LocalPath));
                    }
                }
                else
                {
                    UberDebug.Log("not exist path:" + localConfigPath);
                }

            }
            yield return null;

            if (CodeRoot == AssetLoadPath.Persistent)
            {
                UberDebug.Log("Check for Update!", "red");
                //检查更新资源更新
                StartCoroutine(CheckUpdateResources(() =>
                {
                    //资源更新完毕,准备进入游戏
                    UberDebug.Log("Enter Game!", "red");
                    LaunchLocal();
                }));
            }
            else
            {
                //资源更新完毕,准备进入游戏
                UberDebug.Log("Enter Game!", "red");
                LaunchLocal();
            }
        }

        /// <summary>
        /// 拷贝包体文件
        /// </summary>
        /// <param name="absulateFilePath">文件相对路径</param>
        /// <returns></returns>
        public IEnumerator CopyStreamAsset2PersistantPath(string absulateFilePath)
        {
            string srcFilePath = "";
#if UNITY_EDITOR
            srcFilePath = "file:///" + Application.streamingAssetsPath + "/" + absulateFilePath;
#elif UNITY_ANDROID
            srcFilePath = "jar:file://" + Application.dataPath + "!/assets/" + absulateFilePath;
#elif UNITY_IPHONE
            srcFilePath = "file://" + Application.dataPath + "/Raw/" + absulateFilePath;
#endif
            string targetPath = Application.persistentDataPath + "/" + absulateFilePath;

            string filename = absulateFilePath.Substring(absulateFilePath.LastIndexOf('/') + 1);
            UberDebug.Log("Copy file to persistantPath:" + srcFilePath, "red");
            WWW www = new WWW(srcFilePath);
            yield return www;
            string directory = targetPath.Replace("/" + filename, "");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }


            if (!string.IsNullOrEmpty(www.error))
            {
                UberDebug.LogError(www.error + " " + srcFilePath);
            }
            else
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
                fs.Write(www.bytes, 0, www.bytes.Length);
                fs.Flush();
                fs.Close();
                if (File.Exists(targetPath))
                {
                    UberDebug.Log(string.Format("Copy to persistantPath OK,{0}", targetPath));
                }
                else
                {
                    UberDebug.Log(string.Format("Copy to persistantPath Failed,{0}", filename));
                }
            }
            www.Dispose();
        }

        int downLoadIndex = 0;
        int taskCount = 0;
        /// <summary>
        /// 检查需要更新的资源
        /// </summary>
        private IEnumerator CheckUpdateResources(Action CallBack)
        {
            var path = Application.persistentDataPath;
            string server = string.Format("http://{0}:{1}", FileServerUrl, Port);
            if (!string.IsNullOrEmpty(ServerRootName))
            {
                server = string.Format("{0}/{1}", server, ServerRootName);
            }
            //var t = VersionContorller.Start(server, path,
            //    (i, j) =>
            //    {
            //        downLoadIndex = i;
            //        taskCount = j;
            //        if (i == j && j == 0)
            //        {
            //            slider.value = 1f;
            //            loadTips.text = string.Format("资源加载完成，游戏初始化中");
            //            UberDebug.LogError("<color=yellow>no file to download,will enter game...</color>");
            //        }
            //        else if (i == j && j != 0)
            //        {
            //            slider.value = 1f;
            //            panel.SetActive(false);
            //            loadTips.text = string.Format("资源加载完成，游戏初始化中");
            //            UberDebug.Log("<color=yellow>Resource download finished,will enter game...</color>");
            //        }
            //        else
            //        {
            //            float progress = (i + 1) * 1f / j;
            //            slider.value = progress;
            //            loadTips.text = string.Format("资源加载进度：[{0}]", progress.ToString("P"));
            //            Debug.LogFormat("<color=yellow>资源更新进度：{0}/{1}</color>", i, j);
            //        }
            //    },
            //    (error) =>
            //    {
            //        Debug.LogError("错误:" + error);
            //    }, CallBack);
            IEnumeratorTool.StartCoroutine(VersionContorller.IEStart(server, path,
                (i, j) =>
                {
                    downLoadIndex = i;
                    taskCount = j;
                    if (i == j && j == 0)
                    {
                        slider.value = 1f;
                        loadTips.text = string.Format("资源加载完成，游戏初始化中");
                        UberDebug.LogError("<color=yellow>no file to download,will enter game...</color>");
                    }
                    else if (i == j && j != 0)
                    {
                        slider.value = 1f;
                        panel.SetActive(false);
                        loadTips.text = string.Format("资源加载完成，游戏初始化中");
                        UberDebug.Log("<color=yellow>Resource download finished,will enter game...</color>");
                    }
                    else
                    {
                        float progress = (i + 1) * 1f / j;
                        slider.value = progress;
                        loadTips.text = string.Format("资源加载进度：[{0}]", progress.ToString("P"));
                        Debug.LogFormat("<color=yellow>资源更新进度：{0}/{1}</color>", i, j);
                    }
                },
                (error) =>
                {
                    Debug.LogError("错误:" + error);
                }, CallBack));
            yield return null;
        }


        #region 启动非热更逻辑

        /// <summary>
        /// 启动本地代码
        /// </summary>
        public void LaunchLocal()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();


            var istartType = typeof(IGameStart);
            foreach (var t in types)
            {
                if (t.IsClass && t.GetInterface("IGameStart") != null)
                {
                    var attr = t.GetCustomAttribute(typeof(GameStartAtrribute), false);
                    if (attr != null)
                    {
                        var gs = Activator.CreateInstance(t) as IGameStart;

                        //注册
                        gs.Start();

                        //
                        GFLauncher.OnUpdate = gs.Update;
                        GFLauncher.OnLateUpdate = gs.LateUpdate;
                    }
                }
            }
        }

        #endregion

        #region 启动热更逻辑

        /// <summary>
        /// 初始化
        /// 修改版本,让这个启动逻辑由使用者自行处理
        /// </summary>
        /// <param name="GameId">单游戏更新启动不需要id，多游戏更新需要id号</param>
        public void Launch(string GameId = "")
        {
            //初始化资源加载
            string coderoot = "";
            string sqlroot = "";
            string artroot = "";

            //各自的路径
            //art
            if (ArtRoot == AssetLoadPath.Editor)
            {
                if (Application.isEditor)
                {
                    //默认不走AssetBundle
                    artroot = "";
                }
                else
                {
                    //手机默认直接读取Assetbundle
                    artroot = Application.persistentDataPath;
                }
            }
            else if (ArtRoot == AssetLoadPath.Persistent)
            {
                artroot = Application.persistentDataPath;
            }

            else if (ArtRoot == AssetLoadPath.StreamingAsset)
            {
                artroot = Application.streamingAssetsPath;
            }

            //sql
            if (SQLRoot == AssetLoadPath.Editor)
            {
                //sql 默认读streaming
                sqlroot = Application.streamingAssetsPath;
            }

            else if (SQLRoot == AssetLoadPath.Persistent)
            {
                sqlroot = Application.persistentDataPath;
            }
            else if (SQLRoot == AssetLoadPath.StreamingAsset)
            {
                sqlroot = Application.streamingAssetsPath;
            }

            //code
            if (CodeRoot == AssetLoadPath.Editor)
            {
                //sql 默认读streaming
                coderoot = "";
            }

            else if (CodeRoot == AssetLoadPath.Persistent)
            {
                coderoot = Application.persistentDataPath;
            }
            else if (CodeRoot == AssetLoadPath.StreamingAsset)
            {
                coderoot = Application.streamingAssetsPath;
            }

            //多游戏更新逻辑
            if (Application.isEditor == false)
            {
                if (GameId != "")
                {
                    artroot = artroot + "/" + GameId;
                    coderoot = coderoot + "/" + GameId;
                    sqlroot = sqlroot + "/" + GameId;
                }
            }

            //sql
            SqliteLoder.Load(sqlroot);
            //art
            CResources.Load(artroot);
            //code
            LoadScrpit(coderoot);
        }

        /// <summary>
        /// 开始热更脚本逻辑
        /// </summary>
        private void LoadScrpit(string root)
        {
            if (root != "") //热更代码模式
            {
                ILRuntimeHelper.LoadHotfix(root);
                ILRuntimeHelper.AppDomain.Invoke("GFLauncherBridge", "Start", null,
                    new object[] { true });
            }
            else
            {
                //这里用反射是为了 不访问逻辑模块的具体类，防止编译失败
                var assembly = Assembly.GetExecutingAssembly();
                var type = assembly.GetType("GFLauncherBridge");
                var method = type.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);
                method.Invoke(null, new object[] { false });
            }
        }

        #endregion

        //是否ILR模式
        public bool IsCodeHotfix
        {
            get
            {
                if (CodeRoot != AssetLoadPath.Editor)
                {
                    return true;
                }
                return false;
            }
        }

        //普通帧循环
        private void Update()
        {
            if (OnUpdate != null)
            {
                OnUpdate();
            }
        }

        //更快的帧循环
        private void LateUpdate()
        {
            if (OnLateUpdate != null)
            {
                OnLateUpdate();
            }
        }

        //固定帧更新
        private void FixedUpdate()
        {
            if (OnFixUpdate != null)
                OnFixUpdate();
        }

        void OnApplicationQuit()
        {
#if UNITY_EDITOR
            GFFramework.Sql.SqliteHelper.DB.Close();
            ILRuntimeHelper.Close();
#endif
        }
    }
}