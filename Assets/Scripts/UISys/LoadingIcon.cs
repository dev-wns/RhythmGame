using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingIcon : MonoBehaviour
{
    public GameObject icon;
    public TextMeshProUGUI loadingText;

    public bool hasText = true;
    private string[] textList = new string[] { "로딩중 ", "로딩중 .", "로딩중 ..", "로딩중 ..." };

    public float rotateSpeed = 100f;

    private void Awake()
    {
        InGame scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnLoadEnd += IconDisable;

        //icon?.SetActive( false );
        //loadingText?.gameObject.SetActive( false );
    }

    private void Start()
    {
        StartCoroutine( IconRotate() );

        if ( hasText )
             StartCoroutine( ChangeText() );
    }

    private void IconDisable()
    {
        StopAllCoroutines();

        icon.SetActive( false );
        loadingText.gameObject.SetActive( false );

        Destroy( this );
    }

    private IEnumerator ChangeText()
    {
        int curIndex = 0;
        loadingText.gameObject.SetActive( true );
        loadingText.text = textList[curIndex];

        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .25f );

            if ( ++curIndex >= textList.Length )
                 curIndex = 0;

            loadingText.text = textList[curIndex];
        }
    }

    private IEnumerator IconRotate()
    {
        float curValue = 0f;
        icon.SetActive( true );

        while ( true )
        {
            curValue -= Time.deltaTime * rotateSpeed;
            icon.transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, curValue ) );
            yield return null;
        }
    }
}
