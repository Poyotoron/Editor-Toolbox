using System;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    public static class EditorToolboxNotifier
    {
        internal const string BatchRunningSessionKey = "net.maaaaa.editor-toolbox.Notify.BatchRunning";
        public static int MinSeconds => EditorToolboxNotificationSettings.MinSeconds;

        public static void Notify(EditorToolboxNotificationEvent notificationEvent, bool success, string title, string message)
        {
            if (Application.isBatchMode)
            {
                return;
            }
            if (!EditorToolboxToggles.IsOn(EditorToolboxToggle.CompletionNotifications))
            {
                return;
            }
            // NOTE: 連続アップロードの途中のアップロード・Play モード・コンパイルは、終わりにまとめて知らせる。
            if ((notificationEvent == EditorToolboxNotificationEvent.Upload ||
                notificationEvent == EditorToolboxNotificationEvent.PlayMode ||
                notificationEvent == EditorToolboxNotificationEvent.Import) &&
                SessionState.GetBool(BatchRunningSessionKey, false))
            {
                return;
            }
            GetToggles(notificationEvent, out EditorToolboxToggle sound, out EditorToolboxToggle webhook);
            bool playSound = EditorToolboxToggles.IsOn(sound);
            bool sendWebhook = EditorToolboxToggles.IsOn(webhook);
            if (!playSound && !sendWebhook)
            {
                return;
            }
            Debug.Log(EditorToolboxInfo.LogPrefix + title + ": " + message);
            if (playSound)
            {
                NotificationSound.Play(success);
            }
            if (sendWebhook)
            {
                WebhookSender.Send(EditorToolboxNotificationSettings.WebhookUrl, EditorToolboxNotificationSettings.WebhookTemplate,
                    title, message, success, false);
            }
        }

        internal static void GetToggles(EditorToolboxNotificationEvent e, out EditorToolboxToggle sound, out EditorToolboxToggle webhook)
        {
            switch (e)
            {
                case EditorToolboxNotificationEvent.Upload:
                    sound = EditorToolboxToggle.NotifyUploadSound;
                    webhook = EditorToolboxToggle.NotifyUploadWebhook;
                    break;
                case EditorToolboxNotificationEvent.BatchUpload:
                    sound = EditorToolboxToggle.NotifyBatchUploadSound;
                    webhook = EditorToolboxToggle.NotifyBatchUploadWebhook;
                    break;
                case EditorToolboxNotificationEvent.OutfitConversion:
                    sound = EditorToolboxToggle.NotifyOutfitConversionSound;
                    webhook = EditorToolboxToggle.NotifyOutfitConversionWebhook;
                    break;
                case EditorToolboxNotificationEvent.PlayMode:
                    sound = EditorToolboxToggle.NotifyPlayModeSound;
                    webhook = EditorToolboxToggle.NotifyPlayModeWebhook;
                    break;
                case EditorToolboxNotificationEvent.Import:
                    sound = EditorToolboxToggle.NotifyImportSound;
                    webhook = EditorToolboxToggle.NotifyImportWebhook;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        public static string FormatElapsed(double seconds)
        {
            if (seconds < 0)
            {
                return string.Empty;
            }
            int total = (int)Math.Round(seconds);
            return total < 60 ? $"{total} 秒" : $"{total / 60} 分 {total % 60:00} 秒";
        }
    }
}
