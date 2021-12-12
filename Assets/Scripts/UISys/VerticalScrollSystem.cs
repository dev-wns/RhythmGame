using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class VerticalScrollSystem : MonoBehaviour
{
    private RectTransform rt;
    private RectTransform viewport;
    private RectTransform[] contents;

    private float curPos, moveOffset;
    private int curIndex, minIndex, maxIndex;

    public GameObject curObject { get; private set; }
    public bool IsDuplicate { get; private set; }

    public int maxShowContentsCount = 3;
    public int startContent = 0;
    public int spacing      = 0;
    public int numExtraEnable = 2;

    public delegate void Callback();
    public event Callback OnInitialize;

    private void Start()
    {
        rt       = GetComponent<RectTransform>();
        viewport = transform.parent as RectTransform;
        rt.anchorMin = new Vector2( 0, 1 );
        rt.anchorMax = new Vector2( 0, 1 );

        minIndex = Mathf.FloorToInt( maxShowContentsCount * .5f );
        maxIndex = rt.childCount - minIndex - 1;
        curIndex = startContent;

        contents = new RectTransform[rt.childCount];
        for ( int i = 0; i < rt.childCount; i++ )
        {
            contents[i] = rt.GetChild( i ) as RectTransform;
            //contents[i].GetChild( 0 ).GetComponent<TextMeshProUGUI>().text = i.ToString();
            contents[i].gameObject.name = i.ToString();

            float height = contents[i].sizeDelta.y;
            contents[i].anchoredPosition = new Vector2( 0, ( ( height + spacing ) * minIndex ) - ( ( height + spacing ) * i ) );

            if ( startContent - minIndex <= i && startContent + minIndex >= i )
            {
                contents[i].gameObject.SetActive( true );
            }
            else contents[i].gameObject.SetActive( false );
        }
        curObject = contents[curIndex].gameObject;

        RectTransform childRT = curObject.transform as RectTransform;
        if ( maxShowContentsCount % 2 == 0 )
        {
            rt.sizeDelta       = new Vector2( childRT.sizeDelta.x,      ( maxShowContentsCount + 1 ) * ( childRT.sizeDelta.y + spacing ) );
            viewport.sizeDelta = new Vector2( childRT.sizeDelta.x * 1.1f, maxShowContentsCount * ( childRT.sizeDelta.y + spacing ) );
        }
        else
        {
            rt.sizeDelta       = new Vector2( childRT.sizeDelta.x,        maxShowContentsCount * ( childRT.sizeDelta.y + spacing ) );
            viewport.sizeDelta = new Vector2( childRT.sizeDelta.x * 1.1f, maxShowContentsCount * ( childRT.sizeDelta.y + spacing ) );
        }

        moveOffset = childRT.rect.height + spacing;
        curPos     = ( startContent - minIndex ) * moveOffset;

        rt.localPosition = new Vector2( rt.localPosition.x, curPos );
        ( curObject.transform as RectTransform ).DOScale( new Vector2( 1.1f, 1.1f ), .5f );

        OnInitialize();
    }

    public void PrevMove()
    {
        if ( curPos <= -( moveOffset * minIndex ) )
        {
            IsDuplicate = true;
            return;
        }

        ( curObject.transform as RectTransform ).DOScale( Vector2.one, .5f );

        curPos -= moveOffset;
        rt.DOLocalMoveY( curPos, .5f );
        curObject = contents[--curIndex].gameObject;

        ( curObject.transform as RectTransform ).DOScale( new Vector2( 1.1f, 1.1f ), .5f );

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
        if ( curPos >= moveOffset * maxIndex )
        {
            IsDuplicate = true;
            return;
        }

        ( curObject.transform as RectTransform ).DOScale( Vector2.one, .5f );

        curPos += moveOffset;
        rt.DOLocalMoveY( curPos, .5f );
        curObject = contents[++curIndex].gameObject;

        ( curObject.transform as RectTransform ).DOScale( new Vector2( 1.1f, 1.1f ), .5f );

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
