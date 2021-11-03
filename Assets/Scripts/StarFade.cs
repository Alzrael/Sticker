using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarFade : MonoBehaviour
{
    public int starOrder;
    public int _poofSpeedMultiplier = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            StartCoroutine("Poof");
        }
    }


    private IEnumerator Poof()
    {
        // Record the original & target scale for x & y
        float rawScaleX = 1.0f;
        float rawScaleY = 1.0f;

        float targetScaleX = rawScaleX * 0.75f;
        float targetScaleY = rawScaleY * 0.75f;

        // Speed based on the scale
        float poofSpeedX = 3f * _poofSpeedMultiplier;
        float poofSpeedY = poofSpeedX * rawScaleY;

        yield return new WaitForSeconds(starOrder*.35f);

        // Scale up again
        while(transform.localScale.x < rawScaleX && transform.localScale.y < rawScaleY)
        {
            transform.localScale = new Vector3(transform.localScale.x + poofSpeedX * Time.deltaTime, transform.localScale.y + poofSpeedY * Time.deltaTime, transform.localScale.z);
            yield return null;
        }

        

        transform.localScale = new Vector3(targetScaleX, targetScaleY, transform.localScale.z);

        

        transform.localScale = new Vector3(rawScaleX, rawScaleY, transform.localScale.z);
    }

}
