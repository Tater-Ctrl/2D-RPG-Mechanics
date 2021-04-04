using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fishing : MonoBehaviour {
    public GameObject FishingRod;
    public GameObject fishThrowBar;
    public GameObject fishFillBar;
    Vector3Int playerPosition;
    Vector3Int playerRotation;
    public bool currentlyFishing;
    public bool fishHooked;
    public float fillerTime;
    bool startedFishing;
    GameObject newFloat;
    public int overUI;

    void FishingEvent() {
        playerPosition = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y + 0.5f), Mathf.RoundToInt(transform.position.z));
        playerRotation = new Vector3Int(Mathf.RoundToInt(playerDirection.x), Mathf.RoundToInt(playerDirection.y), Mathf.RoundToInt(playerDirection.z));
        tileBase = tilemap.GetTile(playerPosition);
        tileCollider = tilemapCollider.GetTile(playerPosition + playerRotation * 3);
        if (AvailableItems.instance.inventorySlots[index].FishingRod) {
            FishingRod.SetActive(true);
            if (mouseLeftDown) {
                overUI = AvailableItems.mouseOverUI;
                mouseLeftDown = !mouseLeftDown;
            }
            if (mouseLeft && !currentlyFishing && !fishHooked && overUI == 0) {
                canMove = false;
                hotBar.disableScrollingIndex = true;
                startedFishing = true;
                fishThrowBar.SetActive(true);
                fillerTime += Time.deltaTime;
                FishingRod.GetComponent<EquipmentSprite>().enabled = false;
                float myTime = Mathf.PingPong(fillerTime, 1);
                fishFillBar.GetComponent<Image>().fillAmount = myTime;
                if (playerDirection == new Vector3(0f, -1f, 0f)) {
                    anim.Play("ChargeFishingDown", -1, fillerTime / 2 + 0.1f);
                } else if (playerDirection == new Vector3(0f, 1f, 0f)) {
                    anim.Play("ChargeFishingUp", -1, fillerTime / 2 + 0.1f);
                } else if (playerDirection == new Vector3(1f, 0f, 0f)) {
                    anim.Play("ChargeFishingRight", -1, fillerTime / 2 + 0.1f);
                } else if (playerDirection == new Vector3(-1f, 0f, 0f)) {
                    anim.Play("ChargeFishingLeft", -1, fillerTime / 2 + 0.1f);
                }
            }

            if (!mouseLeft && startedFishing == true && !currentlyFishing) {
                if (fishFillBar.GetComponent<Image>().fillAmount < 0.4f) {
                    StopFishing();
                } else {

                    newFloat = Instantiate(fishFloat, GameObject.Find("Player_Sprite").transform.position + playerDirection, Quaternion.identity, this.gameObject.transform);
                    newFloat.GetComponent<FishingFloat>().ThrowDistance = fishFillBar.GetComponent<Image>().fillAmount;
                    hotBar.disableScrollingIndex = true;
                    fillerTime = 0;
                    AvailableItems.instance.DamageDurability();
                    currentlyFishing = true;
                }
                StartCoroutine(RemoveFishThrowBar(1.5f));
            }
            if (currentlyFishing) {
                if (playerDirection == new Vector3(0f, -1f, 0f)) {
                    anim.Play("IdleFishingDown");
                } else if (playerDirection == new Vector3(0f, 1f, 0f)) {
                    anim.Play("IdleFishingUp");
                } else if (playerDirection == new Vector3(1f, 0f, 0f)) {
                    anim.Play("IdleFishingRight");
                } else if (playerDirection == new Vector3(-1f, 0f, 0f)) {
                    anim.Play("IdleFishingLeft");
                }
                if (mouseRight || Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) {
                    StopFishing();
                }
            }
        } else {
            FishingRod.SetActive(false);
        }
    }
    public void StopFishing() {
        if (fishHooked) {
            playerBox.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
            fishCooldown = true;
            StartCoroutine(CloseFishingHUD(1.5f));
            StartCoroutine(FishCooldown(3));
        }
        if (newFloat != null) {
            newFloat.GetComponent<FishingFloat>().DestroyItem();
            newFloat = null;
        }
        canMove = true;
        startedFishing = false;
        currentlyFishing = false;
        fishHooked = false;
        fishThrowBar.SetActive(false);
        hotBar.disableScrollingIndex = false;
        fillerTime = 0;
    }
    IEnumerator CloseFishingHUD(float time) {
        yield return new WaitForSeconds(time);
        fishBar.fillAmount = 0.4f;
        playerBox.GetComponent<Rigidbody2D>().constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        playerBox.transform.localPosition = new Vector3(0, 0, 0);
        fishBox.transform.localPosition = new Vector3(0, 0, 0);
        fishing_HUD.SetActive(false);
    }
    IEnumerator RemoveFishThrowBar(float time) {
        yield return new WaitForSeconds(time);
        fishThrowBar.SetActive(false);
    }
    void FishingEventStart() {
        //Started from Fishing Float Script
        canMove = false;
        randomNumber = Random.Range(0f, 2f);
        if (fishHooked) {
            fishing_HUD.SetActive(true);
            Vector3 random = new Vector3(0, fishPosition, 0);
            Vector3 current = fishBox.localPosition;
            fishBox.localPosition = Vector3.Lerp(current, random, 0.04f);
        }

        if (mouseLeft && fishHooked) {
            playerBox.GetComponent<Rigidbody2D>().AddForce(transform.up * 850);
        }
        if (fishOn) {
            fishBar.fillAmount += 0.15f * Time.fixedDeltaTime;
        } else if (!fishOn) {
            fishBar.fillAmount -= 0.15f * Time.fixedDeltaTime;
        }
        if (fishBar.fillAmount >= 1) {
            //If you win fishing event
            //Add fishing experience for levels later
            hotBar.AddFish();
            StopFishing();
        } else if (fishBar.fillAmount <= 0) {
            //If lose you fishing event
            //lose fishing experience when failing?
            StopFishing();
        }

        if (playerRotation.x < 0) {
            fishing_HUD.GetComponent<RectTransform>().localPosition = new Vector3(300f, 0f, 0f);

        } else {
            fishing_HUD.GetComponent<RectTransform>().localPosition = new Vector3(-300f, 0f, 0f);
        }
        if (fishBar.fillAmount < 0) {
            fishBar.fillAmount = 0;
        } else if (fishBar.fillAmount > 1) {
            fishBar.fillAmount = 1;
        }
    }
    IEnumerator FishCooldown(float time) {
        yield return new WaitForSeconds(time);
        fishCooldown = false;
    }
    IEnumerator FishMovement() {
        while (true) {
            yield return new WaitForSeconds(randomNumber);
            fishPosition = Random.Range(-165, 165);
        }
    }
}