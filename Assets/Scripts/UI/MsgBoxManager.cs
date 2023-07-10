using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MsgBoxManager : MonoBehaviour
{
    public GameObject Prefab;
    public List<MsgBox> MsgBoxes;
    public static MsgBoxManager Instance;

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

    public void ShowMsgBox(string content, bool hasConfirmButton, Action<bool> result = null){
        GameObject prefab = Instantiate(Prefab, GameObject.FindObjectOfType<Canvas>().transform);
        MsgBox msgBox = prefab.GetComponent<MsgBox>();
        msgBox.ShowMsgBox(content,hasConfirmButton,result);
        MsgBoxes.Add(msgBox);
    }

    public void RemoveMsgBox(MsgBox msgBox){
        MsgBoxes.Remove(msgBox);
        Destroy(msgBox.gameObject);
    }
}
