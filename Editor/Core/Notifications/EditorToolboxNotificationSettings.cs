using System;
using UnityEngine;
using static Poyo.EditorToolbox.Editor.EditorToolboxToggleGUI;

namespace Poyo.EditorToolbox.Editor
{
    internal static class EditorToolboxNotificationSettings
    {
        internal static int MinSeconds => EditorToolboxUserSettings.Instance.overrideNotifyMinSeconds ?
            EditorToolboxUserSettings.Instance.notifyMinSeconds : EditorToolboxGlobalSettings.Instance.notifyMinSeconds;
        internal static string SuccessSoundPath => EditorToolboxUserSettings.Instance.overrideSuccessSoundPath ?
            EditorToolboxUserSettings.Instance.successSoundPath : EditorToolboxGlobalSettings.Instance.successSoundPath;
        internal static string FailureSoundPath => EditorToolboxUserSettings.Instance.overrideFailureSoundPath ?
            EditorToolboxUserSettings.Instance.failureSoundPath : EditorToolboxGlobalSettings.Instance.failureSoundPath;
        internal static string WebhookUrl => EditorToolboxUserSettings.Instance.overrideWebhookUrl ?
            EditorToolboxUserSettings.Instance.webhookUrl : EditorToolboxGlobalSettings.Instance.webhookUrl;
        internal static string WebhookTemplate => EditorToolboxUserSettings.Instance.overrideWebhookTemplate ?
            EditorToolboxUserSettings.Instance.webhookTemplate : EditorToolboxGlobalSettings.Instance.webhookTemplate;

        internal static bool HasOverride(EditorToolboxNotificationValue value)
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            switch (value)
            {
                case EditorToolboxNotificationValue.MinSeconds:
                    return settings.overrideNotifyMinSeconds;
                case EditorToolboxNotificationValue.SuccessSound:
                    return settings.overrideSuccessSoundPath;
                case EditorToolboxNotificationValue.FailureSound:
                    return settings.overrideFailureSoundPath;
                case EditorToolboxNotificationValue.WebhookUrl:
                    return settings.overrideWebhookUrl;
                case EditorToolboxNotificationValue.WebhookTemplate:
                    return settings.overrideWebhookTemplate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        internal static void ClearOverride(EditorToolboxNotificationValue value)
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            switch (value)
            {
                case EditorToolboxNotificationValue.MinSeconds:
                    settings.overrideNotifyMinSeconds = false;
                    break;
                case EditorToolboxNotificationValue.SuccessSound:
                    settings.overrideSuccessSoundPath = false;
                    break;
                case EditorToolboxNotificationValue.FailureSound:
                    settings.overrideFailureSoundPath = false;
                    break;
                case EditorToolboxNotificationValue.WebhookUrl:
                    settings.overrideWebhookUrl = false;
                    break;
                case EditorToolboxNotificationValue.WebhookTemplate:
                    settings.overrideWebhookTemplate = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
            settings.SaveChanges();
        }

        internal static bool HasAnyOverride()
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            return settings.overrideNotifyMinSeconds || settings.overrideSuccessSoundPath || settings.overrideFailureSoundPath ||
                settings.overrideWebhookUrl || settings.overrideWebhookTemplate;
        }

        internal static OverrideState GetOverrideState(EditorToolboxNotificationValue value)
        {
            if (!HasOverride(value))
            {
                return OverrideState.None;
            }
            EditorToolboxGlobalSettings global = EditorToolboxGlobalSettings.Instance;
            bool same;
            switch (value)
            {
                case EditorToolboxNotificationValue.MinSeconds:
                    same = MinSeconds == global.notifyMinSeconds;
                    break;
                case EditorToolboxNotificationValue.SuccessSound:
                    same = AreEqual(SuccessSoundPath, global.successSoundPath);
                    break;
                case EditorToolboxNotificationValue.FailureSound:
                    same = AreEqual(FailureSoundPath, global.failureSoundPath);
                    break;
                case EditorToolboxNotificationValue.WebhookUrl:
                    same = AreEqual(WebhookUrl, global.webhookUrl);
                    break;
                case EditorToolboxNotificationValue.WebhookTemplate:
                    same = AreEqual(WebhookTemplate, global.webhookTemplate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
            return same ? OverrideState.Same : OverrideState.Different;
        }

        private static bool AreEqual(string a, string b)
        {
            return string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.Ordinal);
        }

        internal static void SetMinSeconds(int seconds)
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            settings.notifyMinSeconds = Mathf.Clamp(seconds, 0, 600);
            settings.overrideNotifyMinSeconds = true;
            settings.SaveChanges();
        }

        internal static void SetSoundPath(bool success, string path)
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            if (success)
            {
                settings.successSoundPath = path;
                settings.overrideSuccessSoundPath = true;
            }
            else
            {
                settings.failureSoundPath = path;
                settings.overrideFailureSoundPath = true;
            }
            settings.SaveChanges();
        }

        internal static void SetWebhookUrl(string url)
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            settings.webhookUrl = (url ?? string.Empty).Trim();
            settings.overrideWebhookUrl = true;
            settings.SaveChanges();
        }

        internal static void SetWebhookTemplate(string template)
        {
            EditorToolboxUserSettings settings = EditorToolboxUserSettings.Instance;
            settings.webhookTemplate = template;
            settings.overrideWebhookTemplate = true;
            settings.SaveChanges();
        }
    }
}
