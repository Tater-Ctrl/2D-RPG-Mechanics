using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishMiniGame : MonoBehaviour
{
    // Start is called before the first frame update
    Collider2D col;
    void Start()
    {
        col = GetComponent<BoxCollider2D>();
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "fishingHitbox")
         {
            GameObject.Find("Player").GetComponent<Player>().fishOn = true;
         }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.tag == "fishingHitbox")
        {
           GameObject.Find("Player").GetComponent<Player>().fishOn = false;
        }
    }
}
