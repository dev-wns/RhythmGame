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
    private Transform tf;
    private Vector2 sclCache;

    private HitResult prevType = HitResult.None;

    private void Awake()
    {
        tf = transform as RectTransform;
        rdr = GetComponent<SpriteRenderer>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        sclCache = tf.localScale;
    }

    private void Start()
    {
        sequence = DOTween.Sequence();

        sequence.Pause().SetAutoKill( false );
        sequence.Append( tf.DOScale( sclCache, .15f ) );
        sequence.AppendInterval( .5f );
        sequence.Append( rdr.DOFade( 0f, .5f ) );
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    private void HitEffect( HitResult _type )
    {
        if ( prevType != _type )
        {
            switch ( _type )
            {
                case HitResult.None:                                 return;
                case HitResult.Perfect:     rdr.sprite = sprites[4]; break;
                case HitResult.Great:       rdr.sprite = sprites[3]; break;
                case HitResult.Good:        rdr.sprite = sprites[2]; break;
                case HitResult.Bad:         rdr.sprite = sprites[1]; break;
                case HitResult.Miss:        rdr.sprite = sprites[0]; break;
            }
            prevType = _type;
        }

        rdr.color = Color.white;
        tf.localScale = sclCache * .75f;
        sequence.Restart();
    }
}