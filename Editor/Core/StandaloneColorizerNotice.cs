using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    internal static class StandaloneColorizerNotice
    {
        internal const string WindowWarning =
            "単体版の Irodori Colorizer が入っているため、Poyo's Editor Toolbox の色付けを止めています。パッケージ管理ツールで単体版を削除してください。ラベルと設定は引き継がれます。";

#if EDITOR_TOOLBOX_HAS_STANDALONE_COLORIZER
        internal const bool IsStandaloneInstalled = true;
        private const string ShownKey = "EditorToolbox.StandaloneColorizerNotice.Shown";
        private const string NoticeMessage =
            "単体版の Irodori Colorizer が入っています。\n\n" +
            "単体版が入っている間は、Poyo's Editor Toolbox の色付けを止めています。\n\n" +
            "このプロジェクトの Irodori Colorizer をパッケージ管理ツールで削除してください。ラベルと設定は引き継がれます。";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (Application.isBatchMode || SessionState.GetBool(ShownKey, false))
            {
                return;
            }

            SessionState.SetBool(ShownKey, true);
            Debug.LogWarning(EditorToolboxInfo.LogPrefix + NoticeMessage.Replace("\n\n", " "));
        }
#else
        internal const bool IsStandaloneInstalled = false;
#endif
    }
}
