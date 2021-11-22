using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VerticalScrollSnap : MonoBehaviour
{
    private int CenterNumber;
    private Transform[] contents;
    private short selectIdx;
    private float movePos; // position shall be moved
    private RectTransform rectTransform;

    [Tooltip("Prefab Height + Spacing")]
    public float MoveOffset = 105f;
    public int maxShowContentsCount;

    private bool isDuplicateKey = false;
    public bool IsDuplicateKeyCheck { get { return isDuplicateKey; } }

    public short SelectIndex { get { return selectIdx; } }

    private void Start()
    {
        contents = new Transform[transform.childCount];
        for ( int idx = 0; idx < transform.childCount; ++idx )
        {
            contents[idx] = transform.GetChild( idx );
        }

        rectTransform = GetComponent<RectTransform>();

        CenterNumber = maxShowContentsCount == 0 ? 0 : maxShowContentsCount / 2;
        movePos = rectTransform.localPosition.y - ( MoveOffset * CenterNumber );

        float width = rectTransform.rect.width;
        rectTransform.sizeDelta = new Vector2( width, contents.Length * MoveOffset );
    }

    public void SnapUp()
    {
        if ( selectIdx <= 0 )
        {
            isDuplicateKey = true;
            return;
        }

        contents[selectIdx].DOScale( new Vector3( 1f, 1f, 1f ), 0.1f );
        movePos -= MoveOffset;
        rectTransform.DOLocalMoveY( movePos, 0.1f );

        if ( selectIdx > 0 ) --selectIdx;

        contents[selectIdx].DOScale( new Vector3( 1.1f, 1.1f, 1f ), 0.1f );
        isDuplicateKey = false;
    }

    public void SnapDown()
    {
        if ( selectIdx >= contents.Length - 1 )
        {
            isDuplicateKey = true;
            return;
        }

        contents[selectIdx].DOScale( new Vector3( 1f, 1f, 1f ), 0.1f );
        movePos += MoveOffset;
        rectTransform.DOLocalMoveY( movePos, 0.1f );

        if ( selectIdx < contents.Length - 1 ) ++selectIdx;

        contents[selectIdx].DOScale( new Vector3( 1.1f, 1.1f, 1f ), 0.1f );
        isDuplicateKey = false;
    }
    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}
