using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // If disabled the camera will move to the sticker every time when you apply it then reset when it is done
    public bool focusOnTargetOnly = true;
    public float focusDistance = 10f;

    protected int taskDone = 0;

    // Use this to track scores (if required)
    protected List<StickerTarget> stickerTargets = new List<StickerTarget>();
    public HashSet<Texture2D> textureList = new HashSet<Texture2D>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CompleteATask()
    {
        // Only gets triggered when a sticker is placed correctly - no layer checking atm 
        taskDone ++;
        
        Debug.Log("Task Progress: " + taskDone + "/" + stickerTargets.Count);

        if (taskDone == stickerTargets.Count)
        {
            // Level complete - do something here
        }
    }

    // Advanced
    public void RegisterATask(StickerTarget stickerTarget)
    {
        stickerTargets.Add(stickerTarget);
        textureList.Add(stickerTarget.targetTexture);
    }
}
