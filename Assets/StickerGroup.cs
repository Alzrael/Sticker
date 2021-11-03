using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerGroup : MonoBehaviour
{
    public List<StickerTarget> revealOrder = new List<StickerTarget>();
    private int currentIndex = 0;

    private void Start()
    {
        if (revealOrder.Count > 0)
        {
            revealOrder[0].gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (currentIndex < revealOrder.Count) 
        {
            if (revealOrder[currentIndex].GetIsComplete()) 
            {
                currentIndex++;
                if (currentIndex < revealOrder.Count)
                {
                    revealOrder[currentIndex].gameObject.SetActive(true);
                }
            }
        }
    }
}
