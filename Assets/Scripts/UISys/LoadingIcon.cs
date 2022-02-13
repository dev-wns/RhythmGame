using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingIcon : MonoBehaviour
{
    public RectTransform iconRt;
    public List<GameObject> dotList = new List<GameObject>();

    public float rotateSpeed = 100f;
    private float rotateValue = 0f;

    private void Awake()
    {
        StartCoroutine( Loading() );
    }

    private void AllActive( bool _isActive )
    {
        for ( int i = 0; i < dotList.Count; i++ )
        {
            dotList[i].SetActive( _isActive );
        }
    }

    private IEnumerator Loading()
    {
        int curIndex = 0;
        GameObject curText;
        if ( dotList.Count > 0 )
        {
            curText = dotList[curIndex];
        }
        else
        {
            gameObject.SetActive( false );
            yield break;
        }

        while ( true )
        {
            if ( curIndex == 0 )
                 yield return YieldCache.WaitForSeconds( .25f );
            
            curText.SetActive( true );
            yield return YieldCache.WaitForSeconds( .25f );

            if ( ++curIndex >= dotList.Count )
            {
                AllActive( false );
                curIndex = 0;
            }

            curText = dotList[curIndex];
        }
    }

    private void Update()
    {
        rotateValue += Time.deltaTime * rotateSpeed;
        iconRt.rotation = Quaternion.Euler( new Vector3( 0f, 0f, rotateValue ) );
    }
}
