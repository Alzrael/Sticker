#define USE_ASMR // Comment out this line to use the original version
//#define USE_PRECISE_RAYCAST // Precise -> RaycastHit | Imprecise -> RaycastHitAll

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerManager : MonoBehaviour
{
    [SerializeField] protected int stickerCount = 1;

    // Game / interaction modes
    public bool creationMode = false; // Stickers are not limited to the target object
    public bool selectionMode = false; // Temporary flag for camera rotation mode
    public bool pointerSpawnMode = false; // Allow spawning the same sticker without the repeating the drag & drop process
    
    // Game state flags
    public bool isPreviewing = false;
    public bool isTaskInProgress = false;
    public Task currentTask = Task.IDLE; // More specified task

    // Sticker instance and reference
    [SerializeField] protected GameObject stickerPrefab;
    [SerializeField] protected Texture2D selectedTexture;
    protected GameObject previewSticker;
    protected StickerInstance stickerInstance; // StickerInstance component reference of the previewSticker
    protected StickerTarget stickerTarget;
    protected GameObject pointedObject; // Currently pointed object in the scene
    protected CameraControl cameraControl;
    protected UIDropDownMove uiDropDownMove;

    [Header("Peeling Settings")]
    public int peelBrushSize = 256;
    public Color peelBackgroundColor; // This needs to match the peel texture background color (usually black with transparency)
    protected int totalPixelCount = 0;
    protected float peelingMotion;

    [Header("UI Elements")]
    [SerializeField] protected TMPro.TextMeshProUGUI instructionLabel;

    protected LevelManager levelManager;
    protected GameManager gameManager;

    public enum Task
    {
        IDLE,
        ROTATION,
        PUSHING_BUBBLE,
        PEELING
    }

    void Awake()
    {
        // stickerTextures = Resources.LoadAll<Texture2D>("StickersV2");
        levelManager = FindObjectOfType<LevelManager>();
        gameManager = FindObjectOfType<GameManager>();
        // The LoadSticker() method in GameManager does the same job
        // This method is limited to files under the Resources folder, meanwhile an addressable asset just needs a tag so it can be loaded from anywhere in the project
        // Resources and Addressables assets do not coexist as they use different management systems - the later yields reduced build size & reduced loading time
    }

    void Start()
    {
        cameraControl = Camera.main.GetComponent<CameraControl>();
    }

    void Update()
    {
        // Point Spawn Mode Disabled
        //// Drag & drop *** REPLACE MOUSE INPUT WITH MOBILE TOUCH INPUT HERE ***
        //if (Input.GetMouseButton(0))
        //{
        //    if (!selectionMode && pointerSpawnMode) PreviewSticker();
        //}

        //if (Input.GetMouseButtonUp(0)) 
        //{
        //    if (!selectionMode && pointerSpawnMode) PlaceSticker();
        //}

        // Get TAB key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            selectionMode = !selectionMode;
        }        
        
        // Press P to complete the current task
        if (Input.GetKeyDown(KeyCode.P) && isTaskInProgress)
        {
            switch (currentTask)
            {
                case Task.IDLE:
                //instructionLabel.text = "Place the Sticker!";
                break;
                case Task.ROTATION:
                instructionLabel.text = "Rotate the Sticker!";
                CompleteRotation();
                break;
                case Task.PUSHING_BUBBLE:
                break;
                case Task.PEELING:
                break;
                default:
                break;
            }
        }   

        if (previewSticker && isTaskInProgress)
        {
            // Replace the Keys with UI buttons or touch input
            if (currentTask == Task.ROTATION)
            {   
                if (Input.GetKey(KeyCode.Q)) previewSticker.transform.Rotate(new Vector3(0,0,1));
                if (Input.GetKey(KeyCode.E)) previewSticker.transform.Rotate(new Vector3(0,0,-1));
            }

            // Peeling task?
            if (currentTask == Task.PUSHING_BUBBLE)
            {
                if (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hitInfo;

                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        if (hitInfo.transform.name.Contains("Sticker_"))
                        {
                            // Need to use 1 minus texcoord because the direction is inward due to the decal system
                            // The src texture needs to be RGBA 32bit (might not work on mobile)
                            // The sticker object requires a mesh collider with convex off so texcoord can be extracted

                            int x = (int)((1 - hitInfo.textureCoord.x) * stickerInstance.peelBubbleTexture.width);
                            int y = (int)((1 - hitInfo.textureCoord.y) * stickerInstance.peelBubbleTexture.height);

                            if (x < peelBrushSize / 2 + 1 || x > stickerInstance.peelBubbleTexture.width - peelBrushSize / 2 - 1 || y < peelBrushSize / 2 + 1 || y > stickerInstance.peelBubbleTexture.height - peelBrushSize / 2 - 1)
                            {
                                // OUT OF BOUNDS - DO NOTHING
                            }
                            else
                            {
                                // Replace the selected region with color c
                                Color[] c = stickerInstance.peelBubbleTexture.GetPixels(x - peelBrushSize / 2, y - peelBrushSize / 2, peelBrushSize, peelBrushSize);

                                for (int i = 0; i < c.Length; i++)
                                {
                                    // Ignore transparent pixels
                                    if (c[i] != new Color(0, 0, 0, 0)) c[i] = peelBackgroundColor;
                                }

                                stickerInstance.peelBubbleTexture.SetPixels(x - peelBrushSize / 2, y - peelBrushSize / 2, peelBrushSize, peelBrushSize, c);

                                // Apply change on the GPU
                                stickerInstance.peelBubbleTexture.Apply();

                                // Then swap out the old texture
                                stickerInstance.UpdatePeelTexture();

#if USE_ASMR
                                stickerInstance.peelAnimatedLayer.SetTexture(1, stickerInstance.peelBubbleTexture, true);
#else

#endif
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    // Only check once when the mouse(touch) is released
                    if (CheckPeelingState(stickerInstance.peelBubbleTexture))
                    {
#if USE_ASMR
                        CompletePushingOutBubbles();
#else
                        CompleteStickerTasks();
#endif
                    }
                }
            }
            else if (currentTask == Task.PEELING) 
            {
                // Replace with touch input
                if (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;

                    if (Physics.Raycast(ray, out hitInfo) && hitInfo.transform.name.Contains("Sticker_")) 
                    {
                        peelingMotion = Input.GetAxis("Mouse Y");
                        if (peelingMotion < 0) CompletePeeling();
                    }

                }
            }
        }
    }

    public void SetSelectedSticker(Texture2D texture) //Used by UIStickerButton.cs to set the sticker based on selection from menu
    {
        selectedTexture = texture;
    }

    public void PreviewSticker()
    {
        isPreviewing = true;

        // Setup raycast
        RaycastHit hit;
        
        // Shoot the ray from the center of the camera to the clicked position 
        // *** REPLACE MOUSE INPUT WITH MOBILE TOUCH INPUT HERE ***
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // If hit something
        if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag.Contains("Target"))
        {
            pointedObject = hit.transform.gameObject;

            // Create an instance of the sticker at the hit position
            if (!previewSticker)
            {
                stickerCount ++;

                previewSticker = Instantiate(stickerPrefab, hit.point, Quaternion.identity); 

                stickerInstance = previewSticker.GetComponent<StickerInstance>();

                Renderer renderer = previewSticker.GetComponent<Renderer>();

                // Apply a random sticker
                if(selectedTexture == null)//If you haven't selected a sticker from menu, set sticker to a random one
                {
                    selectedTexture = gameManager.stickers[UnityEngine.Random.Range(0, gameManager.stickers.Count)].srcTexture;
                }

                renderer.material.SetTexture("_MainTex", selectedTexture);

                // Also adjust the scale
                previewSticker.transform.localScale = new Vector3(previewSticker.transform.localScale.x, previewSticker.transform.localScale.y * selectedTexture.height/selectedTexture.width, previewSticker.transform.localScale.z);

                // Set render queue, first come first render so there is no z-fighting
                renderer.material.renderQueue += stickerCount * 2 - 1;
                
                // Peel always render above the actual sticker
                stickerInstance.peel.GetComponent<Renderer>().material.renderQueue = 2501 + stickerCount * 2;

                stickerInstance.stickerLayer = stickerCount;

                stickerInstance.SetHoveringLayerTexture(selectedTexture);

                previewSticker.name = "Sticker_" + stickerCount;

            } 
            else
            {
                // Update position
                previewSticker.transform.position = hit.point;

                // Match size & rotation
                if (hit.collider.gameObject.GetComponent<StickerTarget>())
                {
                    previewSticker.transform.localScale = Vector3.one;
                    Vector2 scaleFactor = hit.collider.gameObject.GetComponent<StickerTarget>().GetScaleFactor();
                    previewSticker.transform.localScale = new Vector3(previewSticker.transform.localScale.x * scaleFactor.x, previewSticker.transform.localScale.y * scaleFactor.y, previewSticker.transform.localScale.z);

                    previewSticker.transform.rotation = hit.collider.gameObject.transform.rotation;
                }
                else
                {
                    previewSticker.transform.localScale = Vector3.one;
                    previewSticker.transform.rotation = Quaternion.identity;

                    // Align with surface normal
                    previewSticker.transform.LookAt(hit.normal);
                    previewSticker.transform.forward = -hit.normal;
                }
            }
        }
    }

    // 
    public void PlaceSticker()
    {
#if USE_PRECISE_RAYCAST
        if (pointedObject) stickerTarget = pointedObject.GetComponent<StickerTarget>();
#else
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            stickerTarget = hit.transform.gameObject.GetComponent<StickerTarget>();
            if (stickerTarget && stickerTarget.IsMatching(stickerInstance))
            {
                break;
            }
        }
#endif
        // Place & prepare for the next one
        if (previewSticker)
        {
            if (!creationMode)
            {
                // Use the following parts if players are only allowed to place the sticker to the designated target (e.g. shape, rotation matching)
                if (stickerTarget && stickerTarget.IsMatching(stickerInstance))
                {
                    if (stickerTarget.stickerBelow)
                    {
                        if (stickerTarget.stickerBelow.GetIsComplete())
                        {
                            // No incomplete sticker below the current target
                            Commit();
                        }
                        else 
                        {
                            // Incomplete sticker below the current target
                            Discard();
                        }
                    }
                    else 
                    {
                        // No sticker below the current target
                        Commit();
                    }
                }
                else
                {
                    // Mismatched target
                    Discard();
                }
            }
            else
            {
                if (levelManager && !levelManager.focusOnTargetOnly)
                {
                    // This parts allows any target
                    cameraControl.SetFocusPoint(previewSticker.transform);
                    // Place the camera in front of the target sticker (5 units away)
                    cameraControl.MoveToPosition(previewSticker.transform.position - previewSticker.transform.forward * 5f);
                }

                previewSticker.GetComponent<StickerInstance>().PlaySpawnAnimation();

                BeginStickerTasks();
            }
        } 
    }

    private void Commit() 
    {
        // Accuracy based on X & Y coordinates. Z is ignored as it is for the depth.
        stickerTarget.SetPlacementAccuracy(1 - Mathf.Clamp((new Vector2(previewSticker.transform.position.x, previewSticker.transform.position.y) - new Vector2(stickerTarget.transform.position.x, stickerTarget.transform.position.y)).magnitude, 0, 1));

        // Then center it for the rotation task
        previewSticker.transform.position = stickerTarget.transform.position;

        // Move it a bit closer to avoid overlapping
        previewSticker.transform.position -= previewSticker.transform.forward * 0.01f;

        // Match the target scale & rotation
        previewSticker.transform.localScale = Vector3.one;
        Vector2 scaleFactor = stickerTarget.GetScaleFactor();
        previewSticker.transform.localScale = new Vector3(previewSticker.transform.localScale.x * scaleFactor.x, previewSticker.transform.localScale.y * scaleFactor.y, previewSticker.transform.localScale.z);
        previewSticker.transform.rotation = stickerTarget.transform.rotation;

        if (levelManager && !levelManager.focusOnTargetOnly)
        {
            cameraControl.SetFocusPoint(previewSticker.transform);

            // Place the camera in front of the target sticker (5 units away)
            cameraControl.MoveToPosition(previewSticker.transform.position - previewSticker.transform.forward * 5f);
        }

        BeginStickerTasks();
    }

    private void Discard() 
    {
        // Reset everything here
        stickerCount--;
        isPreviewing = false;
        pointedObject = null;
        stickerInstance = null;
        Destroy(previewSticker);
    }


    // 
    private void SelectSticker()
    {
        // RaycastHit hit;
        // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // if (Physics.Raycast(ray, out hit))
        // {
        //     GameObject obj = hit.transform.gameObject;
        //     if (obj.GetComponent<Sticker>()) Destroy(obj);
        // }
    }

    private void BeginStickerTasks()
    {   
        // Change game state
        isTaskInProgress = true;
        currentTask = Task.ROTATION;

        // Hide the UI
        uiDropDownMove.ShowUI(false);
        
        // Toggle sticker visibility
        stickerInstance.ToggleSticker(false);
        stickerInstance.ToggleHoveringLayer(true);

#if USE_ASMR
        CompleteRotation();
#else

#endif
    }

    private void CompleteRotation()
    {
        // Prepare for rotation degree matching (world-space)
        stickerTarget.transform.parent = null;

        // Check the rotation difference (value between 0 to 180)
        float angleDiff = Quaternion.Angle(previewSticker.transform.rotation, stickerTarget.transform.rotation);
        stickerTarget.SetRotationAccuracy((180f - angleDiff) / 180f);

        // Generate the peel texture based on the sticker shape - the valid total pixel is based on how many pixels in that shape
        totalPixelCount = stickerInstance.UpdatePeelMask();

#if USE_ASMR

        stickerInstance.stickerAnimatedLayer.SetTexture(1, selectedTexture, true);
        stickerInstance.peelAnimatedLayer.SetTexture(1, stickerInstance.peelBubbleTexture, true);
        stickerInstance.PlaySpawnAnimationV2();
        stickerInstance.ToggleHoveringLayer(false);

#else

        // Play spawn animation and toggle visibility
        stickerInstance.PlaySpawnAnimation();
        stickerInstance.ToggleHoveringLayer(false);
        stickerInstance.ToggleSticker(true);
        stickerInstance.TogglePeel(true);

#endif

        // Enable the collider for pushing out bubbles
        previewSticker.GetComponent<Collider>().enabled = true;

        // Change game state
        currentTask = Task.PUSHING_BUBBLE;
        instructionLabel.text = "Push out Bubbles\n↑↓←→ Press & Move";
    }

    private bool CheckPeelingState(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels(0, 0, texture.width, texture.height);
        int completedPixelCount = 0;

        foreach (Color c in pixels)
        {
            if (c == peelBackgroundColor) completedPixelCount ++;

            // Complete the peeling task if the percentage is above 98%
            if ((float)completedPixelCount/(float)totalPixelCount >= 0.98f) return true;
        }

        Debug.Log("Peeling Percentage: " + ((float)completedPixelCount/(float)totalPixelCount) * 100 + "%");

        return false;
    }

    private void CompletePushingOutBubbles() 
    {
        currentTask = Task.PEELING;
        instructionLabel.text = "Remove the Peel\n↓↓ Slide Down";
    }

    private void CompletePeeling() 
    {
        // Play peeling animation
        stickerInstance.stickerAnimatedLayer.gameObject.SetActive(true);
        stickerInstance.PlayPeelingAnimation();

        // Remove layers once done
        StartCoroutine(AddRigidbodyToPeel());
        Destroy(stickerInstance.peelAnimatedLayer.gameObject, 3f);
        Destroy(stickerInstance.stickerAnimatedLayer.gameObject, 1f); // Or hide it for the sticker removal operation

        // Show the decal
        stickerInstance.ToggleSticker(true);

        CompleteStickerTasks();
    }
    
    private void CompleteStickerTasks()
    {   
        // Reset game state
        currentTask = Task.IDLE;
        instructionLabel.text = "";
        isTaskInProgress = false;
        isPreviewing = false;

        // Reset preview sticker
        stickerInstance.TogglePeel(false);
        previewSticker.GetComponent<Collider>().enabled = false;
        previewSticker = null;

        // Reset sticker target
        stickerTarget.MarkAsComplete();
        stickerTarget = null;
        pointedObject = null;

        // Reset camera position and bring back the UI
        if (levelManager && !levelManager.focusOnTargetOnly) 
        {
            cameraControl.MoveBack();
        } 

        uiDropDownMove.ShowUI(true);
    }

    public void RegisterUIDropDownComponent(UIDropDownMove ui)
    {
        uiDropDownMove = ui;
    }

    private IEnumerator AddRigidbodyToPeel()
    {
        // Add a rigidbody so the peel will fall due to the gravity
        yield return new WaitForSeconds(0.75f);
        stickerInstance.peelAnimatedLayer.gameObject.AddComponent<Rigidbody>();
    }

}
