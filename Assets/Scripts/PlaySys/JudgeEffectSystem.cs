using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class JudgeEffectSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private Judgement judge;
    private SpriteRenderer rdr;
    private Sequence sequence;
    private Vector2 endScale;

    private HitResult prevType = HitResult.None;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        endScale = transform.localScale;
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    private void Start()
    {
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( transform.DOScale( endScale, .1f ) ).
                 AppendInterval( .5f ).
                 Append( rdr.DOFade( 0f, .5f ) );
    }

    private void HitEffect( HitResult _result, NoteType _type )
    {
        if ( _type == NoteType.Slider )
             return;

        if ( prevType != _result )
        {
            switch ( _result )
            {
                case HitResult.None:                                 return;
                case HitResult.Maximum:
                case HitResult.Perfect:     rdr.sprite = sprites[4]; break;
                case HitResult.Great:       rdr.sprite = sprites[3]; break;
                case HitResult.Good:        rdr.sprite = sprites[2]; break;
                case HitResult.Bad:         rdr.sprite = sprites[1]; break;
                case HitResult.Miss:        rdr.sprite = sprites[0]; break;
            }
            prevType = _result;
        }

        rdr.color = Color.white;
        transform.localScale = endScale * .75f;
        sequence.Restart();
    }
}