using System;
using System.Collections.Generic;

namespace Poyo.EditorToolbox.Editor
{
    public static class EditorToolboxToggles
    {
        private static readonly EditorToolboxToggle[] AllToggles =
            (EditorToolboxToggle[])Enum.GetValues(typeof(EditorToolboxToggle));
        internal static IReadOnlyList<EditorToolboxToggle> All => AllToggles;
        private static bool[] _resolved;

        public static bool IsOn(EditorToolboxToggle toggle)
        {
            // NOTE: 旧名のパッケージと二重に動かないよう、併存中はすべて止める。
            if (EditorToolboxLegacy.IsInstalled)
            {
                return false;
            }
            if (_resolved == null)
            {
                // NOTE: Hierarchy などの全行コールバックから呼ばれるため、行ごとのリスト検索を避ける。
                var resolved = new bool[AllToggles.Length];
                foreach (EditorToolboxToggle item in AllToggles)
                {
                    resolved[(int)item] = TryGetOverride(item, out bool value) ? value : GetGlobal(item);
                }
                _resolved = resolved;
            }
            return _resolved[(int)toggle];
        }

        internal static bool GetInitialValue(EditorToolboxToggle toggle)
        {
            switch (toggle)
            {
                case EditorToolboxToggle.NotifyUploadSound:
                case EditorToolboxToggle.NotifyUploadWebhook:
                case EditorToolboxToggle.NotifyBatchUploadSound:
                case EditorToolboxToggle.NotifyBatchUploadWebhook:
                case EditorToolboxToggle.NotifyOutfitConversionSound:
                case EditorToolboxToggle.NotifyOutfitConversionWebhook:
                case EditorToolboxToggle.NotifyPlayModeSound:
                case EditorToolboxToggle.NotifyImportSound:
                    return true;
                case EditorToolboxToggle.HierarchyToggles:
                case EditorToolboxToggle.StripVariantSuffix:
                case EditorToolboxToggle.SceneSetup:
                case EditorToolboxToggle.VrcSdkAutoFill:
                case EditorToolboxToggle.InspectorNewWindowButton:
                case EditorToolboxToggle.InspectorCopyToButton:
                case EditorToolboxToggle.IrodoriColorizer:
                case EditorToolboxToggle.CompletionNotifications:
                case EditorToolboxToggle.NotifyPlayModeWebhook:
                case EditorToolboxToggle.NotifyImportWebhook:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(toggle));
            }
        }

        internal static bool GetGlobal(EditorToolboxToggle toggle)
        {
            return EditorToolboxGlobalSettings.Instance.TryGetToggle(toggle, out bool value)
                ? value : GetInitialValue(toggle);
        }

        internal static void SetGlobal(EditorToolboxToggle toggle, bool value)
        {
            EditorToolboxGlobalSettings.Instance.SetToggle(toggle, value);
            EditorToolboxGlobalSettings.Instance.Save();
        }

        private static bool TryGetOverride(EditorToolboxToggle toggle, out bool value)
        {
            string key = toggle.ToString();
            foreach (EditorToolboxToggleValue entry in EditorToolboxSettings.Instance.GetToggleOverrides())
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

        internal static bool HasOverride(EditorToolboxToggle toggle)
        {
            return TryGetOverride(toggle, out _);
        }

        internal static void SetOverride(EditorToolboxToggle toggle, bool value)
        {
            List<EditorToolboxToggleValue> entries = EditorToolboxSettings.Instance.GetToggleOverrides();
            string key = toggle.ToString();
            EditorToolboxToggleValue entry = entries.Find(item => item != null && item.key == key);
            if (entry == null)
            {
                entries.Add(new EditorToolboxToggleValue { key = key, value = value });
            }
            else
            {
                entry.value = value;
            }
            EditorToolboxSettings.Instance.SaveChanges();
            Invalidate();
        }

        internal static void ClearOverride(EditorToolboxToggle toggle)
        {
            string key = toggle.ToString();
            EditorToolboxSettings.Instance.GetToggleOverrides().RemoveAll(item => item != null && item.key == key);
            EditorToolboxSettings.Instance.SaveChanges();
            Invalidate();
        }

        internal static void Invalidate()
        {
            _resolved = null;
        }
    }
}
