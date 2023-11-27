using UnityEngine;

public class RotateImage : MonoBehaviour
{
    public GameObject icon;
    public float rotateSpeed = 100f;
    private float curValue;

    private void Awake()
    {
        icon.SetActive( true );
    }

    void Update()
    {
        curValue -= Time.deltaTime * rotateSpeed;
        icon.transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, curValue ) );
    }
}
