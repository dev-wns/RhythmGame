using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [Header("FPS Counter")]
    private float deltaTime;

    public TextMeshProUGUI text;

    private void Awake()
    {
        StartCoroutine( UpdateFrame() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        deltaTime += ( Time.unscaledDeltaTime - deltaTime ) * .1f;
    }

    private IEnumerator UpdateFrame()
    {
        while( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );
            text.text = $"{( int )( 1f / deltaTime )}";
        }
    }
}
