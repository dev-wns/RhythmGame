using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ComboSystem : NumberAtlasBase
{
    [Header( "System" )]
    private Judgement judge;
    private int combo;

    private Tween comboTween, missComboTween;

    private readonly Color comboColor = new Color( 1f, 1f, 1f, .75f );
    private readonly Color missColor  = new Color( 1f, 0f, 0f, .75f );

    protected override void Awake()
    {
        base.Awake();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboImageUpdate;

        ComboImageUpdate( JudgeType.Miss );
    }

    private void OnDestroy()
    {
        comboTween?.Kill();
        missComboTween?.Kill();
    }

    private void ComboImageUpdate( JudgeType _type )
    {
        float calcCombo;
        switch ( _type )
        {
            case JudgeType.None:
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

                    images[i].sprite = sprites[( int )calcCombo % 10];
                    calcCombo *= .1f;
                }

                transform.localScale = new Vector3( .75f, .75f, 1f );
                comboTween?.Kill();
                comboTween = transform.DOScale( Vector3.one, .085f );
            } break;

            case JudgeType.Miss:
            {
                combo = 0;

                images[0].gameObject.SetActive( true );
                images[0].sprite = atlas.GetSprite( $"combo-0" );
                for ( int i = 1; i < images.Count; i++ )
                {
                    if ( images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( false );
                }

                //images[0].color      = missColor;
                transform.localScale = Vector3.one;
                //missComboTween?.Kill();
                //missComboTween = images[0].DOColor( comboColor, .085f );
            } break;
        }
    }
}
