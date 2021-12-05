using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    private Vector3 originPosition;
    public float amount = 200;
    private float time = 0;

    private void Awake()
    {
        originPosition = transform.position;
        StartCoroutine( Shake() );
    }

    private IEnumerator Shake()
    {
        while ( true )
        {
            yield return null;
            time = 0;
            float value = AudioVisualizer.bassAmount * amount;
            if ( value >= 10f )
            {
                while ( time < .1f )
                {
                    Vector3 newPos = Random.insideUnitCircle * value / 3f;
                    transform.position = new Vector3( newPos.x - 1, newPos.y - 1, -10 );
                    time += Time.deltaTime;
                    yield return null;
                }
            }

            transform.position = originPosition;
        }
    }
}