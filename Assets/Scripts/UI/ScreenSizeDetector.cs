using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSizeDetector : MonoBehaviour
{
    private int previousWidth;
    private int previousHeight;
    public List<GameObject> Listeners;
    public static ScreenSizeDetector Instance;
    IEnumerator Start()
    {
        Instance = this;
        // 记录初始的屏幕尺寸
        previousWidth = Screen.width;
        previousHeight = Screen.height;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            // 如果当前的屏幕尺寸与上一次记录的不同，表示屏幕尺寸发生了修改
            if (Screen.width != previousWidth || Screen.height != previousHeight)
            {
                // 屏幕尺寸发生了修改，执行对应的操作
                // ...
                Debug.Log("Screen size change detected! " + Screen.currentResolution);
                foreach(GameObject obj in Listeners){
                    obj.SendMessage("OnScreenSizeChanged");
                }
                // 更新记录的屏幕尺寸
                previousWidth = Screen.width;
                previousHeight = Screen.height;
            }
        }
    }
}