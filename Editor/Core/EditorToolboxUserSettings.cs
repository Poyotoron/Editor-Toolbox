using UnityEngine;
using UnityEditor;

namespace Poyo.EditorToolbox.Editor
{
    // NOTE: UserSettings はプロジェクトごと・利用者ごとの保存先で、Unity の標準の .gitignore でリポジトリから外れる。
    //       秘密の Webhook URL と PC ごとに違う音のパスを置くため、ProjectSettings には保存しない。
    [FilePath(EditorToolboxUserSettings.SettingsFilePath, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class EditorToolboxUserSettings : ScriptableSingleton<EditorToolboxUserSettings>
    {
        internal const string SettingsFilePath = "UserSettings/EditorToolbox.asset";
        private static readonly string[] LegacyPaths = { EditorToolboxLegacy.UserSettingsPath };
        private static bool _migrated;

        // NOTE: 引き継ぎより先に空の設定を読み込まないよう、入口を一本化する。
        internal static EditorToolboxUserSettings Instance
        {
            get
            {
                if (!_migrated)
                {
                    _migrated = EditorToolboxLegacy.TryCopySettingsFile(SettingsFilePath, LegacyPaths,
                        "Packages/net.maaaaa.editor-toolbox/Editor/Core/EditorToolboxUserSettings.cs", out string copiedFrom);
                    if (copiedFrom != null)
                    {
                        Debug.Log(EditorToolboxInfo.LogPrefix + "Marshmallow Jar の設定を引き継ぎました: " + copiedFrom + " → " + SettingsFilePath);
                        string template = EditorToolboxLegacy.ReplaceLegacyTemplate(instance.webhookTemplate);
                        if (template != instance.webhookTemplate)
                        {
                            instance.webhookTemplate = template;
                            instance.SaveChanges();
                        }
                    }
                }
                return instance;
            }
        }

        public int schemaVersion = 1;
        public bool overrideNotifyMinSeconds = false;
        public int notifyMinSeconds = EditorToolboxGlobalSettings.DefaultNotifyMinSeconds;
        public bool overrideSuccessSoundPath = false;
        public string successSoundPath = string.Empty;
        public bool overrideFailureSoundPath = false;
        public string failureSoundPath = string.Empty;
        public bool overrideWebhookUrl = false;
        public string webhookUrl = string.Empty;
        public bool overrideWebhookTemplate = false;
        public string webhookTemplate = EditorToolboxGlobalSettings.DiscordWebhookTemplate;

        internal void SaveChanges()
        {
            // NOTE: 併存中に保存すると、旧版を削除したあとの引き継ぎを妨げる。
            if (EditorToolboxLegacy.IsInstalled)
            {
                return;
            }
            Save(true);
        }
    }
}
