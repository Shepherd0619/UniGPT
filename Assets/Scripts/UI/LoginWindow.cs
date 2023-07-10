using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LoginWindow : MonoBehaviour
{
    public static LoginWindow Instance;
    public GameObject Splashscreen;
    public GameObject LoginScreen;
    public TMP_InputField ServerAddress;
    public TMP_InputField Port;
    public TMP_InputField Username;

    private void Awake()
    {
        Instance = this;
        LoginScreen.SetActive(false);
        Splashscreen.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowLoginScreen(bool needSplash)
    {
        if (needSplash)
        {
            Splashscreen.SetActive(true);
            LoginScreen.SetActive(false);
        }
        else
        {
            Splashscreen.SetActive(false);
            LoginScreen.SetActive(true);
        }
    }

    public void HideLoginScreen()
    {
        Splashscreen.SetActive(false);
        LoginScreen.SetActive(false);
    }
}
