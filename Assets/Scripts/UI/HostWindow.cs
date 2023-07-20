using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ES3Internal;

public class HostWindow : MonoBehaviour
{
    public static HostWindow Instance;
    public GameObject ConfigWindow;
    public List<UI_ConfigBool> UI_ConfigBools;
    public List<UI_ConfigInt> UI_ConfigInts;
    public List<UI_ConfigText> UI_ConfigTexts;
    public Button OKBtn;
    public Button CancelBtn;
    
    public class Config
    {
        [System.Serializable]
        public class Text
        {
            public string Name;
            public string Value;
            public string Default;
        }
        [System.Serializable]
        public class Int
        {
            public string Name;
            public int Value;
            public int Default;
        }
        [System.Serializable]
        public class Bool
        {
            public string Name;
            public bool Value;
            public bool Default;
        }
    }
    [System.Serializable]
    public class UI_ConfigText
    {
        public Config.Text Config;
        public TMP_InputField UI;
    }
    [System.Serializable]
    public class UI_ConfigInt
    {
        public Config.Int Config;
        public TMP_InputField UI;
    }
    [System.Serializable]
    public class UI_ConfigBool
    {
        public Config.Bool Config;
        public Toggle UI;
    }

    private void Awake()
    {
        ConfigWindow.SetActive(false);
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

    public void ShowConfigWindow(bool hasCancelBtn)
    {
        ConfigWindow.SetActive(true);
        //TODO: 应该弄成类似提示框那种东西，这样的话能把ConfigWindow重新利用。
        //如在开服时作为开服前的参数配置，开服后是正常的修改运行时配置。
    }

    public void HideConfigWindow()
    {
        ConfigWindow.SetActive(false);
    }

    public void LoadLastSavedConfig()
    {
        foreach(UI_ConfigBool cb in UI_ConfigBools){
            cb.Config.Value = ES3.Load<bool>(cb.Config.Name, cb.Config.Default);
            cb.UI.isOn = cb.Config.Value;
        }

        foreach(UI_ConfigInt ci in UI_ConfigInts){
            ci.Config.Value = ES3.Load<int>(ci.Config.Name, ci.Config.Default);
            ci.UI.text = ci.Config.Value.ToString();
        }

        foreach(UI_ConfigText ct in UI_ConfigTexts){
            ct.Config.Value = ES3.Load<string>(ct.Config.Name, ct.Config.Default);
            ct.UI.text = ct.Config.Value;
        }
        Debug.Log("[HostWindow]LastSavedConfig Loaded!");
    }

    public void WriteConfig(){
        foreach(UI_ConfigBool cb in UI_ConfigBools){
            ES3.Save<Config.Bool>(cb.Config.Name, cb.Config);
        }

        foreach(UI_ConfigInt ci in UI_ConfigInts){
            ES3.Save<Config.Int>(ci.Config.Name, ci.Config);
        }

        foreach(UI_ConfigText ct in UI_ConfigTexts){
            ES3.Save<Config.Text>(ct.Config.Name, ct.Config);
        }
        Debug.Log("[HostWindow]Config Written!");
    }

}
