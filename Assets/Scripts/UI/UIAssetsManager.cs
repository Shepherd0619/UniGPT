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
        Debug.Log("[UIAssetsManager]Start loading Addressable.");
        AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync("UI/Icon");
        yield return handle;
        Debug.Log("[UIAssetsManager]Addressable loaded.");
        Icons.Clear();
        foreach(IResourceLocation search in handle.Result)
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
        Addressables.Release(handle);

        //TODO: 加载其他类型的UI资源
        Debug.Log("[ChatWindow]Start loading asset.");
        AsyncOperationHandle<GameObject> ChatMessagePrefabHandle = Addressables.LoadAssetAsync<GameObject>("MessageUI");
        yield return ChatMessagePrefabHandle;
        Debug.Log("[ChatWindow]Addressable loaded.");
        ChatWindow.Instance.ChatMessagePrefab = ChatMessagePrefabHandle.Result;
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
        foreach(ImageSet image in Icons)
        {
            Addressables.Release(image.handle);
        }
    }
}
