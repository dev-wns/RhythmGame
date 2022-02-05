using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageRotater : MonoBehaviour
{
    public RectTransform image;
    public float speed = -10;

    private float value;

    private void Update()
    {
        value += Time.deltaTime * speed;
        image.rotation = Quaternion.Euler( new Vector3( 0f, 0f, value ) );
    }
}
