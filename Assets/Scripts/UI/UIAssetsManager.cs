using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAssetsManager : MonoBehaviour
{
    public List<ImageSet> Icons;
    public List<ImageSet> Backgrounds;
    public List<ImageSet> Emojis;

    public class ImageSet{
        public string Name;
        public Sprite Img;
    }

    public static UIAssetsManager Instance;

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

    public Sprite GetIcon(string Name){
        return (Icons.Find(result => result.Name == Name)).Img;
    }
}
