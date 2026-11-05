using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    public static class EditorToolboxLegacy
    {
        public const string PackageId = "net.maaaaa.marshmallow-jar";
        internal const string GlobalSettingsKey = "net.maaaaa.marshmallow-jar.GlobalSettings";
        internal const string ProjectSettingsPath = "ProjectSettings/MarshmallowJar.asset";
        internal const string UserSettingsPath = "UserSettings/MarshmallowJar.asset";
        public const string ColorizerSettingsPath = "ProjectSettings/MarshmallowJar.IrodoriColorizer.asset";
        internal const string DiscordWebhookTemplate = "{\n  \"username\": \"Marshmallow Jar\",\n  \"embeds\": [\n    {\n      \"title\": \"{title}\",\n      \"description\": \"{message}\",\n      \"color\": {color},\n      \"footer\": { \"text\": \"{project} / {time}\" }\n    }\n  ]\n}";
        internal const string Warning = "Poyo's Editor Toolbox の旧名 Marshmallow Jar が入っているため、Poyo's Editor Toolbox の機能を止めています。VCC などのパッケージ管理ツールで Marshmallow Jar を削除してください。削除後に設定が自動で引き継がれます。";

        // NOTE: if の条件に使うため、到達できないコードの警告を避ける。
        public static readonly bool IsInstalled =
#if EDITOR_TOOLBOX_HAS_LEGACY
            true;
#else
            false;
#endif
        private static readonly Regex ScriptReferencePattern =
            new Regex(@"m_Script: \{fileID: 11500000, guid: [0-9a-f]{32}, type: 3\}");

        internal static string ReplaceLegacyTemplate(string template)
        {
            return string.Equals(template, DiscordWebhookTemplate, StringComparison.Ordinal)
                ? EditorToolboxGlobalSettings.DiscordWebhookTemplate : template;
        }

        public static bool TryCopySettingsFile(
            string newPath, string[] oldPaths, string scriptAssetPath, out string copiedFrom)
        {
            copiedFrom = null;
            if (IsInstalled || File.Exists(newPath))
            {
                return true;
            }
            try
            {
                foreach (string oldPath in oldPaths)
                {
                    if (!File.Exists(oldPath))
                    {
                        continue;
                    }
                    string guid = AssetDatabase.AssetPathToGUID(scriptAssetPath);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogWarning(EditorToolboxInfo.LogPrefix + "設定を引き継げませんでした。設定のスクリプトが見つかりません: " + scriptAssetPath);
                        return false;
                    }
                    string contents = File.ReadAllText(oldPath);
                    if (!ScriptReferencePattern.IsMatch(contents))
                    {
                        Debug.LogWarning(EditorToolboxInfo.LogPrefix + "設定ファイルの形式を読み取れなかったため、引き継ぎませんでした: " + oldPath);
                        continue;
                    }
                    // NOTE: 単体版の色付け設定はスクリプトの GUID が異なるため、同じ処理で両方を扱う。
                    string migrated = ScriptReferencePattern.Replace(contents,
                        "m_Script: {fileID: 11500000, guid: " + guid + ", type: 3}", 1);
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                    File.WriteAllText(newPath, migrated);
                    copiedFrom = oldPath;
                    return true;
                }
            }
            catch (IOException exception)
            {
                Debug.LogException(exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                Debug.LogException(exception);
            }
            return true;
        }

        internal static bool DrawWarningIfInstalled()
        {
            if (!IsInstalled)
            {
                return false;
            }
            EditorGUILayout.HelpBox(Warning, MessageType.Warning);
            return true;
        }

#if EDITOR_TOOLBOX_HAS_LEGACY
        [InitializeOnLoadMethod]
        private static void ShowNotice()
        {
            const string key = "EditorToolbox.LegacyNotice.Shown";
            if (Application.isBatchMode || SessionState.GetBool(key, false))
            {
                return;
            }
            SessionState.SetBool(key, true);
            Debug.LogWarning(EditorToolboxInfo.LogPrefix + Warning);
        }
#endif
    }
}
