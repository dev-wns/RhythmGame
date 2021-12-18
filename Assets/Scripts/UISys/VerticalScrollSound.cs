using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VerticalScrollSound : MonoBehaviour
{
    public GameObject songPrefab; // sound infomation prefab
    private RectTransform rt;
    private RectTransform viewport;
    private List<RectTransform> contents = new List<RectTransform>();

    private float curPos, moveOffset;
    private int curIndex, minIndex, maxIndex;

    public bool IsDuplicate { get; private set; }

    public int maxShowContentsCount = 3;
    //public int startContent = 0;
    public int spacing = 0;
    public int numExtraEnable = 2;

    public void Awake()
    {
        rt = GetComponent<RectTransform>();
        viewport = transform.parent as RectTransform;
        rt.anchorMin = new Vector2( 0, 1 );
        rt.anchorMax = new Vector2( 0, 1 );

        minIndex = Mathf.FloorToInt( maxShowContentsCount * .5f );
        maxIndex = GameManager.Songs.Count - minIndex - 1;
        curIndex = GameManager.CurrentSoundIndex;

        // Create Scroll Contents
        contents.Capacity = GameManager.Songs.Count;
        for ( int i = 0; i < GameManager.Songs.Count; i++ )
        {
            // scrollview song contents
            GameObject obj = Instantiate( songPrefab, rt );

            // 사운드 이름 설정
            Song data = GameManager.Songs[i];
            System.Text.StringBuilder artist = new System.Text.StringBuilder();
            artist.Capacity = data.artist.Length + 8 + data.creator.Length;
            artist.Append( data.artist ).Append( " // " ).Append( data.creator );

            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();
            info[0].text = data.title;
            info[1].text = data.version; 
            info[2].text = artist.ToString();

            // 객체 위치 설정
            RectTransform dataTransform = obj.transform as RectTransform;
            float height = dataTransform.sizeDelta.y;
            dataTransform.anchoredPosition = new Vector2( 0, ( ( height + spacing ) * minIndex ) - ( ( height + spacing ) * i ) );

            // 화면에 그려지는 객체만 활성화
            if ( GameManager.CurrentSoundIndex - minIndex <= i && GameManager.CurrentSoundIndex + minIndex >= i )
                 dataTransform.gameObject.SetActive( true );
            else dataTransform.gameObject.SetActive( false );

            contents.Add( dataTransform );
        }

        // 보여줄 객체가 짝수면 홀수인것처럼 중간에 자리 잡도록 설정
        RectTransform prefabRT = songPrefab.transform as RectTransform;
        if ( maxShowContentsCount % 2 == 0 )
        {
            rt.sizeDelta       = new Vector2( prefabRT.sizeDelta.x, ( maxShowContentsCount + 1 ) * ( prefabRT.sizeDelta.y + spacing ) );
            viewport.sizeDelta = new Vector2( prefabRT.sizeDelta.x * 1.1f, maxShowContentsCount * ( prefabRT.sizeDelta.y + spacing ) );
        }
        else
        {
            rt.sizeDelta       = new Vector2( prefabRT.sizeDelta.x, maxShowContentsCount * ( prefabRT.sizeDelta.y + spacing ) );
            viewport.sizeDelta = new Vector2( prefabRT.sizeDelta.x * 1.1f, maxShowContentsCount * ( prefabRT.sizeDelta.y + spacing ) );
        }

        moveOffset = prefabRT.rect.height + spacing;
        
        // 시작인덱스 위치로 이동
        curPos = ( GameManager.CurrentSoundIndex - minIndex ) * moveOffset;
        rt.localPosition = new Vector2( rt.localPosition.x, curPos );
    }

    public void PrevMove()
    {
        if ( curIndex == 0 )
        {
            IsDuplicate = true;
            return;
        }

        curPos -= moveOffset;
        rt.DOLocalMoveY( curPos, .5f );
        GameManager.Inst.SelectSong( --curIndex );

        if ( minIndex <= curIndex )
        {
            contents[curIndex - minIndex].gameObject.SetActive( true );
        }

        if ( maxIndex > curIndex + numExtraEnable )
        {
            contents[minIndex + curIndex + numExtraEnable + 1].gameObject.SetActive( false );
        }
        IsDuplicate = false;
    }

    public void NextMove()
    {
        if ( curIndex == contents.Count - 1 )
        {
            IsDuplicate = true;
            return;
        }

        curPos += moveOffset;
        rt.DOLocalMoveY( curPos, .5f );
        GameManager.Inst.SelectSong( ++curIndex );

        if ( maxIndex >= curIndex )
        {
            contents[minIndex + curIndex].gameObject.SetActive( true );
        }

        if ( minIndex < curIndex - numExtraEnable )
        {
            contents[curIndex - minIndex - numExtraEnable - 1].gameObject.SetActive( false );
        }
        IsDuplicate = false;
    }
}
