using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPTChatMessage
{
    public GPTPlayer Sender;
    //如果为空，则为全员消息
    public GPTPlayer Receiver;
    public string content;
}
