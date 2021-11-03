using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIStickerButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ISelectHandler, IDeselectHandler
{
    public Texture2D _sticker;
    [SerializeField] [Range(0,4)] private float _poofSpeedMultiplier = 1.0f;
    private StickerManager _stickerManagerScript;
    private UIDropDownMove _UIDropDownMove;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] [Range(0,1)]private float _stickerIconScale = 0.95f;

    // Start is called before the first frame update
    private RawImage _outlineShape, _outlineMask, _stickerIcon;
    void Awake()
    {
        _stickerManagerScript =  GameObject.FindObjectOfType<StickerManager>().GetComponent<StickerManager>();//_stickerManager.GetComponent<StickerManager>();
        _UIDropDownMove = GameObject.FindObjectOfType<UIDropDownMove>().GetComponent<UIDropDownMove>();

        _outlineShape = this.GetComponent<RawImage>();
        _outlineMask = this.transform.GetChild(0).GetComponent<RawImage>();
        _stickerIcon = this.transform.GetChild(1).GetComponent<RawImage>();
        
        // Assign the mask shape
        _outlineShape.texture = _sticker;

        // Assign the outline color
        _outlineMask.color = _outlineColor;

        // Actual sticker
        _stickerIcon.texture = _sticker;
        _stickerIcon.rectTransform.localScale = _stickerIconScale * Vector3.one;

        // Hide the outline
        ToggleOutline(false);
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_stickerManagerScript.selectionMode && !_stickerManagerScript.pointerSpawnMode)
        {
            Debug.Log("Sticker Name: " + _sticker.name);
            _stickerManagerScript.SetSelectedSticker(_sticker);
            StartCoroutine(Poof());
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_stickerManagerScript.selectionMode && !_stickerManagerScript.pointerSpawnMode) _stickerManagerScript.PreviewSticker();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_stickerManagerScript.selectionMode && !_stickerManagerScript.pointerSpawnMode)  _stickerManagerScript.PlaceSticker();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _UIDropDownMove.SetCurrentlySelectedButton(this);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        
    }

    public void ToggleOutline(bool flag)
    {
        _outlineShape.enabled = flag;
        _outlineMask.enabled = flag;
    }
}
