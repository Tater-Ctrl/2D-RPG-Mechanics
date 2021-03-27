using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    Vector2 moveDir;
    Rigidbody2D rb;
    public float speed = 10f;
    public Tilemap tilemap;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = (transform.up * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;

        if(Input.GetMouseButtonDown(1)) moveUnits();
    }

    void FixedUpdate() {
        rb.velocity = moveDir * speed;
    }

    void moveUnits() {
        Vector3 target = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        Vector3Int cellPos = GetWorldTileByCellPosition(target);

        GetDistance(cellPos);
    }

    Vector3Int GetWorldTileByCellPosition(Vector3 worldPosition) {
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        if(MapVariables.noiseMap[cellPos.x, cellPos.y] > MapVariables.regions[1].height && MapVariables.noiseMap[cellPos.x, cellPos.y] < MapVariables.regions[6].height) {
            return cellPos;
        } else {
            return Vector3Int.zero;
        }
    }

    int GetDistance(Vector3Int target) {
        int distX = Mathf.Abs(Mathf.FloorToInt(transform.position.x) - target.x);
        int distY = Mathf.Abs(Mathf.FloorToInt(transform.position.y) - target.y);

        if(distX > distY) {
            return 14 * distY + 10 * (distX - distY);
        } else {
            return 14 * distX + 10 * (distY - distX);
        }
    }
}
