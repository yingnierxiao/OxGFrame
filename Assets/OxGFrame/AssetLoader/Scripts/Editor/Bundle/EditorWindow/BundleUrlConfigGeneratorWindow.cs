﻿using OxGFrame.AssetLoader.Bundle;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    public class BundleUrlConfigGeneratorWindow : EditorWindow
    {
        private static BundleUrlConfigGeneratorWindow _instance = null;
        internal static BundleUrlConfigGeneratorWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<BundleUrlConfigGeneratorWindow>();
            return _instance;
        }

        [SerializeField]
        public string bundleIp;
        [SerializeField]
        public string bundleFallbackIp;
        [SerializeField]
        public string storeLink;
        [SerializeField]
        public bool autoReveal;

        internal static string PROJECT_PATH;
        internal static string KEY_SAVER;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(BundleHelper.MenuRoot + "Bundle Url Config Generator (" + BundleConfig.bundleUrlFileName + ")", false, 899)]
        public static void ShowWindow()
        {
            PROJECT_PATH = Application.dataPath;
            KEY_SAVER = $"{PROJECT_PATH}_{nameof(BundleUrlConfigGeneratorWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Bundle Url Config Generator");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this.bundleIp = EditorStorage.GetData(KEY_SAVER, "bundleIp", "127.0.0.1");
            this.bundleFallbackIp = EditorStorage.GetData(KEY_SAVER, "bundleFallbackIp", "127.0.0.1");
            this.storeLink = EditorStorage.GetData(KEY_SAVER, "storeLink", "http://");

            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVER, "autoReveal", "true"));
        }

        private void OnGUI()
        {
            // operation type area
            EditorGUI.BeginChangeCheck();

            this._DrawExportBundleUrlConfigToStreamingAssetsView();
        }

        private void _DrawExportBundleUrlConfigToStreamingAssetsView()
        {
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();
            var bg = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString("#1c589c", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
            bg.SetPixels(pixels);
            bg.Apply();
            style.normal.background = bg;
            EditorGUILayout.BeginVertical(style);
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Export BundleUrlConfig To StreamingAssets"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawBundleIPTextFieldView();
            this._DrawBundleFallbackIPTextFieldView();
            this._DrawStoreLinkTextFieldView();
            this._DrawProcessButtonView();

            EditorGUILayout.EndVertical();
        }

        private void _DrawBundleIPTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.bundleIp = EditorGUILayout.TextField("Bundle IP", this.bundleIp);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "bundleIp", this.bundleIp);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawBundleFallbackIPTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.bundleFallbackIp = EditorGUILayout.TextField("Bundle Fallback IP", this.bundleFallbackIp);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "bundleFallbackIp", this.bundleFallbackIp);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawStoreLinkTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.storeLink = EditorGUILayout.TextField("Store Link", this.storeLink);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "storeLink", this.storeLink);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawProcessButtonView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            // auto reveal toggle
            this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked after process will reveal destination folder."));
            EditorStorage.SaveData(KEY_SAVER, "autoReveal", this.autoReveal.ToString());

            // process button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Process", GUILayout.MaxWidth(100f)))
            {
                string outputPath = Application.streamingAssetsPath;
                BundleHelper.ExportBundleUrlConfig(this.bundleIp, this.bundleFallbackIp, this.storeLink, outputPath);
                EditorUtility.DisplayDialog("Process Message", "Export BundleUrlConfig To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{BundleConfig.bundleUrlFileName}");
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }
    }
}