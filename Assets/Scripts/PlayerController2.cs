﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController2 : MonoBehaviour
{

    public float moveTime = .1f;
    public LayerMask blockingLayer;
    public LayerMask pickUpLayer;
    public int secondsForBallon = 60;
    public int startingSecondsOfOxygen = 60;
    public GameObject bombGO;


    private Rigidbody2D rb2D;
    private Collider2D cc2D;
    private float inverseMoveTime;
    private bool isMoving;

    private int bombs = 0;
    private int crystals = 0;
    private int minerals = 0;
    private int oxygen = 0;
    

  //  private Vector3 newDirection;

    private Text crystalText;
    private Text bombText;
    private Text mineralText;
    private Text oxygenText;

    private GameObject plantedBomb;

    private Vector3 end;
    private Vector3 currDirection;
    private Vector3 newDirection;

    // Use this for initialization
    void Start()
    {

        oxygen = startingSecondsOfOxygen;

        inverseMoveTime = 1f / moveTime;
        rb2D = GetComponent<Rigidbody2D>();
        cc2D = GetComponent<Collider2D>();
        isMoving = false;
        crystalText = GameObject.Find("Crystals").GetComponent<Text>();
        mineralText = GameObject.Find("Minerals").GetComponent<Text>();
        bombText = GameObject.Find("Bombs").GetComponent<Text>();
        oxygenText = GameObject.Find("Oxygen").GetComponent<Text>();

        StartCoroutine(OxygenFlow());
    }

    private bool switcher = false;

    private void SetBomb()
    {
        if (bombs > 0 && plantedBomb == null)
        {

            plantedBomb = Instantiate(bombGO, transform.position, Quaternion.identity);
            plantedBomb.GetComponent<BombController>().Planted();
            //            plantedBomb.SetActive(false);

            PickUpBomb(true);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (plantedBomb != null)
        {
            //Debug.Log((plantedBomb.transform.position - transform.position).sqrMagnitude);
            if ((plantedBomb.transform.position - transform.position).sqrMagnitude >= 1)
            {
                plantedBomb.GetComponent<BombController>().Activate();
                plantedBomb = null;
            }
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        if (horizontalInput != 0) newDirection = new Vector2(horizontalInput, 0f);
        else if (verticalInput != 0) newDirection = new Vector2(0f, verticalInput);
        else newDirection = Vector2.zero;

        bool control = Input.GetKey(KeyCode.LeftControl);
        if (switcher != control) switcher = control;

        bool space = Input.GetKey(KeyCode.Space);


        if (isMoving)
        {
            Debug.Log("is moving");
            if (control || (space && bombs > 0)) newDirection = Vector2.zero;

            float sqrRemainingDistance = (end - transform.position).sqrMagnitude;
            if (sqrRemainingDistance > float.Epsilon)
            {
                Vector3 newPostion = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);

                if(newPostion == end && currDirection == newDirection)
                {
                    Collider2D directionObject = Physics2D.OverlapBox(end + newDirection, new Vector2(.9f, .9f), 0);
                    if (directionObject == null || directionObject.gameObject.CompareTag("Ground") || directionObject.gameObject.layer == LayerMask.NameToLayer("PickUp"))
                    {
                        Collider2D directionUp = Physics2D.OverlapBox(end + newDirection + new Vector3(0f, 1f, 0f), new Vector2(.9f, .9f), 0);
                        if (directionUp != null)
                        {
                            MovingObjectController moverUp = directionUp.gameObject.GetComponent<MovingObjectController>();
                            if (moverUp == null || !moverUp.IsMoving)
                            {
                                end += newDirection;
                                newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
                            }
                        }
                        else
                        {
                            end += newDirection;
                            newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
                        }

                    }
                }
                cc2D.offset = (end - transform.position) / 2;
                rb2D.MovePosition(newPostion);
                return;
            }
            cc2D.offset = new Vector2(0f, 0f);
            isMoving = false;
        }
        else
        {
            Debug.Log("is not moving");
            end = newDirection + transform.position;
            currDirection = newDirection;
            
            Collider2D directionObject = Physics2D.OverlapBox(end, new Vector2(.9f, .9f), 0);

            if (directionObject == null)
            {
                Collider2D directionUp = Physics2D.OverlapBox(end + new Vector3(0f, 1f), new Vector2(.9f, .9f), 0);
                if (directionUp != null)
                {
                    MovingObjectController moverUp = directionUp.gameObject.GetComponent<MovingObjectController>();
                    if (moverUp != null && moverUp.IsMoving) return;
                }
            }

            if (directionObject != null && 1 << directionObject.gameObject.layer == blockingLayer.value)
            {

                MovingObjectController MOCStone = directionObject.gameObject.GetComponent<MovingObjectController>();
                if (MOCStone == null) return;

                if (!MOCStone.Push(newDirection)) return;
                else if (control) return;
            }

            if (directionObject != null && 1 << directionObject.gameObject.layer == pickUpLayer.value)
            {
                if (directionObject.gameObject.CompareTag("Ballon")) PickUpOxygen();
                else if (directionObject.gameObject.CompareTag("BombPickUp")) PickUpBomb();
                else if (directionObject.gameObject.CompareTag("Crystal")) PickUpCrystals();
                else if (directionObject.gameObject.CompareTag("Mineral")) PickUpMinerals();

                Destroy(directionObject.gameObject);
                if (control) return;
            }

            if (directionObject != null && directionObject.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Destroy(directionObject.gameObject);
                if (control) return;
            }
            if (!control)
            {

                isMoving = true;
                FixedUpdate();
            }
        }

        
    }
    

    public void PickUpBomb(bool remove = false)
    {
        if (remove) bombs--;
        else bombs++;
        bombText.text = "Bombs: " + bombs;
    }

    public void PickUpMinerals()
    {
        minerals++;
        mineralText.text = "Minerals: " + minerals;
    }

    public void PickUpOxygen()
    {
        oxygen += secondsForBallon;
        oxygenText.text = "Oxygen: " + oxygen;
    }

    public void PickUpCrystals()
    {
        crystals++;
        crystalText.text = "Crystals: " + crystals;
    }

    private IEnumerator OxygenFlow()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            oxygen--;
            oxygenText.text = "Oxygen: " + oxygen;
            if (oxygen <= 0) Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.LoadScene("Menu");
    }
}
