using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sticker_", menuName = "ScriptableObjects/Sticker", order = 1)]
public class Sticker : ScriptableObject
{
    public string displayName;
    public Texture2D srcTexture;
    public StickerTag[] tags;
    public int price;

    public enum StickerTag 
    {
        FASHION,
        CARTOON,
        SCIFI,
        GRAFFITI,
        FOOD,
        EMOJI,
        COMPANY,
        ANIMAL,
        MISC,

    }
}
