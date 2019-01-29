using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GF.Debug;
using GFFramework.Helper;
using GFFramework.Http;
using ILRuntime.Runtime.Intepreter;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Networking;

namespace GFFramework
{
    public class AssetConfig
    {
        public string Platfrom = "";
        public double Version = 0.1d;
        public List<AssetItem> Assets = new List<AssetItem>();
    }

    public class AssetItem
    {
        public string HashName = "";
        public string LocalPath = "";
    }

    /// <summary>
    /// 版本控制类
    /// </summary>
    static public class VersionContorller
    {
        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="serverConfigPath">服务器配置根目录</param>
        /// <param name="localConfigPath">本地根目录</param>
        /// <param name="onProcess"></param>
        /// <param name="onError"></param>
        /// 返回码: -1：error  0：success
        static async public Task<int> Start(string serverConfigPath, string localConfigPath, Action<int, int> onProcess, Action<string> onError, Action OnDownloadFinished = null)
        {

            IPath.Combine("", "");
            var client = HttpMgr.Inst.GetFreeHttpClient();
            var platform = Utils.GetPlatformPath(Application.platform);
            //开始下载服务器配置
            var serverPath = serverConfigPath + "/" + platform + "_Server/" + platform + "_VersionConfig.json";
            Debug.Log("server:" + serverPath);
            string serverConfig = "";
            try
            {
                serverConfig = await client.DownloadStringTaskAsync(serverPath);
                UberDebug.Log("服务器资源配置:" + serverConfig);
            }
            catch (Exception e)
            {
                onError(e.Message);
                return -1;
            }

            var serverconf = LitJson.JsonMapper.ToObject<AssetConfig>(serverConfig);
            AssetConfig localconf = new AssetConfig();
            var localPath = localConfigPath + "/" + platform + "_Server/" + platform + "_VersionConfig.json";
            UberDebug.Log("localConfigPath:" + localPath);

            if (File.Exists(localPath))
            {
                localconf = LitJson.JsonMapper.ToObject<AssetConfig>(File.ReadAllText(localPath));
            }
            else
            {
                UberDebug.LogError("Can't find path：" + localPath);
                localconf.Version = 0;
                localconf.Platfrom = Utils.GetPlatformPath(Application.platform);
            }

            UberDebug.Log(string.Format("<color=red>serverconf.Version:{0} localconf.Version:{1} </color>", serverconf.Version, localconf.Version));
            if (serverconf.Version > localconf.Version)
            {
                //对比差异列表进行下载
                var list = CompareConfig(localConfigPath, localconf, serverconf);
                if (list.Count > 0)
                {
                    //预通知要进入热更模式
                    onProcess(0, list.Count);
                }

                int count = 0;
                foreach (var item in list)
                {
                    count++;
                    var sp = serverConfigPath + "/" + platform + "_Server/" + item.HashName;
                    var lp = localConfigPath + "/" + platform + "/" + item.LocalPath;

                    //创建目录
                    var direct = Path.GetDirectoryName(lp);
                    if (Directory.Exists(direct) == false)
                    {
                        Directory.CreateDirectory(direct);
                    }

                    //下载
                    try
                    {
                        await client.DownloadFileTaskAsync(sp, lp);
                    }
                    catch (Exception e)
                    {
                        UberDebug.LogError(sp);
                        onError(e.Message);
                        return -1;
                    }

                    UberDebug.Log(string.Format("下载成功：{0}", sp));
                    onProcess(count, list.Count);
                }

                //写到本地
                if (list.Count > 0)
                {
                    File.WriteAllText(localPath, serverConfig);
                    UberDebug.Log(string.Format("Write Config to local,localPath:{0}", localPath));
                    if (OnDownloadFinished != null)
                        OnDownloadFinished();
                }
                else
                {
                    UberDebug.Log("Need't update file,Will enter game...");
                    if (OnDownloadFinished != null)
                        OnDownloadFinished();
                }
            }
            else
            {
                //版本号相同,不需要对比更新
                UberDebug.Log("Need't update file,Will enter game...");
                if (OnDownloadFinished != null)
                    OnDownloadFinished();
            }

            client.Dispose();
            return 0;
        }

        static public IEnumerator IEStart(string serverConfigPath, string localConfigPath, Action<int, int> onProcess, Action<string> onError, Action OnDownloadFinished = null)
        {
            IPath.Combine("", "");
            var client = HttpMgr.Inst.GetFreeHttpClient();
            var platform = Utils.GetPlatformPath(Application.platform);
            //开始下载服务器配置
            var serverPath = serverConfigPath + "/" + platform + "_Server/" + platform + "_VersionConfig.json";
            Debug.Log("server:" + serverPath);
            string serverConfig = "";
            //下载config
            {
                var wr = UnityWebRequest.Get(serverPath);
                yield return wr.SendWebRequest();
                if (wr.error == null)
                {
                    serverConfig = wr.downloadHandler.text;
                    UberDebug.Log("服务器资源配置:" + serverConfig);
                }
                else
                {
                    UberDebug.LogError(wr.error);
                }

            }

            var serverconf = LitJson.JsonMapper.ToObject<AssetConfig>(serverConfig);
            AssetConfig localconf = null;
            var localPath = localConfigPath + "/" + platform + "_Server/" + platform + "_VersionConfig.json";

            if (File.Exists(localPath))
            {
                localconf = LitJson.JsonMapper.ToObject<AssetConfig>(File.ReadAllText(localPath));
            }

            //对比差异列表进行下载
            var list = CompareConfig(localConfigPath, localconf, serverconf);
            if (list.Count > 0)
            {
                //预通知要进入热更模式
                onProcess(0, list.Count);
            }

            int count = 0;
            foreach (var item in list)
            {
                count++;
                var sp = serverConfigPath + "/" + platform + "_Server/" + item.HashName;
                var lp = localConfigPath + "/" + platform + "/" + item.LocalPath;

                //创建目录
                var direct = Path.GetDirectoryName(lp);
                if (Directory.Exists(direct) == false)
                {
                    Directory.CreateDirectory(direct);
                }

                //下载
                var wr = UnityWebRequest.Get(sp);
                yield return wr.SendWebRequest();
                if (wr.error == null)
                {
                    File.WriteAllBytes(lp, wr.downloadHandler.data);
                    UberDebug.Log("下载成功：" + sp);
                    onProcess(count, list.Count);
                }
                else
                {
                    UberDebug.LogError("下载失败:" + wr.error);
                    onError(wr.error);
                }
            }

            //写到本地
            //写到本地
            if (list.Count > 0)
            {
                File.WriteAllText(localPath, serverConfig);
                UberDebug.Log(string.Format("Write Config to local,localPath:{0}", localPath));
                if (OnDownloadFinished != null)
                    OnDownloadFinished();
            }
            else
            {
                UberDebug.Log("Need't update file,Will enter game...");
                if (OnDownloadFinished != null)
                    OnDownloadFinished();
            }
        }


        /// <summary>
        /// 对比
        /// </summary>
        static public List<AssetItem> CompareConfig(string root, AssetConfig local, AssetConfig server)
        {
            if (local == null)
            {
                return server.Assets;
            }

            var list = new List<AssetItem>();
            //比对平台
            if (local.Platfrom == server.Platfrom)
            {
                foreach (var serverAsset in server.Assets)
                {
                    //比较本地是否有 hash、文件名一致的资源
                    var result = local.Assets.Find((a) => a.HashName == serverAsset.HashName && a.LocalPath == serverAsset.LocalPath);

                    if (result == null)
                    {
                        list.Add(serverAsset);
                    }
                }
            }
            else
            {
                UberDebug.LogError("Platform not same!");
            }

            return list;
        }
    }
}