using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using DG.Tweening;

public class ComboSystem : MonoBehaviour
{
    public SpriteAtlas atlas;
    public Judgement judge;
    private List<Image> images = new List<Image>();
    private int combo;

    private void Awake()
    {
        judge.OnJudge += ComboImageUpdate;

        images.AddRange( GetComponentsInChildren<Image>() );
        images.Reverse();

        ComboImageUpdate( JudgeType.Miss );
    }

    private void ComboImageUpdate( JudgeType _type )
    {
        float calcCombo;
        switch ( _type )
        {
            case JudgeType.Kool:
            case JudgeType.Cool:
            case JudgeType.Good:
            case JudgeType.Bad:
            {
                int num;
                calcCombo = ++combo;
                if ( combo > 0 ) num = ( int )Mathf.Log10( calcCombo ) + 1;
                else             num = 1;

                for ( int i = 0; i < images.Count; i++ )
                {
                    if ( i == num ) break;

                    if ( !images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( true );

                    images[i].sprite = atlas.GetSprite( $"score-{( int )calcCombo % 10}" );
                    calcCombo *= .1f;
                }

                DOTween.Kill( transform );
                transform.localScale = new Vector3( .75f, .75f, 1f );
                transform.DOScale( Vector3.one, .1f );
            } break;

            case JudgeType.Miss:
            {
                combo = 0;

                images[0].gameObject.SetActive( true );
                images[0].sprite = atlas.GetSprite( $"score-0" );
                for ( int i = 1; i < images.Count; i++ )
                {
                    if ( images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( false );
                }
            } break;
        }
    }
}
