using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    [Serializable]
    internal sealed class EditorToolboxGlobalSettings
    {
        internal const int DefaultNotifyMinSeconds = 30;
        internal const string CreatedSessionKey = "net.maaaaa.editor-toolbox.GlobalSettingsCreated";
        internal const string DiscordWebhookTemplate = "{\n  \"username\": \"Poyo's Editor Toolbox\",\n  \"embeds\": [\n    {\n      \"title\": \"{title}\",\n      \"description\": \"{message}\",\n      \"color\": {color},\n      \"footer\": { \"text\": \"{project} / {time}\" }\n    }\n  ]\n}";

        public int schemaVersion = 1;
        public List<EditorToolboxToggleValue> toggles = new List<EditorToolboxToggleValue>();
        public int notifyMinSeconds = DefaultNotifyMinSeconds;
        public string successSoundPath = string.Empty;
        public string failureSoundPath = string.Empty;
        public string webhookUrl = string.Empty;
        public string webhookTemplate = DiscordWebhookTemplate;

        private static EditorToolboxGlobalSettings _instance;
        internal static EditorToolboxGlobalSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        internal static void Reload()
        {
            _instance = null;
            EditorToolboxToggles.Invalidate();
        }

        private static EditorToolboxGlobalSettings Load()
        {
            bool hasNew = EditorPrefs.HasKey(EditorToolboxInfo.GlobalSettingsKey);
            bool hasLegacy = EditorPrefs.HasKey(EditorToolboxLegacy.GlobalSettingsKey);
            if (!hasNew && !hasLegacy)
            {
                EditorToolboxGlobalSettings initial = CreateInitial();
                if (!EditorToolboxLegacy.IsInstalled)
                {
                    initial.Save();
                    SessionState.SetBool(CreatedSessionKey, true);
                }
                return initial;
            }

            try
            {
                EditorToolboxGlobalSettings settings = Parse(EditorPrefs.GetString(hasNew
                    ? EditorToolboxInfo.GlobalSettingsKey : EditorToolboxLegacy.GlobalSettingsKey));
                if (!hasNew)
                {
                    settings.webhookTemplate = EditorToolboxLegacy.ReplaceLegacyTemplate(settings.webhookTemplate);
                    // NOTE: ほかのプロジェクトが旧名のパッケージを使う可能性があるため、旧キーは残す。
                    if (!EditorToolboxLegacy.IsInstalled)
                    {
                        settings.Save();
                        Debug.Log(EditorToolboxInfo.LogPrefix + "Marshmallow Jar の全体設定を引き継ぎました。");
                    }
                }
                return settings;
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning(EditorToolboxInfo.LogPrefix + "全体設定を読み込めませんでした。今回は初期値を使います。設定を変えると上書き保存されます。詳細: " + exception.Message);
                return CreateInitial();
            }
        }

        private static EditorToolboxGlobalSettings Parse(string json)
        {
            EditorToolboxGlobalSettings settings = JsonUtility.FromJson<EditorToolboxGlobalSettings>(json);
            if (settings == null)
            {
                throw new ArgumentException("設定が空です。");
            }
            if (settings.toggles == null)
            {
                settings.toggles = new List<EditorToolboxToggleValue>();
            }
            settings.notifyMinSeconds = Mathf.Clamp(settings.notifyMinSeconds, 0, 600);
            return settings;
        }

        private static EditorToolboxGlobalSettings CreateInitial()
        {
            var settings = new EditorToolboxGlobalSettings();
            // NOTE: 後の版で初期値を変えても、既にある全体設定の値を変えないため全項目を保存する。
            foreach (EditorToolboxToggle toggle in EditorToolboxToggles.All)
            {
                settings.SetToggle(toggle, EditorToolboxToggles.GetInitialValue(toggle));
            }
            return settings;
        }

        internal void Save()
        {
            // NOTE: 併存中の保存は、旧版を削除したあとの引き継ぎを妨げる。
            if (EditorToolboxLegacy.IsInstalled)
            {
                return;
            }
            // NOTE: 全プロジェクトで共有され、プロジェクトにファイルを置かずに済むため EditorPrefs に保存する。
            EditorPrefs.SetString(EditorToolboxInfo.GlobalSettingsKey, JsonUtility.ToJson(this));
            EditorToolboxToggles.Invalidate();
        }

        internal bool TryGetToggle(EditorToolboxToggle toggle, out bool value)
        {
            string key = toggle.ToString();
            foreach (EditorToolboxToggleValue entry in toggles)
            {
                if (entry != null && entry.key == key)
                {
                    value = entry.value;
                    return true;
                }
            }
            value = false;
            return false;
        }

        internal void SetToggle(EditorToolboxToggle toggle, bool value)
        {
            string key = toggle.ToString();
            foreach (EditorToolboxToggleValue entry in toggles)
            {
                if (entry != null && entry.key == key)
                {
                    entry.value = value;
                    return;
                }
            }
            toggles.Add(new EditorToolboxToggleValue { key = key, value = value });
        }
    }
}
