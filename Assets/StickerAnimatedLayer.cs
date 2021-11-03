using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerAnimatedLayer : MonoBehaviour
{
    [SerializeField] protected Renderer layerRenderer;

    public void SetTexture(int index, Texture2D target, bool rotate)
    {
        layerRenderer.materials[index].SetTexture("_BaseMap", rotate ? TextureProcessing.Rotate90(target) : target);
    
    }
}
