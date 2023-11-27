using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingText : RotateImage
{
    public TextMeshProUGUI text;
    private static string[] textList = new string[] { "로딩중 ", "로딩중 .", "로딩중 ..", "로딩중 ..." };

    private void Awake()
    {
        InGame scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnLoadEnd += IconDisable;
        transform.position = new Vector3( transform.position.x + GameSetting.GearOffsetX, transform.position.y, transform.position.z );
    }

    private void Start()
    {
        if ( !ReferenceEquals( text, null ) )
             StartCoroutine( ChangeText() );
    }

    private void IconDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ChangeText()
    {
        int curIndex = 0;
        text.gameObject.SetActive( true );
        text.text = textList[curIndex];

        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .25f );

            if ( ++curIndex >= textList.Length )
                 curIndex = 0;

            text.text = textList[curIndex];
        }
    }
}
