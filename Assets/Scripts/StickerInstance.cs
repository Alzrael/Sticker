using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class StickerInstance : MonoBehaviour
{
    public int stickerLayer = 0;

    [Header("Poof VFX")]
    public GameObject VFXPoof;
    [SerializeField] [Range(0,4)] private float _poofSpeedMultiplier = .75f;

    [Header("Peel Layer")]
    public GameObject peel;
    public Texture2D peelBubbleTexture;
    [SerializeField] private bool _maskPeelLayer = true;

    [Header("Hovering Layer")]
    public GameObject hoveringLayer;
    protected float hoveringLayerAlpha = 1;

    [Header("Animated Layer (Applying/Peeling)")]
    public StickerAnimatedLayer stickerAnimatedLayer;
    public StickerAnimatedLayer peelAnimatedLayer;
    protected Animator stickerLayerAnimator;
    protected Animator peelLayerAnimator;

    void Start()
    {
        // Only set the threshold if the sticker volume overlaps with more than one object
        Collider[] hitColliders = Physics.OverlapBox(this.transform.position, this.transform.localScale / 2, this.transform.rotation);
        if (hitColliders.Length > 1)
        {
            this.GetComponent<Renderer>().material.SetFloat("_ProjectionAngleDiscardThreshold", 0.2f);
        }

        // Make a copy of the peel texture for the bubble task
        Texture2D srcTexture = (Texture2D)peel.GetComponent<Renderer>().material.GetTexture("_MainTex");
        peelBubbleTexture = new Texture2D(srcTexture.width, srcTexture.height);
        Graphics.CopyTexture(srcTexture, peelBubbleTexture);

        // Manually change the render queue of the hovering / animating layer so it can be rendered above the decal
        hoveringLayer.GetComponent<Renderer>().material.renderQueue = 3000; // The render queue of the decal system starts from 2501, each sticker will add 2 to it (one for the sticker itself and one for its peel).

        stickerLayerAnimator = stickerAnimatedLayer.GetComponent<Animator>();
        peelLayerAnimator = peelAnimatedLayer.GetComponent<Animator>();
    }

    void Update()
    {
        // Hovering layer flicking effect
        if (hoveringLayer && hoveringLayer.activeSelf)
        {
            hoveringLayerAlpha = Mathf.PingPong(Time.time * 0.5f, 0.25f);
            SetHoveringLayerColor(new Color(1,1,1,1 - hoveringLayerAlpha));  
        }
    }

    void OnDrawGizmos()
    {
        // Draw the direction for debugging purpose
        // Inside
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(this.transform.position, transform.TransformDirection(Vector3.forward) * 4f);

        // Outside
        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.transform.position, -transform.TransformDirection(Vector3.forward) * 4f);
    }

    public void PlaySpawnAnimationV2() 
    {
        stickerAnimatedLayer.gameObject.SetActive(true);
        peelAnimatedLayer.gameObject.SetActive(true);

        stickerLayerAnimator.SetTrigger("StartApplying");
        peelLayerAnimator.SetTrigger("StartApplying");
    }

    public void PlayPeelingAnimation() 
    {
        peelLayerAnimator.SetTrigger("StartPeeling");
    }


    public void PlaySpawnAnimation()
    {
        //I think the VFX always faces a set direction, regardless of the spawn rotation
        // Spawn the poof vfx first
        GameObject poof = Instantiate(VFXPoof, this.transform);

        // Make it always face to the camera
        poof.transform.LookAt(Camera.main.transform);

        // Probably also scale it based on the sticker size

        // Scripting method without using animator and animations - relative scale & poof speed 
        // Animator component needs to be disabled
        StartCoroutine(Poof());
    }

    private IEnumerator Poof()
    {
        // Record the original & target scale for x & y
        float rawScaleX = transform.localScale.x;
        float rawScaleY = transform.localScale.y;

        float targetScaleX = rawScaleX * 0.75f;
        float targetScaleY = rawScaleY * 0.75f;

        // Speed based on the scale
        float poofSpeedX = 1f * _poofSpeedMultiplier;
        float poofSpeedY = poofSpeedX * rawScaleY;

        // Scale down
        while(transform.localScale.x > targetScaleX && transform.localScale.y > targetScaleY)
        {
            transform.localScale = new Vector3(transform.localScale.x - poofSpeedX * Time.deltaTime, transform.localScale.y - poofSpeedY * Time.deltaTime, transform.localScale.z);
            yield return null;
        }

        transform.localScale = new Vector3(targetScaleX, targetScaleY, transform.localScale.z);

        // Scale up again
        while(transform.localScale.x < rawScaleX && transform.localScale.y < rawScaleY)
        {
            transform.localScale = new Vector3(transform.localScale.x + poofSpeedX * Time.deltaTime, transform.localScale.y + poofSpeedY * Time.deltaTime, transform.localScale.z);
            yield return null;
        }

        transform.localScale = new Vector3(rawScaleX, rawScaleY, transform.localScale.z);
    }

    public void UpdatePeelTexture()
    {
        peel.GetComponent<Renderer>().material.SetTexture("_MainTex", peelBubbleTexture);
    }

    public void TogglePeel(bool flag)
    {
        peel.SetActive(flag);
    }

    public void ToggleSticker(bool flag)
    {
        this.GetComponent<Renderer>().enabled = flag;
    }

    public void ToggleHoveringLayer(bool flag)
    {
        hoveringLayer.SetActive(flag);
    }

    public void SetHoveringLayerTexture(Texture2D texture)
    {
        hoveringLayer.GetComponent<Renderer>().material.SetTexture("_BaseMap", texture);
    }

    public void SetHoveringLayerColor(Color color)
    {
        hoveringLayer.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
    }

    public int UpdatePeelMask()
    {
        if (_maskPeelLayer)
        {
            // Create the mask to remove bubble outside the sticker shape
            Texture2D srcTexture = (Texture2D)this.GetComponent<Renderer>().material.GetTexture("_MainTex");
            Texture2D maskRef = new Texture2D(srcTexture.width, srcTexture.height);
            Graphics.CopyTexture(srcTexture, maskRef);

            // Scale the mask to match pixels
            TextureScale.Bilinear(maskRef, peelBubbleTexture.width, peelBubbleTexture.height);

            // 
            Color[] maskPixel = maskRef.GetPixels(0, 0, maskRef.width, maskRef.height);
            Color[] peelPixel = peelBubbleTexture.GetPixels(0, 0, peelBubbleTexture.width, peelBubbleTexture.height);

            int transparentPixel = 0;

            for (int i = 0; i < maskRef.width * maskRef.height; i++)
            {
                // Remove the pixel from the peel if the same pixel on the original sticker has an alpha channel of 0
                if (maskPixel[i].a == 0)
                {
                    peelPixel[i] = new Color(0, 0, 0, 0);
                    transparentPixel++;
                }
            }

            peelBubbleTexture.SetPixels(0, 0, peelBubbleTexture.width, peelBubbleTexture.height, peelPixel);
            peelBubbleTexture.Apply();
            UpdatePeelTexture();

            // Return valid pixel count
            return peelBubbleTexture.width * peelBubbleTexture.height - transparentPixel;
        }
        else 
        {
            return peelBubbleTexture.width * peelBubbleTexture.height;
        }
    }

    public string GetTextureName() 
    {
        return this.GetComponent<Renderer>().material.GetTexture("_MainTex").name;
    }
}
