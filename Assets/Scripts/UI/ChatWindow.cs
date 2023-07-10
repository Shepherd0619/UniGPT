using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChatWindow : MonoBehaviour
{
    public static ChatWindow Instance;
    public ScrollRect ChatContainer;
    public GameObject ChatMessagePrefab;
    public Image LocalPlayerAvatar;
    
    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowChatWindow(){
        gameObject.SetActive(true);
    }

    public void HideChatWindow(){
        gameObject.SetActive(false);
    }

    public void Init(){

    }

    public void SetLocalPlayerInfo(GPTPlayer player){
        LocalPlayerAvatar.sprite = player.Avatar;
    }
}
