using Poyo.EditorToolbox.Editor;
using UnityEngine;

namespace Poyo.EditorToolbox.IrodoriColorizer.Editor
{
    internal static class IrodoriSettingsMigration
    {
        private static readonly string[] LegacyPaths =
        {
            EditorToolboxLegacy.ColorizerSettingsPath,
            IrodoriInfo.StandaloneSettingsFilePath,
        };
        private static bool _checked;

        internal static void ImportPreviousSettings()
        {
            if (_checked)
            {
                return;
            }
            _checked = EditorToolboxLegacy.TryCopySettingsFile(IrodoriInfo.SettingsFilePath,
                LegacyPaths, IrodoriInfo.SettingsScriptPath, out string from);
            if (from != null)
            {
                Debug.Log(from == IrodoriInfo.StandaloneSettingsFilePath
                    ? "Irodori Colorizer: 単体版の設定を引き継ぎました。"
                    : "Irodori Colorizer: Marshmallow Jar の設定を引き継ぎました。");
            }
        }
    }
}
