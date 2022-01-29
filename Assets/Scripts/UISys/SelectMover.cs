using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMover : MonoBehaviour
{
    public Transform left, right;
    public float speed = 10f;
    public float moveAmount = 30f;

    // Init Pos
    private Vector2 leftPos, rightPos;
    private bool isReverse = true;
    private float curValue;

    private void Awake()
    {
        leftPos  = left.position;
        rightPos = right.position;
    }

    //private void Update()
    //{
    //    float deltaTime = speed * Time.deltaTime;
    //    curValue += isReverse ? deltaTime : -deltaTime;
    //    isReverse = curValue <= 0f || curValue >= moveAmount ? !isReverse : isReverse;


    //    left.position = new Vector2( leftPos.x - curValue, left.transform.position.y );
    //    right.position = new Vector2( rightPos.x + curValue, right.transform.position.y );
    //}
}
