using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_EnableTickImages : MonoBehaviour
{
    public GameObject tick1, tick2, tick3;
    [SerializeField] private int _poofSpeedMultiplier = 2;

    void Update()
    {
        if(!tick1.activeSelf)
        {
            if(Input.GetKeyDown("1"))
            {
                tick1.SetActive(true);
                StartCoroutine("Poof", tick1);
            }
        }
        if(!tick2.activeSelf)
        {
            if(Input.GetKeyDown("2"))
            {
                 tick2.SetActive(true);
                 StartCoroutine("Poof", tick2);
            }
        }

        if(!tick3.activeSelf)
        {
            if(Input.GetKeyDown("3"))
            {
                tick3.SetActive(true);
                StartCoroutine("Poof", tick3);
            }
        }
    }

     private IEnumerator Poof(GameObject go)
    {
        // Record the original & target scale for x & y
        float rawScaleX = go.transform.localScale.x;
        float rawScaleY = go.transform.localScale.y;

        float targetScaleX = rawScaleX * 0.75f;
        float targetScaleY = rawScaleY * 0.75f;

        // Speed based on the scale
        float poofSpeedX = 1f * _poofSpeedMultiplier;
        float poofSpeedY = poofSpeedX * rawScaleY;

        // Scale down
        while(go.transform.localScale.x > targetScaleX && go.transform.localScale.y > targetScaleY)
        {
            go.transform.localScale = new Vector3(go.transform.localScale.x - poofSpeedX * Time.deltaTime, go.transform.localScale.y - poofSpeedY * Time.deltaTime, go.transform.localScale.z);
            yield return null;
        }

        go.transform.localScale = new Vector3(targetScaleX, targetScaleY, go.transform.localScale.z);

        // Scale up again
        while(go.transform.localScale.x < rawScaleX && go.transform.localScale.y < rawScaleY)
        {
            go.transform.localScale = new Vector3(go.transform.localScale.x + poofSpeedX * Time.deltaTime, go.transform.localScale.y + poofSpeedY * Time.deltaTime, go.transform.localScale.z);
            yield return null;
        }

        go.transform.localScale = new Vector3(rawScaleX, rawScaleY, go.transform.localScale.z);
    }
}

