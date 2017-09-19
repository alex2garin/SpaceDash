﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour {
    
    public float explosionLengthTime = 1f;
    public float destroyDelayTime = 0.5f;
    public bool isActive;
    

//    private SpriteRenderer sr;
    private Animator animator;

    private Rigidbody2D rb2D;
    private BoxCollider2D BC2D;
    private MovingObjectController MOC;

    // Use this for initialization
    void Awake () {
        //        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        //animator.SetBool("isActive", isActive);
        rb2D = GetComponent<Rigidbody2D>();
        BC2D = GetComponent<BoxCollider2D>();
        MOC = GetComponent<MovingObjectController>();

        //Debug.Log(rb2D);
    }

    public void Planted()
    {
        MOC.enabled = false;
        rb2D.Sleep();
        BC2D.enabled = false;
    }

    public void Activate()
    {
        rb2D.WakeUp();
        BC2D.enabled = true;
        MOC.enabled = true;
    }

    public void ReadyToExplode()
    {
//        Debug.Log("ready to explode is called");
        StartCoroutine(AnimationDelay());
    }

    private IEnumerator AnimationDelay()
    {
        animator.SetTrigger("Explode");

        yield return new WaitForSeconds(explosionLengthTime);
        Explode();

        yield return new WaitForSeconds(destroyDelayTime - explosionLengthTime);
        Destroy(gameObject);
    }
        
	
    private void Explode()
    {
        
        Collider2D[] surround = Physics2D.OverlapBoxAll(transform.position, new Vector2(2f, 2f), 0f);
        foreach (var objectCollider2D in surround)
        {

            if (objectCollider2D.gameObject.CompareTag("Bomb") && objectCollider2D.gameObject != gameObject) objectCollider2D.gameObject.GetComponent<BombController>().ReadyToExplode();
            if (objectCollider2D.gameObject.CompareTag("Bomb")) continue;

            DestroyByExplosion objectToDestroy = objectCollider2D.gameObject.GetComponent<DestroyByExplosion>();
            if (objectToDestroy != null) objectToDestroy.ToDestroyByExplosion();
        }
        var neighbours = Physics2D.OverlapBoxAll(transform.position, new Vector2(4f, 4f), 0f);
        foreach (var neighbour in neighbours)
        {
            CornersAndBordersController cabc = neighbour.gameObject.GetComponent<CornersAndBordersController>();
            if (cabc != null) cabc.needToUpdate = true;
        }

    }

    //public void SetActive( bool isActiveFlag)
    //{
    //    if (isActiveFlag) sr.sprite = activeBombSprite;
    //    else sr.sprite = deactiveBombSprite;
    //    isActive = isActiveFlag;
    //}
    

  

}
