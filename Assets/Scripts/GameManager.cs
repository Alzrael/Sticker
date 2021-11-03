using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour
{
    // May change this class to singleton (private instance)
    [Header("Scriptable Stickers - Auto Loaded Addressables")] // Will be auto loaded (WIP)
    public List<Sticker> stickers = new List<Sticker>();

    void Awake()
    {
        // Make sure there is only one instance in the scene
        GameManager[] duplicate = GameObject.FindObjectsOfType<GameManager>();

        if (duplicate.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else 
        {
            LoadSticker();
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void LoadSticker()
    {
        // Any scriptable stickers with the STICKER tag will be loaded
        // Can also load any other type e.g. Texture2D with tags like STICKER_TEX
        Addressables.LoadAssetsAsync<Sticker>("STICKER", scriptableSticker =>
        {
            stickers.Add(scriptableSticker);
        }).Completed += OnStickerLoaded;
    }

    private void OnStickerLoaded(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<Sticker>> handle)
    {
        // Release the memory once it is done
        Addressables.Release(handle);
    }
}
