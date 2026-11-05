namespace Poyo.EditorToolbox.Editor
{
    /// <summary>パッケージ共通の定数。</summary>
    public static class EditorToolboxInfo
    {
        public const string PackageId = "net.maaaaa.editor-toolbox";
        public const string DisplayName = "Poyo's Editor Toolbox";
        public const string Version = "0.3.0";
        public const string GlobalSettingsKey = PackageId + ".GlobalSettings";
        public const string PreferencesPath = "Preferences/" + DisplayName;
        public const string PreferencesMenuPath = "Tools/" + DisplayName + "/Preferences…";
        public const int PreferencesMenuPriority = 1001;

        // NOTE: Tools/Poyo's Editor Toolbox は子メニューを持つフォルダにする。Unity は同じパスを
        //       メニュー項目とフォルダの両方には使えないため、設定を開く項目も子に置く。
        public const string MenuPath = "Tools/" + DisplayName + "/Project Settings…";
        public const int MenuPriority = 1000;
        public const string SettingsPath = "Project/" + DisplayName;
        public const string ClippingCancellerSettingsPath = SettingsPath + "/Clipping Canceller";

        public const string LogPrefix = "[Poyo's Editor Toolbox] ";
    }
}
