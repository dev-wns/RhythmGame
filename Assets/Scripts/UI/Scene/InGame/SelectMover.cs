using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMover : MonoBehaviour
{
    public Transform left, right;
    public float speed = 10f;
    public float moveAmount = 30f;

    private float timer;

    // Init Pos
    private Vector2 leftPos, rightPos;

    private void Awake()
    {
        leftPos  = left.position;
        rightPos = right.position;
    }

    private void Update()
    {
        timer += speed * Time.deltaTime;
        float offset = ( Mathf.Cos( timer ) + 1f ) * .5f; // 0 ~ 1
        left.position  = new Vector2( leftPos.x  - ( moveAmount * offset ), left.transform.position.y  );
        right.position = new Vector2( rightPos.x + ( moveAmount * offset ), right.transform.position.y );
    }
}
