using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class UIAssetsManager : MonoBehaviour
{
    public List<ImageSet> Icons;
    public List<ImageSet> Backgrounds;
    public List<ImageSet> Emojis;
    public List<GameObject> Windows;
    [System.Serializable]
    public class ImageSet
    {
        public string Name;
        public Sprite Img;
        public AsyncOperationHandle<Sprite> handle;
    }

    public static UIAssetsManager Instance;

    private void Awake()
    {
        Instance = this;
        StartCoroutine(LoadAssets());
    }

    IEnumerator LoadAssets()
    {
        Debug.Log("[UIAssetsManager]Start loading Icon Addressable.");
        AsyncOperationHandle<IList<IResourceLocation>> IconHandle = Addressables.LoadResourceLocationsAsync("UI/Icon");
        yield return IconHandle;
        if(IconHandle.Status == AsyncOperationStatus.Failed || IconHandle.Result.Count == 0)
        {
            MsgBoxManager.Instance.ShowMsgBox("Failed to load resources. Please verify the installation.\nIf this is the first time you encountered such issue, please click Confirm.\nIf not, please click Cancel to quit and reinstall the app.",true, (result) =>
            {
                if (result)
                {
                    StartCoroutine(LoadAssets());
                }
                else
                {
                    Application.Quit();
                }
            });
        }

        Debug.Log("[UIAssetsManager]Icon Addressable loaded.");
        Icons.Clear();
        foreach (IResourceLocation search in IconHandle.Result)
        {
            if (Icons.Find(x => x.Name == search.PrimaryKey) == null)
            {
                AsyncOperationHandle<Sprite> spr = Addressables.LoadAssetAsync<Sprite>(search.PrimaryKey + "[" + Path.GetFileNameWithoutExtension(search.InternalId) + "]");
                yield return spr;
                Debug.Log("[UIAssetsManager]" + search.PrimaryKey + " loaded.");
                Icons.Add(new ImageSet()
                {
                    Name = search.PrimaryKey,
                    Img = spr.Result,
                    handle = spr
                });
            }
        }
        Addressables.Release(IconHandle);

        Debug.Log("[UIAssetsManager]Start loading Window Addressable.");
        AsyncOperationHandle<IList<IResourceLocation>> WindowHandle = Addressables.LoadResourceLocationsAsync("UI/Window");
        yield return WindowHandle;
        if (WindowHandle.Status == AsyncOperationStatus.Failed || WindowHandle.Result.Count == 0)
        {
            MsgBoxManager.Instance.ShowMsgBox("Failed to load resources. Please verify the installation.\nIf this is the first time you encountered such issue, please click Confirm.\nIf not, please click Cancel to quit and reinstall the app.", true, (result) =>
            {
                if (result)
                {
                    StartCoroutine(LoadAssets());
                }
                else
                {
                    Application.Quit();
                }
            });
        }

        Debug.Log("[UIAssetsManager]Window Addressable loaded.");
        Windows.Clear();
        foreach (IResourceLocation search in WindowHandle.Result)
        {
            if (Windows.Find(x => x.name == search.PrimaryKey) == null)
            {
                AsyncOperationHandle<GameObject> win = Addressables.LoadAssetAsync<GameObject>(search.PrimaryKey);
                yield return win;
                Debug.Log("[UIAssetsManager]" + search.PrimaryKey + " loaded.");
                Windows.Add(win.Result);
            }
        }

#if !UNITY_SERVER
        LoginWindow.Instance.ShowSplashScreen();
#endif

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Sprite GetIcon(string Name)
    {
        return (Icons.Find(result => result.Name == Name)).Img;
    }

    public Texture2D GetIcon2Texture(string Name)
    {
        Sprite sprite = (Icons.Find(result => result.Name == Name)).Img;
        Texture2D texture = new Texture2D(sprite.texture.width, sprite.texture.height, sprite.texture.format, false);
        texture.SetPixels(sprite.texture.GetPixels());
        texture.Apply();

        return texture;
    }

    private void OnDestroy()
    {
        foreach (ImageSet image in Icons)
        {
            Addressables.Release(image.handle);
        }
    }
}
