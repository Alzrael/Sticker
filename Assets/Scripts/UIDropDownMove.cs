using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropDownMove : MonoBehaviour
{
     #region variables
    private bool UIUp;
    private Button _button;
    [SerializeField]private float smoothing = 1f;
    private RectTransform backgroundRectTransform;
    private Vector2 startLocation, targetLocation;
    [SerializeField] private StickerManager _stickerManagerScript;
    private UIStickerButton currentlySelectedButton;

    #endregion

    void Awake()
    {
        _stickerManagerScript = GameObject.FindObjectOfType<StickerManager>().GetComponent<StickerManager>();
        _stickerManagerScript.RegisterUIDropDownComponent(this);
        _button = this.GetComponent<Button>();
    }

    void Start()
    {
        backgroundRectTransform = this.transform.parent.gameObject.GetComponent<RectTransform>();
        startLocation = backgroundRectTransform.anchoredPosition;
        targetLocation = new Vector3(backgroundRectTransform.anchoredPosition.x, backgroundRectTransform.rect.height / 2);
    }

    public void MoveUI()
    {
        if(_button.enabled)
        {
            _button.enabled = false;
            if(!UIUp)
            {
                StartCoroutine(MoveCoroutine(backgroundRectTransform, targetLocation));
                UIUp = true;
            }
            else
            {
                StartCoroutine(MoveCoroutine(backgroundRectTransform, startLocation));
                UIUp = false; 
            }  
        }

    }

    public void ShowUI(bool flag)
    {
        if (flag)
        {
            StartCoroutine(MoveCoroutine(backgroundRectTransform, targetLocation)); 
        }
        else
        {   
            StartCoroutine(MoveCoroutine(backgroundRectTransform, startLocation));
        }
        UIUp = flag;
    }  

    IEnumerator MoveCoroutine(RectTransform start, Vector2 target)//Moves the background to the location passed in
    {
        while (Vector2.Distance(start.anchoredPosition, target) > 0.05f)
        {
            start.anchoredPosition = Vector2.Lerp(start.anchoredPosition, target, smoothing * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForEndOfFrame(); //Delays the end of the coroutine
        _button.enabled = true;
        _stickerManagerScript.pointerSpawnMode = !UIUp;
    }

    public void SetCurrentlySelectedButton(UIStickerButton button)
    {
        if (currentlySelectedButton) currentlySelectedButton.ToggleOutline(false);
        
        currentlySelectedButton = button;
        currentlySelectedButton.ToggleOutline(true);
    }
}
