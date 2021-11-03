using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerTarget : MonoBehaviour
{
    [SerializeField] protected bool isStickerPiece = false;

    // For matching the texture shape
    public Texture2D targetTexture;
    public Texture2D outlineTexture;
    protected LevelManager levelManager;

    [SerializeField] protected Material stickerMaterial;
    [SerializeField] protected bool matchName = true;
    [SerializeField] protected bool matchType = false;
    
    public StickerTarget stickerBelow;

    protected bool isComplete = false;

    // Score system 
    public int Score { get; private set; }
    protected float accuracyPlacement;
    protected float accuracyRotation;
    protected const float ACCURACY_THRESHOLD= 0.97f;

    // For previewing stticker in editor
    private string textureNameOld = null;
    private Material previewMaterial = null;

    void Awake()
    {
        // Update name
        this.gameObject.name = "ST_" + targetTexture.name;

        // Use outline texture here 
        // Or mask the original texture, extract its outline using an edge detection algorithm - this will be resource-intensive
        if (outlineTexture)
        {
            // Remove black background
            int width = outlineTexture.width;
            int height = outlineTexture.height;
            Texture2D temp = new Texture2D(width, height, outlineTexture.format, true);
            Graphics.CopyTexture(outlineTexture, temp);

            Color[] pixels = temp.GetPixels(0, 0, width, height);
            for (int i = 0; i < width * height; i++)
            {
                if (pixels[i] == new Color(0, 0, 0, 1)) pixels[i].a = 0;
            }

            temp.SetPixels(0, 0, width, height, pixels);
            temp.Apply();
            this.GetComponent<Renderer>().material.SetTexture("_MainTex", temp);
        }
        else 
        {
            // Generate a mask if its outline texture is not available
            this.GetComponent<Renderer>().material.SetTexture("_MainTex", TextureProcessing.Mask(targetTexture));
        }

        // Outline always appears above any sticker
        this.GetComponent<Renderer>().material.renderQueue = 2700;

        // Unpack if in a sticker group
        if (!this.transform.parent.name.Contains("TargetObject"))
        {
            GameObject p = GameObject.Find("TargetObject");
            if (p) this.transform.parent = p.transform;
        }

        levelManager = FindObjectOfType<LevelManager>();
        if (levelManager) levelManager.RegisterATask(this);

        if (isStickerPiece) this.gameObject.SetActive(false);
    }

    // This part should be linked with the task system
    public void MarkAsComplete()
    {
        this.isComplete = true;
        Score = (int)(accuracyPlacement * 200) + (int)(accuracyRotation * 100);
        this.gameObject.SetActive(false);
        levelManager.CompleteATask();
    }

    public bool GetIsComplete()
    { 
        return isComplete; 
    }

    public void SetPlacementAccuracy(float value)
    {
        accuracyPlacement = value > ACCURACY_THRESHOLD ? 1f : value;
        Debug.Log("Placement Accuracy: " + accuracyPlacement * 100 + "%");
    }

    public void SetRotationAccuracy(float value)
    {
        accuracyRotation = value > ACCURACY_THRESHOLD ? 1f : value;
        Debug.Log("Rotation Accuracy: " + accuracyRotation * 100 + "%");
    }

    public bool IsMatching(StickerInstance instance) 
    {
        if (matchName)
        {
            if (targetTexture.name.Contains(instance.GetTextureName())) return true;
            return false;
        }
        else if (matchType)
        {
            return false;
        }
        else 
        {
            return false;
        }
    }

    // The scale is (0.1, 0.1, 0.1) by default - this function returns the scale factor for the actual sticker (only x & y matter)
    public Vector2 GetScaleFactor()
    {
        return (this.transform.localScale / 0.1f);
    }

    private void OnValidate()
    {
        // Update preview material in editor
        if (targetTexture) 
        {
            this.gameObject.name = "ST_" + targetTexture.name;

            if (string.IsNullOrWhiteSpace(textureNameOld)) textureNameOld = "default";

            if (!textureNameOld.Equals(targetTexture.name))
            {
                textureNameOld = targetTexture.name;

                previewMaterial = new Material(stickerMaterial);
                previewMaterial.SetTexture("_MainTex", targetTexture);
                this.GetComponent<Renderer>().sharedMaterial = previewMaterial;
            }
        }

        // Check if there is a sticker below
        if (this.transform.parent) 
        {
            StickerTarget st = this.transform.parent.GetComponent<StickerTarget>();
            if (st) stickerBelow = st;
        }
    }
}
