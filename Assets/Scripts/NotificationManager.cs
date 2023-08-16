using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static MuteSettings muteSettings;
    public class MuteSettings
    {
        public bool MentionAll;
        public bool MentionMyself;
    }
    // 在WebGL平台上需要调用浏览器的相关API实现通知推送
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ScheduleNotification(string title, string message, float delayInSeconds);

    public static void PushNotification(string title, string message, float delayInSeconds)
    {
        ScheduleNotification(title, message, delayInSeconds);
    }

#else
    // TODO: 在PC和手机平台上可以使用Unity的本地通知系统实现通知推送

#endif
}