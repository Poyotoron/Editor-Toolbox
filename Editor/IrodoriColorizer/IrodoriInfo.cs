namespace Poyo.EditorToolbox.IrodoriColorizer.Editor
{
    /// <summary>パッケージ共通の定数。</summary>
    internal static class IrodoriInfo
    {
        internal const string DisplayName = "Irodori Colorizer";

        // NOTE: 色付けは Poyo's Editor Toolbox の一機能なので、右クリックメニューの親はパッケージ名に揃える。
        //       深さを変えると既存の利用者が項目を探せなくなるため、1 段のまま置き換える。
        internal const string AssetMenuRoot = "Assets/Poyo's Editor Toolbox/";
        internal const string GameObjectMenuRoot = "GameObject/Poyo's Editor Toolbox/";

        internal const string SettingsPath = "Project/Poyo's Editor Toolbox/Irodori Colorizer";

        internal const string SettingsFilePath = "ProjectSettings/EditorToolbox.IrodoriColorizer.asset";
        internal const string StandaloneSettingsFilePath = "ProjectSettings/IrodoriColorizer.asset";
        internal const string SettingsScriptPath = "Packages/net.maaaaa.editor-toolbox/Editor/IrodoriColorizer/IrodoriSettings.cs";
    }
}
