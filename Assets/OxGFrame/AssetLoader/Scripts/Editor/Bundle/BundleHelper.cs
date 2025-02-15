﻿using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using OxGFrame.AssetLoader.Utility;
using YooAsset.Editor;
using YooAsset;
using System.Linq;
using System;

namespace OxGFrame.AssetLoader.Editor
{
    public static class BundleHelper
    {
        internal const string MenuRoot = "OxGFrame/AssetLoader/";

        #region Public Methods
        #region Exporter
        /// <summary>
        /// 輸出 App 配置檔至輸出路徑 (Export AppConfig to StreamingAssets [for Built-in])
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="compressed"></param>
        public static void ExportAppConfig(string productName, string appVersion, string outputPath, bool activeBuildTarget, BuildTarget buildTarget)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            // 生成配置檔數據
            var cfg = GenerateAppConfig(productName, appVersion, activeBuildTarget, buildTarget);

            // 配置檔序列化, 將進行寫入
            string jsonCfg = JsonConvert.SerializeObject(cfg, Formatting.Indented);

            // 寫入配置文件
            string writePath = Path.Combine(outputPath, BundleConfig.appCfgName + BundleConfig.appCfgExtension);
            WriteTxt(jsonCfg, writePath);

            Debug.Log($"<color=#00FF00>【Export {BundleConfig.appCfgName}{BundleConfig.appCfgExtension} Completes】App Version: {cfg.APP_VERSION}</color>");
        }

        /// <summary>
        /// 輸出 Configs 跟 App Bundles (Export Configs and App Bundles for CDN Server)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="productName"></param>
        /// <param name="appVersion"></param>
        /// <param name="groupInfos"></param>
        /// <param name="exportPackages"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        /// <param name="isClearOutputPath"></param>
        public static void ExportConfigsAndAppBundles(string inputPath, string outputPath, string productName, string appVersion, List<GroupInfo> groupInfos, string[] exportPackages, bool activeBuildTarget, BuildTarget buildTarget, bool isClearOutputPath = true)
        {
            // 生成配置檔數據 (AppConfig)
            var appCfg = GenerateAppConfig(productName, appVersion, activeBuildTarget, buildTarget);
            var patchCfg = GeneratePatchConfig(groupInfos, exportPackages, inputPath);

            // 清空輸出路徑
            if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            #region YooAsset Bundle
            ExportNewestYooAssetBundles(inputPath, outputPath, appCfg.PLATFORM, productName, appVersion, exportPackages);
            #endregion

            #region AppConfig Write
            // 配置檔序列化, 將進行寫入
            string jsonCfg = JsonConvert.SerializeObject(appCfg, Formatting.Indented);

            // 寫入配置文件
            string writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}", BundleConfig.appCfgName + BundleConfig.appCfgExtension);
            WriteTxt(jsonCfg, writePath);

            // 寫入配置文件 (BAK)
            writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}" + $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}", BundleConfig.appCfgName + BundleConfig.appCfgBakExtension);
            WriteTxt(jsonCfg, writePath);
            #endregion

            #region PatchConfig Write
            // 配置檔序列化, 將進行寫入
            jsonCfg = JsonConvert.SerializeObject(patchCfg, Formatting.Indented);

            // 寫入配置文件
            writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}", BundleConfig.patchCfgName + BundleConfig.patchCfgExtension);
            WriteTxt(jsonCfg, writePath);

            // 寫入配置文件 (BAK)
            writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}" + $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}", BundleConfig.patchCfgName + BundleConfig.patchCfgBakExtension);
            WriteTxt(jsonCfg, writePath);
            #endregion

            Debug.Log($"<color=#00FF00>【Export Configs And App Bundles Completes】 App Version: {appCfg.APP_VERSION}</color>");
        }

        /// <summary>
        /// 輸出 App Bundles (Export App Bundles Without Configs for CDN Server)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="productName"></param>
        /// <param name="appVersion"></param>
        /// <param name="exportPackages"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        /// <param name="isClearOutputPath"></param>
        public static void ExportAppBundles(string inputPath, string outputPath, string productName, string appVersion, string[] exportPackages, bool activeBuildTarget, BuildTarget buildTarget, bool isClearOutputPath = true)
        {
            // 生成配置檔數據 (AppConfig)
            var appCfg = GenerateAppConfig(productName, appVersion, activeBuildTarget, buildTarget);

            // 清空輸出路徑
            if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            #region YooAsset Bundle
            ExportNewestYooAssetBundles(inputPath, outputPath, appCfg.PLATFORM, productName, appVersion, exportPackages);
            #endregion

            Debug.Log($"<color=#00FF00>【Export App Bundles Without Configs Completes】</color>");
        }

        /// <summary>
        /// 輸出 DLC Bundles (Export Individual DLC Bundles for CDN Server)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="productName"></param>
        /// <param name="dlcInfos"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        /// <param name="isClearOutputPath"></param>
        public static void ExportIndividualDlcBundles(string inputPath, string outputPath, string productName, List<DlcInfo> dlcInfos, bool activeBuildTarget, BuildTarget buildTarget, bool isClearOutputPath = true)
        {
            // 平台
            string platform = activeBuildTarget ? $"{EditorUserBuildSettings.activeBuildTarget}" : $"{buildTarget}";

            // 清空輸出路徑
            if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            #region YooAsset Bundle
            ExportNewestYooAssetDlcBundles(inputPath, outputPath, platform, productName, dlcInfos);
            #endregion

            Debug.Log($"<color=#00FF00>【Export DLC Bundles Completes】</color>");
        }

        /// <summary>
        /// 產生 Bundle URL 配置檔至輸出路徑 (Export BundleUrlConfig to StreamingAssets [for Built-in])
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="appVersion"></param>
        /// <param name="outputPath"></param>
        public static void ExportBundleUrlConfig(string bundleIp, string bundleFallbackIp, string storeLink, string outputPath)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            if (string.IsNullOrEmpty(bundleIp)) bundleIp = "127.0.0.1";
            if (string.IsNullOrEmpty(bundleFallbackIp)) bundleFallbackIp = "127.0.0.1";
            if (string.IsNullOrEmpty(storeLink)) storeLink = "http://";

            IEnumerable<string> contents = new string[]
            {
            @$"# {BundleConfig.BUNDLE_IP} = First CDN Server IP (Plan A)",
            @$"# {BundleConfig.BUNDLE_FALLBACK_IP} = Second CDN Server IP (Plan B)",
            @$"# {BundleConfig.STORE_LINK} = GooglePlay Store Link (https://play.google.com/store/apps/details?id=YOUR_ID) or Apple Store Link (itms-apps://itunes.apple.com/app/idYOUR_ID)",
            "",
            $"{BundleConfig.BUNDLE_IP} {bundleIp}",
            $"{BundleConfig.BUNDLE_FALLBACK_IP} {bundleFallbackIp}",
            $"{BundleConfig.STORE_LINK} {storeLink}",
            };

            string fullOutputPath = Path.Combine(outputPath, BundleConfig.bundleUrlFileName);

            // 寫入配置文件
            File.WriteAllLines(fullOutputPath, contents, System.Text.Encoding.UTF8);

            Debug.Log($"<color=#00FF00>【Export {BundleConfig.bundleUrlFileName} Completes】</color>");
        }
        #endregion

        #region Parser and Converter
        /// <summary>
        /// Parsing group info args (g1,t1#g2,t1,t2...)
        /// </summary>
        /// <param name="groupInfoArgs"></param>
        /// <returns></returns>
        public static List<GroupInfo> ParsingGroupInfosByArgs(string groupInfoArgs)
        {
            if (string.IsNullOrEmpty(groupInfoArgs)) return new List<GroupInfo>();

            // Parsing GroupInfo Arguments
            List<GroupInfo> parsedGroupInfos = new List<GroupInfo>();

            try
            {
                string[] groups = groupInfoArgs.Trim().Split('#');
                foreach (string group in groups)
                {
                    GroupInfo groupInfo = new GroupInfo();

                    string[] args = group.Trim().Split(',');
                    List<string> tags = new List<string>();
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i == 0) groupInfo.groupName = args[i];
                        else tags.Add(args[i]);
                    }
                    groupInfo.tags = tags.ToArray();

                    parsedGroupInfos.Add(groupInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return new List<GroupInfo>();
            }

            return parsedGroupInfos;
        }

        /// <summary>
        /// Convert group info args to List<GroupInfo>
        /// </summary>
        /// <param name="groupInfos"></param>
        /// <returns></returns>
        public static string ConvertGroupInfosToArgs(List<GroupInfo> groupInfos)
        {
            string args = string.Empty;
            for (int i = 0; i < groupInfos.Count; i++)
            {
                string groupName = groupInfos[i].groupName;
                string tags = string.Empty;
                foreach (var tag in groupInfos[i].tags)
                {
                    tags += $",{tag}";
                }
                args += $"{groupName}{tags}" + ((i == groupInfos.Count - 1) ? string.Empty : "#");
            }

            return args;
        }

        /// <summary>
        /// Parsing dlc info args (dlc1,version#dlc2,version...)
        /// </summary>
        /// <param name="dlcInfoArgs"></param>
        /// <returns></returns>
        public static List<DlcInfo> ParsingDlcInfosByArgs(string dlcInfoArgs)
        {
            if (string.IsNullOrEmpty(dlcInfoArgs)) return new List<DlcInfo>();

            // Parsing DlcInfo Arguments
            List<DlcInfo> parsedDlcInfos = new List<DlcInfo>();

            try
            {
                string[] dlcs = dlcInfoArgs.Trim().Split('#');
                foreach (string dlc in dlcs)
                {
                    DlcInfo dlcInfo = new DlcInfo();

                    string[] args = dlc.Trim().Split(',');
                    dlcInfo.packageName = args[0];
                    dlcInfo.dlcVersion = args[1];

                    parsedDlcInfos.Add(dlcInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return new List<DlcInfo>();
            }

            return parsedDlcInfos;
        }

        /// <summary>
        /// Convert dlc info args to List<DlcInfo>
        /// </summary>
        /// <param name="dlcInfos"></param>
        /// <returns></returns>
        public static string ConvertDlcInfosToArgs(List<DlcInfo> dlcInfos)
        {
            string args = string.Empty;
            for (int i = 0; i < dlcInfos.Count; i++)
            {
                string packageName = dlcInfos[i].packageName;
                string version = dlcInfos[i].dlcVersion;
                args += $"{packageName},{version}" + ((i == dlcInfos.Count - 1) ? string.Empty : "#");
            }

            return args;
        }
        #endregion
        #endregion

        #region Internal Methods
        /// <summary>
        /// 輸出最新的 YooAsset Bundles (找尋輸出時間最大值)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="platform"></param>
        /// <param name="productName"></param>
        /// <param name="appVersion"></param>
        internal static void ExportNewestYooAssetBundles(string inputPath, string outputPath, string platform, string productName, string appVersion, string[] exportPackages)
        {
            #region YooAsset Bundle Process
            // 取得 Bundle 輸出路徑
            if (string.IsNullOrEmpty(inputPath)) inputPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

            // 取得平台路徑
            string inputPathWithPlatform = Path.Combine(inputPath, $"{EditorUserBuildSettings.activeBuildTarget}");

            // 取得平台下的所有 Packages
            string[] packagePaths = Directory.GetDirectories(inputPathWithPlatform);

            // 進行資料夾之間的複製
            foreach (var packagePath in packagePaths)
            {
                bool isSkip = true;
                string packageName = Path.GetFileNameWithoutExtension(packagePath);
                foreach (var exportPackage in exportPackages)
                {
                    if (packageName == exportPackage)
                    {
                        isSkip = false;
                        break;
                    }
                }

                if (isSkip) continue;

                // 取得 NewestPackagePath
                string newestVersionPath = NewestPackagePathFilter(packagePath);

                string destFullDir = Path.GetFullPath(outputPath + $@"/{productName}" + $@"/{platform}" + $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}" + $@"/{packageName}");
                BundleUtility.CopyFolderRecursively(newestVersionPath, destFullDir);

                {
                    string versionName = Path.GetFileNameWithoutExtension(newestVersionPath);
                    Debug.Log($"<color=#00FF00>【Copy Bundles】 Package Name: {packageName}, Package Version: {versionName}</color>");
                }
            }
            #endregion
        }

        /// <summary>
        /// 輸出最新的 YooAsset DLC Bundles (找尋輸出時間最大值)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="platform"></param>
        /// <param name="productName"></param>
        /// <param name="dlcVersion"></param>
        internal static void ExportNewestYooAssetDlcBundles(string inputPath, string outputPath, string platform, string productName, List<DlcInfo> dlcInfos)
        {
            #region YooAsset Bundle Process
            // 取得 Bundle 輸出路徑
            if (string.IsNullOrEmpty(inputPath)) inputPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

            // 取得平台路徑
            string inputPathWithPlatform = Path.Combine(inputPath, $"{EditorUserBuildSettings.activeBuildTarget}");

            // 取得平台下的所有 Packages
            string[] packagePaths = Directory.GetDirectories(inputPathWithPlatform);

            // 進行資料夾之間的複製
            foreach (var packagePath in packagePaths)
            {
                bool isSkip = true;
                string packageName = Path.GetFileNameWithoutExtension(packagePath);
                string dlcVersion = null;
                foreach (var dlcInfo in dlcInfos)
                {
                    if (packageName == dlcInfo.packageName)
                    {
                        isSkip = false;
                        dlcVersion = dlcInfo.dlcVersion;
                        break;
                    }
                }

                if (isSkip) continue;

                string[] versionPaths = Directory.GetDirectories(packagePath);
                Dictionary<string, decimal> packageVersions = new Dictionary<string, decimal>();
                foreach (var versionPath in versionPaths)
                {
                    string versionName = Path.GetFileNameWithoutExtension(versionPath);

                    if (versionName.IndexOf('-') <= -1) continue;

                    string major = versionName.Substring(0, versionName.LastIndexOf("-"));
                    string minor = versionName.Substring(versionName.LastIndexOf("-") + 1, versionName.Length - versionName.LastIndexOf("-") - 1);

                    // yyyy-mm-dd
                    major = major.Trim().Replace("-", string.Empty);
                    // 24 h * 60 m = 1440 m (max is 4 num of digits)
                    minor = minor.Trim().PadLeft(4, '0');
                    //Debug.Log($"Major Date: {major}, Minor Minute: {minor} => {major}{minor}");

                    string refineVersionName = $"{major}{minor}";
                    if (decimal.TryParse(refineVersionName, out decimal value)) packageVersions.Add(versionPath, value);
                }

                string newestVersionPath = packageVersions.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                decimal newestVersion = packageVersions.Aggregate((x, y) => x.Value > y.Value ? x : y).Value;
                if (string.IsNullOrEmpty(dlcVersion)) dlcVersion = $"{newestVersion}";

                string destFullDir = Path.GetFullPath(outputPath + $@"/{productName}" + $@"/{platform}" + $@"/{BundleConfig.dlcFolderName}" + $@"/{packageName}" + $@"/{dlcVersion}");
                BundleUtility.CopyFolderRecursively(newestVersionPath, destFullDir);

                {
                    string versionName = Path.GetFileNameWithoutExtension(newestVersionPath);
                    Debug.Log($"<color=#00FF00>【Copy DLC Bundles】 Package Name: {packageName}, Package Version: {versionName}</color>");
                }
            }
            #endregion
        }

        /// <summary>
        /// 返回來源路徑的配置檔數據
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="inputPath"></param>
        /// <param name="compressed"></param>
        /// <returns></returns>
        internal static AppConfig GenerateAppConfig(string productName, string appVersion, bool activeBuildTarget, BuildTarget buildTarget)
        {
            // 生成配置檔
            var cfg = new AppConfig();

            // 平台
            cfg.PLATFORM = activeBuildTarget ? $"{EditorUserBuildSettings.activeBuildTarget}" : $"{buildTarget}";

            // 產品名稱
            cfg.PRODUCT_NAME = productName;

            // 主程式版本
            cfg.APP_VERSION = string.IsNullOrEmpty(appVersion) ? Application.version : appVersion;

            Debug.Log($"<color=#00FF00>【Generate】{BundleConfig.appCfgName}{BundleConfig.appCfgExtension} Completes.</color>");

            return cfg;
        }

        internal static PatchConfig GeneratePatchConfig(List<GroupInfo> groupInfos, string[] exportPackages, string inputPath = null)
        {
            // 生成配置檔
            var cfg = new PatchConfig();

            // 取得 Bundle 路徑
            if (string.IsNullOrEmpty(inputPath)) inputPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

            string inputPathWithPlatform = Path.Combine(inputPath, $"{EditorUserBuildSettings.activeBuildTarget}");

            string[] packagePaths = Directory.GetDirectories(inputPathWithPlatform);

            foreach (var packagePath in packagePaths)
            {
                bool isSkip = true;
                string packageName = Path.GetFileNameWithoutExtension(packagePath);
                foreach (var exportPackage in exportPackages)
                {
                    if (packageName == exportPackage)
                    {
                        isSkip = false;
                        break;
                    }
                }

                if (isSkip) continue;

                // 取得 NewestPackagePath
                string newestVersionPath = NewestPackagePathFilter(packagePath);

                // 建立 Package info
                Bundle.PackageInfo packageInfo = new Bundle.PackageInfo();
                // 生成 Package total size
                long packageSize = 0;

                FileInfo[] files = BundleUtility.GetFilesRecursively(newestVersionPath);
                foreach (var file in files)
                {
                    // 累加檔案大小
                    packageSize += file.Length;
                }

                {
                    string versionName = Path.GetFileNameWithoutExtension(newestVersionPath);
                    string refineVersionName = versionName.Replace("-", string.Empty);

                    packageInfo.packageName = packageName;
                    packageInfo.packageVersion = refineVersionName;
                    packageInfo.packageSize = packageSize;
                }

                // 資源包清單
                cfg.AddPackageInfo(packageInfo);
            }

            cfg.GROUP_INFOS = groupInfos;

            Debug.Log($"<color=#00FF00>【Generate】PatchConfig Completes.</color>");

            return cfg;
        }

        internal static string NewestPackagePathFilter(string packagePath)
        {
            #region Newest Filter
            string[] versionPaths = Directory.GetDirectories(packagePath);
            Dictionary<string, decimal> packageVersions = new Dictionary<string, decimal>();
            foreach (var versionPath in versionPaths)
            {
                string versionName = Path.GetFileNameWithoutExtension(versionPath);

                if (versionName.IndexOf('-') <= -1) continue;

                string major = versionName.Substring(0, versionName.LastIndexOf("-"));
                string minor = versionName.Substring(versionName.LastIndexOf("-") + 1, versionName.Length - versionName.LastIndexOf("-") - 1);

                // yyyy-mm-dd
                major = major.Trim().Replace("-", string.Empty);
                // 24 h * 60 m = 1440 m (max is 4 num of digits)
                minor = minor.Trim().PadLeft(4, '0');
                //Debug.Log($"Major Date: {major}, Minor Minute: {minor} => {major}{minor}");

                string refineVersionName = $"{major}{minor}";
                if (decimal.TryParse(refineVersionName, out decimal value)) packageVersions.Add(versionPath, value);
            }

            string newestVersionPath = packageVersions.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            #endregion

            return newestVersionPath;
        }

        /// <summary>
        /// 寫入文字文件檔
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="outputPath"></param>
        internal static void WriteTxt(string txt, string outputPath)
        {
            // 寫入配置文件
            var file = File.CreateText(outputPath);
            file.Write(txt);
            file.Close();
        }
        #endregion

        #region MenuItems
        [MenuItem(MenuRoot + "Local Download Directory (Sandbox)/Open Download Directory", false, 197)]
        internal static void OpenDownloadDir()
        {
            var dir = BundleConfig.GetLocalSandboxRootPath();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.Diagnostics.Process.Start(dir);
        }

        [MenuItem(MenuRoot + "Local Download Directory (Sandbox)/Clear Download Directory", false, 198)]
        internal static void ClearDownloadDir()
        {
            bool operate = EditorUtility.DisplayDialog(
                "Clear Download Folder",
                "Are you sure you want to delete download folder?",
                "yes",
                "no");

            if (!operate) return;

            var dir = BundleConfig.GetLocalSandboxRootPath();
            BundleUtility.DeleteFolder(dir);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        [MenuItem(MenuRoot + "Clear Last Group Info Record", false, 199)]
        internal static void ClearLastGroupRecord()
        {
            AssetPatcher.ClearLastGroupInfo();
        }
        #endregion
    }
}