using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ComboSystem : NumberAtlasBase
{
    [Header( "System" )]
    private Judgement judge;
    private int combo;

    private Sequence comboSequence;

    protected override void Awake()
    {
        base.Awake();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboImageUpdate;

        ComboImageUpdate( JudgeType.Miss );
    }

    private void Start()
    {
        comboSequence = DOTween.Sequence();

        comboSequence.Pause().SetAutoKill( false );
        comboSequence.Append( transform.DOScale( Vector3.one, .085f ) );
    }

    private void OnDestroy()
    {
        comboSequence?.Kill();
    }

    private void ComboImageUpdate( JudgeType _type )
    {
        float calcCombo;
        switch ( _type )
        {
            case JudgeType.None:
            case JudgeType.Perfect:
            case JudgeType.LazyPerfect:
            case JudgeType.Great:
            case JudgeType.Good:
            case JudgeType.Bad:
            {
                int num;
                calcCombo = ++combo;
                if ( combo > 0 ) num = Globals.Log10( calcCombo ) + 1;
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
                comboSequence.Restart();
            } break;

            case JudgeType.Miss:
            {
                combo = 0;

                images[0].gameObject.SetActive( true );
                images[0].sprite = sprites[0];
                for ( int i = 1; i < images.Count; i++ )
                {
                    if ( images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( false );
                }

                transform.localScale = Vector3.one;
            } break;
        }
    }
}
