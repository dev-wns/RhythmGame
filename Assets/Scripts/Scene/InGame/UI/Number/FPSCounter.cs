using System.Collections;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Header("FPS Counter")]
    private float deltaTime;

    public TextMeshProUGUI text;

    private void Awake()
    {
        text.SetText( "{0}", 10000 ); // capacity
        text.SetText( "{0}", 0 );
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
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );
            text.SetText( "{0}", ( int )( 1f / deltaTime ) );
        }
    }
}
