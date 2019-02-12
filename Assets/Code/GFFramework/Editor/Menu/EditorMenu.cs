using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GFFramework.Helper;
using GFFramework.Editor.UI;

namespace GFFramework.Editor
{ 
    public class GFEditorMenu
    {
        [MenuItem("Tools/初始化目录", false, 1)]
        public static void GFInit()
        {
           GFFrameInit.Init();
        }

        [MenuItem("Tools/1.DLL打包", false, 52)]
        public static void ExecuteBuildDLL()
        {
            var window =
                (EditorWindow_ScriptBuildDll) EditorWindow.GetWindow(typeof(EditorWindow_ScriptBuildDll), false, "DLL打包工具");
            window.Show();
        }

        [MenuItem("Tools/2.AssetBundle打包", false, 53)]
        public static void ExecuteAssetBundle()
        {
            var window =
                (EditorWindow_GenAssetBundle) EditorWindow.GetWindow(typeof(EditorWindow_GenAssetBundle), false,
                    "AB打包工具");
            window.Show();
        }

        [MenuItem("Tools/3.表格/表格->生成Class", false, 54)]
        public static void ExecuteGenTableCalss()
        {
            Excel2Code.GenCode();
        }

        [MenuItem("Tools/3.表格/表格->生成SQLite", false, 55)]
        public static void ExecuteGenTable()
        {
            Excel2SQLiteTools.GenSQLite(IPath.Combine(Application.streamingAssetsPath,Utils.GetPlatformPath(Application.platform)));
            Debug.Log("表格导出完毕");
        }
        
        [MenuItem("Tools/3.表格/json->生成SQLite", false, 58)]
        public static void ExecuteJsonToSqlite()
        {
            Excel2SQLiteTools.GenJsonToSQLite(IPath.Combine(Application.streamingAssetsPath,Utils.GetPlatformPath(Application.platform)));
            Debug.Log("表格导出完毕");
        }

        //[MenuItem("Tools/资源压缩/图片压缩", false,56)]
        //public static void ChangeTexture()
        //{
        //    var window =(Editor_2ChangeTextureImporter)EditorWindow.GetWindow(typeof(Editor_2ChangeTextureImporter), false, "图片格式设置");
        //    window.Show();
        //}
        [MenuItem("Tools/资源一键打包", false, 101)]
        public static void GenResouceall()
        {
            var window =(EditorWindow_OnkeyBuildAsset)EditorWindow.GetWindow(typeof(EditorWindow_OnkeyBuildAsset), false, "一键打包");
            window.Show();
        }
      
    }
}