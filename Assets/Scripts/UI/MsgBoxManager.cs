using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class MsgBoxManager : MonoBehaviour
{
    public GameObject Prefab;
    public List<MsgBox> MsgBoxes;
    public static MsgBoxManager Instance;
    AsyncOperationHandle<GameObject> handle;

    private void Awake() {
        Instance = this;
        
    }

    IEnumerator LoadAsset()
    {
        Debug.Log("[MsgBoxManager]Start loading asset.");
        handle = Addressables.LoadAssetAsync<GameObject>("MessageBox");
        yield return handle;
        Debug.Log("[MsgBoxManager]Addressable loaded.");
        Prefab = handle.Result;
    }

    private void OnDestroy()
    {
        Addressables.Release(handle);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMsgBox(string content, bool hasCancelButton, Action<bool> result = null){
        GameObject prefab = Instantiate(Prefab, GameObject.FindObjectOfType<Canvas>().transform);
        MsgBox msgBox = prefab.GetComponent<MsgBox>();
        msgBox.ShowMsgBox(content,hasCancelButton,result);
        MsgBoxes.Add(msgBox);
    }

    public int ShowMsgBoxNonInteractable(string content, bool hasCancelButton, Action<bool> result = null)
    {
        GameObject prefab = Instantiate(Prefab, GameObject.FindObjectOfType<Canvas>().transform);
        MsgBox msgBox = prefab.GetComponent<MsgBox>();
        msgBox.ShowMsgBoxNonInteractable(content, hasCancelButton, result);
        MsgBoxes.Add(msgBox);
        return MsgBoxes.Count - 1;
    }

    public void RemoveNonInteractableMsgBox(int msgBoxId, bool result)
    {
        if (MsgBoxes == null || MsgBoxes[msgBoxId] == null)
            return;

        MsgBoxes[msgBoxId].callback?.Invoke(result);
        Destroy(MsgBoxes[msgBoxId].gameObject);
        MsgBoxes.RemoveAt(msgBoxId);
    }

    public void RemoveMsgBox(MsgBox msgBox){
        MsgBoxes.Remove(msgBox);
        Destroy(msgBox.gameObject);
    }
}
