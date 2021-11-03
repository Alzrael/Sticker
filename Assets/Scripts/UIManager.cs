using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Lets not bother using this system for the Ad
//Make a simpler system showing off a handful of stickers

public class UIManager : MonoBehaviour
{
        public GameObject buttonPrefab;
        public Transform buttonContainer;

        [SerializeField] private int _stickerCount;
        private Button [] _buttons;
        private Texture [] _stickers;


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        GetAllStickers();
        SpawnAllButtons();
    }

    void SpawnAllButtons()
    {
        //Checks for duplicate UI managers as it persists through scenes. Will only set up a new one if 
        //there is no UI manager currently in scene. Else, destroy itself
        UIManager [] duplicate = GameObject.FindObjectsOfType<UIManager>();
        string textureName;

        if (duplicate.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            foreach(Texture texture in _stickers) //Makes a button foreach sticker
            {
                GameObject go = Instantiate(buttonPrefab) as GameObject;
                go.transform.SetParent(buttonContainer);
                go.GetComponent<RawImage>().texture = texture;

                textureName = texture.name;
                go.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(textureName));
            }
        }
    }
    void GetAllStickers()
    {
        //Grabs all stickers from folder and places them into an array
        _stickers = Resources.LoadAll<Texture>("Materials/Stickers");
        _stickerCount = _stickers.Length;
    }

    public void OnButtonClick(string stickerName)
    {
        Debug.Log("Sticker Name:  "+stickerName);
    }
}
