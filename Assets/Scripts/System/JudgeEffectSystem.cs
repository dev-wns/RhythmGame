using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffectSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private Judgement judge;
    private SpriteRenderer rdr;
    private Sequence sequence;
    private Vector2 endScale;

    private HitResult prevResult = HitResult.None;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        endScale = transform.localScale;
        transform.position = new Vector3( transform.position.x + GameSetting.GearOffsetX, transform.position.y, transform.position.z );
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

    private void HitEffect( JudgeResult _result )
    {
        if ( _result.noteType == NoteType.Slider )
             return;

        HitResult hitResult = _result.hitResult;
        if ( prevResult != hitResult )
        {
            switch ( hitResult )
            {
                case HitResult.Maximum:
                case HitResult.Perfect: rdr.sprite = sprites[4]; break;
                case HitResult.Great:   rdr.sprite = sprites[3]; break;
                case HitResult.Good:    rdr.sprite = sprites[2]; break;
                case HitResult.Bad:     rdr.sprite = sprites[1]; break;
                case HitResult.Miss:    rdr.sprite = sprites[0]; break;
                default: return;
            }
            prevResult = hitResult;
        }

        rdr.color = Color.white;
        transform.localScale = endScale * .5f;
        sequence.Restart();
    }
}