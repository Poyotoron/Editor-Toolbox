using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    internal static class LilToonClippingCanceller
    {
        internal const string PendingMessageSessionKey = "net.maaaaa.editor-toolbox.ClippingCanceller.LilToonMessage";
        internal const string SettingFilePath = "ProjectSettings/lilToonSetting.json";
        internal const string LockFilePath = "Packages/jp.lilxyzw.liltoon/SettingLock.json";
        internal const string RefreshMenuPath = "Assets/lilToon/[Shader] Refresh shaders";
        internal const string UnreadableMessage = "lilToon の設定ファイル（ProjectSettings/lilToonSetting.json）を読み取れませんでした。lilToon のメニュー Assets > lilToon > [Shader] Refresh shaders を一度実行すると作られます。";
        private static readonly Regex FeaturePattern =
            new Regex("(\"LIL_FEATURE_CLIPPING_CANCELLER\"\\s*:\\s*)(true|false)");
        internal enum State
        {
            NotInstalled,
            Unreadable,
            Off,
            On,
        }
        internal static readonly bool IsInstalled =
#if EDITOR_TOOLBOX_HAS_LILTOON
            true;
#else
            false;
#endif

        internal static State ReadState()
        {
            if (!IsInstalled)
            {
                return State.NotInstalled;
            }
            if (!TryReadSetting(out string text, out Encoding encoding))
            {
                return State.Unreadable;
            }
            Match match = FeaturePattern.Match(text);
            return !match.Success ? State.Unreadable :
                match.Groups[2].Value == "true" ? State.On : State.Off;
        }

        internal static bool IsLocked()
        {
            return File.Exists(LockFilePath);
        }

        internal static long GetChangeStamp()
        {
            // NOTE: lilToon のマテリアルの画面からも設定ファイルが書き換わるため、更新時刻とロックの有無で変化を見る。
            long stamp = File.Exists(SettingFilePath) ? File.GetLastWriteTimeUtc(SettingFilePath).Ticks : 0L;
            return IsLocked() ? ~stamp : stamp;
        }

        internal static string ReplaceFeatureValue(string json, bool enabled)
        {
            // NOTE: lilToon のアセンブリを参照せずに済ませるため、該当箇所だけをテキストで書き換える。
            return FeaturePattern.Replace(json, "${1}" + (enabled ? "true" : "false"), 1);
        }

        internal static bool TrySetEnabled(bool enabled, out string message)
        {
            if (!IsInstalled)
            {
                message = "lilToon が見つかりません。";
                return false;
            }
            if (IsLocked())
            {
                message = "lilToon の設定がロックされているため、切り替えませんでした。lilToon の設定画面でロックを外してください。";
                return false;
            }
            if (!TryReadSetting(out string text, out Encoding encoding))
            {
                message = UnreadableMessage;
                return false;
            }
            Match match = FeaturePattern.Match(text);
            if (!match.Success)
            {
                message = UnreadableMessage;
                return false;
            }
            if ((match.Groups[2].Value == "true") == enabled)
            {
                message = null;
                return true;
            }
            try
            {
                // NOTE: 読み込んだ文字コードを使い、BOM と改行をそのまま残す。
                File.WriteAllText(SettingFilePath, ReplaceFeatureValue(text, enabled), encoding);
            }
            catch (IOException exception)
            {
                Debug.LogException(exception);
                message = "lilToon の設定ファイルを書き換えられませんでした。";
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                Debug.LogException(exception);
                message = "lilToon の設定ファイルを書き換えられませんでした。";
                return false;
            }
            Debug.Log(EditorToolboxInfo.LogPrefix + "lilToon の距離クリッピングキャンセラーを " +
                (enabled ? "ON" : "OFF") + " にしました。シェーダーを作り直します。");
            // NOTE: 作り直しの処理は公開されていないため、設定を読み直す lilToon 自身のメニューを呼ぶ。
            if (EditorApplication.ExecuteMenuItem(RefreshMenuPath))
            {
                SessionState.EraseString(PendingMessageSessionKey);
                message = null;
            }
            else
            {
                message = "lilToon の設定は書き換えましたが、シェーダーを作り直せませんでした。メニュー Assets > lilToon > [Shader] Refresh shaders を実行してください。";
                // NOTE: 再読み込みで画面の状態が消えるため、警告をセッションに残して次に開いたときに出す。
                SessionState.SetString(PendingMessageSessionKey, message);
                Debug.LogWarning(EditorToolboxInfo.LogPrefix + message);
            }
            // NOTE: lilToon の画面が古い設定を表示したり上書き保存したりしないよう、保持した設定を捨てさせる。
            EditorUtility.RequestScriptReload();
            return true;
        }

        private static bool TryReadSetting(out string text, out Encoding encoding)
        {
            text = null;
            encoding = null;
            if (!File.Exists(SettingFilePath))
            {
                return false;
            }
            try
            {
                using (var reader = new StreamReader(SettingFilePath, new UTF8Encoding(false), true))
                {
                    text = reader.ReadToEnd();
                    encoding = reader.CurrentEncoding;
                }
                return true;
            }
            catch (IOException exception)
            {
                Debug.LogException(exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                Debug.LogException(exception);
            }
            return false;
        }
    }
}
