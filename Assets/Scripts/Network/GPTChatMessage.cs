using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPTChatMessage
{
    //具体情况请以是否为ClientRPC或者TargetRPC为准
    //如果为空，则为系统消息
    public GPTPlayer Sender;
    //如果为空，则为全员消息
    public GPTPlayer Receiver;
    public string content;
}
