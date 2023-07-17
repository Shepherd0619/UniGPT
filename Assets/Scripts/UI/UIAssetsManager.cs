using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    }

    public static UIAssetsManager Instance;

    private void Awake()
    {
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
}
