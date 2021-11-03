using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* TextureProcessing.cs
* Yuqi Wang
* 25 Aug 2021
* Pixel based texutre processing tool
*/

public static class TextureProcessing
{
    public static Texture2D Rotate90(Texture2D input, bool clockwise = true)
    {
        int w = input.width;
        int h = input.height;

        Texture2D temp = new Texture2D(w, h);
        Graphics.CopyTexture(input, temp);

        Color[] pixels = temp.GetPixels();
        Color[] rotatedPixels = new Color[pixels.Length];

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? pixels.Length - 1 - (j * w + i) : j * w + i;
                rotatedPixels[iRotated] = pixels[iOriginal];
            }
        }

        Texture2D result = new Texture2D(h, w);
        result.SetPixels(rotatedPixels);
        result.Apply();
        return result;
    }

    public static Texture2D Flip(Texture2D input, bool vertical = false)
    {
        Texture2D result = new Texture2D(input.width, input.height);

        int xN = input.width;
        int yN = input.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                if (vertical)
                {
                    result.SetPixel(xN - i - 1, j, input.GetPixel(i, j));
                }
                else
                {
                    result.SetPixel(i, j, input.GetPixel(xN - i - 1, j));
                }
            }
        }
        result.Apply();

        return result;
    }

    public static Texture2D Mask(Texture2D input)
    {
        int w = input.width;
        int h = input.height;

        Texture2D result = new Texture2D(w, h);
        Graphics.CopyTexture(input, result);

        Color[] maskPixel = result.GetPixels(0, 0, w, h);

        for (int i = 0; i < w * h; i++)
        {
            if (maskPixel[i].a != 0) maskPixel[i] = Color.white;
        }

        result.SetPixels(0, 0, w, h, maskPixel);
        result.Apply();
        return result;
    }
}
