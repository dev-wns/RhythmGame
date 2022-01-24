using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Flags]
public enum NumberBit : byte
{
    None      = 0,
    _1        = 1 << 0,
    _10       = 1 << 1,
    _100      = 1 << 2,
    _1000     = 1 << 3,
    _10000    = 1 << 4,
    _100000   = 1 << 5,
    _1000000  = 1 << 6,
    _10000000 = 1 << 7,
    All       = byte.MaxValue,
}

public class ComboSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private List<Image> images = new List<Image>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private Judgement judge;
    private int previousCombo = -1, currentCombo;

    private Sequence comboSequence;
    private NumberBit previousBit, currentBit;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<Image>( true ) );
        images.Reverse();

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboUpdate;

        StartCoroutine( ComboProcess() );
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
        StopAllCoroutines();
    }

    private IEnumerator ComboProcess()
    {
        while( true )
        {
            yield return YieldCache.WaitForSeconds( .025f );

            if ( previousCombo == currentCombo ) 
                 continue;

            previousCombo = currentCombo;
            previousBit   = currentBit;

            if ( currentCombo == 0 )
            {
                currentBit = NumberBit._1;

                if ( !images[0].gameObject.activeInHierarchy )
                     images[0].gameObject.SetActive( true );
                images[0].sprite = sprites[0];

                for ( int i = 1; i < images.Count; i++ )
                {
                    if ( images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( false );
                }

                transform.localScale = Vector3.one;
            }
            else
            {
                int num;
                float calcCombo = currentCombo;
                if ( currentCombo > 0 ) num = Globals.Log10( calcCombo ) + 1;
                else                    num = 1;

                for ( int i = 0; i < images.Count; i++ )
                {
                    if ( i == num ) break;
                    currentBit |= ( NumberBit )( 1 << i );

                    if ( !images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( true );

                    images[i].sprite = sprites[( int )calcCombo % 10];
                    calcCombo *= .1f;
                }

                transform.localScale = new Vector3( .75f, .75f, 1f );
                comboSequence.Restart();
            }

            if ( previousBit != currentBit )
                 layoutGroup.SetLayoutHorizontal();
        }
    }

    private void ComboUpdate( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None:
            case JudgeType.Perfect:
            case JudgeType.LazyPerfect:
            case JudgeType.Great:
            case JudgeType.Good:
            case JudgeType.Bad:
            currentCombo++;
            break;

            case JudgeType.Miss:
            currentCombo = 0;
            break;
        }
    }
}
