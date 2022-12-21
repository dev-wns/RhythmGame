using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EffectSystem))]
public class JudgeEffectSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private Judgement judge;
    private SpriteRenderer rdr;
    private EffectSystem effectSys;
    private Vector2 endScale;

    private HitResult prevType = HitResult.None;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        effectSys = GetComponent<EffectSystem>();

        endScale = transform.localScale;

        effectSys.Append( transform.DoScale( endScale, .1f ) ).
                  AppendInterval( .5f ).
                  Append( rdr.DoFade( 0f, .5f ) );
    }

    private void HitEffect( HitResult _type )
    {
        if ( prevType != _type )
        {
            switch ( _type )
            {
                case HitResult.None:                                 return;
                case HitResult.Maximum:
                case HitResult.Perfect:     rdr.sprite = sprites[4]; break;
                case HitResult.Great:       rdr.sprite = sprites[3]; break;
                case HitResult.Good:        rdr.sprite = sprites[2]; break;
                case HitResult.Bad:         rdr.sprite = sprites[1]; break;
                case HitResult.Miss:        rdr.sprite = sprites[0]; break;
            }
            prevType = _type;
        }

        rdr.color = Color.white;
        transform.localScale = endScale * .75f;
        effectSys.Restart();
    }
}