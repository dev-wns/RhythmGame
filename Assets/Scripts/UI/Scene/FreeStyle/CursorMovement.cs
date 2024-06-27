using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorMovement : MonoBehaviour
{
    public Camera uiCamera;

    private void Update()
    {
        Vector3 pos = Input.mousePosition;
        transform.position = uiCamera.ScreenToWorldPoint( new Vector3( pos.x, pos.y, 10f ) );
    }
}
