using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonPanelSlider : MonoBehaviour
{
    private StickerManager _stickerManagerScript;
    [SerializeField] private RectTransform _buttonGroup;
    [SerializeField] private GameObject buttonPrefab;
    private float _sliderMotion;
    private bool _sliderSlowdown = false;

    private LevelManager levelManager;

    void Awake()
    {
        _stickerManagerScript = GameObject.FindObjectOfType<StickerManager>().GetComponent<StickerManager>();
        levelManager = GameObject.FindObjectOfType<LevelManager>();

        // Clear the preview buttons
        foreach (Transform t in _buttonGroup) GameObject.Destroy(t.gameObject);
    }

    void Start()
    {
        // Generate buttons based on exisitng sticker targets
        foreach (Texture2D t in levelManager.textureList)
        {
            GameObject button = Instantiate(buttonPrefab, _buttonGroup);
            button.GetComponent<UIStickerButton>()._sticker = t;
            button.GetComponent<RawImage>().texture = t;
            button.transform.GetChild(1).GetComponent<RawImage>().texture = t;
        }
    }

    void Update()
    {
        if (!_stickerManagerScript.pointerSpawnMode && !_stickerManagerScript.isPreviewing)
        {
            if (Input.GetMouseButton(0))
            {
                _sliderMotion = Input.GetAxis("Mouse X");
                _sliderSlowdown = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _sliderSlowdown = true;
            }
        }
    }

    void LateUpdate()
    {
        if (!_stickerManagerScript.isPreviewing) 
        {
            // Need smooth the translation here
            float newXPos = Mathf.Clamp(_buttonGroup.anchoredPosition.x + 1000 * Time.deltaTime * _sliderMotion, (_buttonGroup.GetComponent<GridLayoutGroup>().spacing.x * -2f) * _buttonGroup.childCount, 0);
            _buttonGroup.anchoredPosition = new Vector2(newXPos, 0);
            if (_sliderSlowdown) _sliderMotion = Mathf.Lerp(_sliderMotion, 0, Time.deltaTime * 2);
        }
    }
}
