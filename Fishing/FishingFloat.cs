using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishingFloat : MonoBehaviour
{
    public Tilemap tilemap;
    public float ThrowDistance;
    Rigidbody2D rb;
    LineRenderer line;
    public Sprite[] sprite;
    SpriteRenderer currentSprite;

    // Start is called before the first frame update
    void Start()
    {
        currentSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        line.SetPosition(1, this.transform.position);
        line.SetPosition(0, this.transform.position + new Vector3(0,0.2f,0));
        Vector3 player = GameObject.Find("Player").GetComponent<Player>().playerDirection;
        if(player == new Vector3(0f,-1f,0f))
        {
            line.SetPosition(1, GameObject.Find("Player_Sprite").transform.position + new Vector3(0,1.3f,0));
            line.sortingOrder = 6;
        } else if(player == new Vector3(0f,1f,0f))
        {
            line.SetPosition(1, GameObject.Find("Player_Sprite").transform.position + new Vector3(0,1.4f,0));
            line.sortingOrder = 4;
        } else if(player == new Vector3(1f,0f,0f))
        {
            line.SetPosition(1, GameObject.Find("Player_Sprite").transform.position + new Vector3(1f,1.4f,0f));
            line.sortingOrder = 4;
        } else if(player == new Vector3(-1f,0f,0f))
        {
            line.SetPosition(1, GameObject.Find("Player_Sprite").transform.position + new Vector3(-1f,1.4f,0f));
            line.sortingOrder = 4;
        }
        //Spawn on player and be thrown multipled by fillbar
        PushAway();
        StartCoroutine(CheckBeneath(0.7f));
    }
    void Awake()
    {
        StartCoroutine(WaitingForBite(3f));
    }

    void Update()
    {
        line.SetPosition(0, this.transform.position + new Vector3(0,0.2f,0));
    }
    void PushAway()
    {
        tilemap = GameObject.Find("Player").GetComponent<Player>().tilemapCollider;
        Vector3 playerPosition = GameObject.Find("Player").GetComponent<Player>().playerDirection;
        float pushForce = GameObject.Find("Player").GetComponent<Player>().fillerTime;
        rb.AddForce(playerPosition * 1650 * ThrowDistance);
    }
    public int chanceToBite;
    IEnumerator WaitingForBite(float time)
    {
        while(true){
            yield return new WaitForSeconds(time);
            if (chanceToBite > Random.Range(0f,100f))
            {
                GameObject.Find("Player").GetComponent<Player>().fishHooked = true;
            }
        }
    }

    public void DestroyItem()
    {
        Destroy(this.gameObject);
    }
    IEnumerator CheckBeneath(float time)
    {
        //check if water is beneath
        yield return new WaitForSeconds(time);
        Vector3Int position = new Vector3Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.y), Mathf.RoundToInt(this.transform.position.z));
        TileBase tile = tilemap.GetTile(position);
        if(tile != null)
        {
            if(tile.name == "Ground TileSheet_4")
            {
                currentSprite.sprite = sprite[1];
            } else {
                Destroy(this.gameObject);
                GameObject.Find("Player").GetComponent<Player>().StopFishing();
            }
        } else {
                Destroy(this.gameObject);
                GameObject.Find("Player").GetComponent<Player>().StopFishing();            
        }
    }
}
