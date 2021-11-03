using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoofDestroySelf : MonoBehaviour
{
    
    void Start()
    {
         StartCoroutine("Destroy");
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }
}
