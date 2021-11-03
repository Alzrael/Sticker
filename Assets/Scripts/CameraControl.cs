using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float _rotationSpeed = 4;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private bool _canRotate = false;

    protected Vector3 cameraLastPosition;
    protected Transform cameraLastTarget;

    // Start is called before the first frame update
    void Start()
    {
        if (target) _cameraOffset = this.transform.position - target.transform.position;
        this.transform.LookAt(target.transform);
    }

    // Update is called once per frame
    void Update()
    {  
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _canRotate = !_canRotate; // *** LINK THIS TO THE UI BUTTON***
        }
    }

    #region for tracking object fall
    //Used to update the camera target once the fall has ended
    public void UpdateOffset()
    {
         _cameraOffset = this.transform.position - target.transform.position;
    }

    public void UpdateFocusPoint() 
    {
        this.transform.LookAt(target.transform);
    }
    #endregion

    private void LateUpdate()
    {
        if (_canRotate) 
        {
            if (target)
            {
                // *** REPLACE MOUSE INPUT WITH MOBILE TOUCH INPUT HERE ***
                Quaternion cameraTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * _rotationSpeed, Vector3.up);
                _cameraOffset = cameraTurnAngle * _cameraOffset;
            }

            Vector3 cameraNewPosition = target.transform.position + _cameraOffset;

            this.transform.position = Vector3.Slerp(this.transform.position, cameraNewPosition, 0.5f);

            this.transform.LookAt(target.transform);
        }
    }

    public void SetFocusPoint(Transform target) 
    {
        this.target = target;
    }

    public Transform GetFocusPoint() 
    {
        return this.target;
    }

    public void SetOffset(Vector3 offset)
    {
        _cameraOffset = offset;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(8, 8, 1024, 32), "Press TAB to toggle between the camera mode and the sticker mode");
        GUI.Label(new Rect(8, 24, 1024, 32), "Current mode: " + (_canRotate ? "Camera" : "Sticker"));
    }

    // Should only be called once
    public void MoveToPosition(Vector3 targetPosition)
    {
        cameraLastPosition = this.transform.position;
        cameraLastTarget = target;
        StartCoroutine(MoveWithFocus(targetPosition));
    }

    private IEnumerator MoveWithFocus(Vector3 targetPosition)
    {
        while (this.transform.position != targetPosition)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, 20f * Time.deltaTime);
            UpdateFocusPoint();
            yield return null;
        }
    }

    // Reset to the last position
    public void MoveBack()
    {
        SetFocusPoint(cameraLastTarget);
        MoveToPosition(cameraLastPosition);
    }
}
