using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrop : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _isGrounded = false;
    [SerializeField] [Range(1, 10)] private float _fallSpeedMultiplier = 3f;
    public CameraControl cameraControl;
    private bool cameraMoved = false;
    public GameObject dustEffect;
    private bool spawnedDust;
    private LevelManager _levelManager;

    void Awake()
    {
        _rb = this.gameObject.GetComponent<Rigidbody>();
        _levelManager = FindObjectOfType<LevelManager>();
    }

    void Update()
    {
        if(!_isGrounded)
        {
            Drop();
            _isGrounded = IsGrounded();
            //Debug.Log(_isGrounded + "    "  + this.transform.rotation.x);
        }
        else if(_rb != null)
        {
            //Destroy(_rb);
            Destroy(_rb, 1.5f); // Destroy after 3s(or more?) to make sure the bounce is done
            cameraControl.UpdateOffset();
            
            if(!spawnedDust)
            {
                Instantiate(dustEffect, this.gameObject.transform.position, this.gameObject.transform.rotation);
                spawnedDust = true;
            }

            if (_levelManager && _levelManager.focusOnTargetOnly && !cameraMoved) 
            {
                Transform objectFocusPoint = cameraControl.GetFocusPoint();

                // Place the camera in front of the target sticker (5 units away)
                cameraControl.MoveToPosition(objectFocusPoint.position - objectFocusPoint.forward * _levelManager.focusDistance);

                cameraMoved = true;
            }
        }
    }
    bool IsGrounded()
    {
        // Check if it touches the ground and stops moving (0.01f is the threshold) - this may not work, the moment when it collides with the floor, its angular velocity, velocity.x and velocity.z could be zero
         return Physics.Raycast(transform.position, - Vector3.up,0.1f) && (_rb.velocity.x <= 0.01f) && (_rb.velocity.z <= 0.01f);

        // The rigidbody component should be destroyed once the physical bounce is done
        // Change vector3.up to transform.up (but this depends on the model itself)?
    }

    void Drop()
    {
        //print("Hit");
           _rb.velocity += Vector3.up * Physics.gravity.y * (_fallSpeedMultiplier - 1) * Time.deltaTime;
           cameraControl.UpdateFocusPoint();
    }
}
