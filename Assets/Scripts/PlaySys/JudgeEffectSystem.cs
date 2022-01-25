using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class JudgeEffectSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private Judgement judge;
    private Image image;
    private Sequence sequence;
    private RectTransform rt;
    private Vector2 sizeDeltaCache;

    private JudgeType prevType = JudgeType.None;

    private void Awake()
    {
        rt = transform as RectTransform;
        image = GetComponent<Image>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        sizeDeltaCache = rt.sizeDelta;
    }

    private void Start()
    {
        sequence = DOTween.Sequence();

        sequence.Pause().SetAutoKill( false );
        sequence.Append( rt.DOSizeDelta( sizeDeltaCache, .15f, true ) );
        sequence.AppendInterval( .5f );
        sequence.Append( image.DOFade( 0f, .5f ) );
    }

    private void OnDestroy()
    {
        sequence.Kill();
    }

    private void HitEffect( JudgeType _type )
    {
        if ( prevType != _type )
        {
            switch ( _type )
            {
                case JudgeType.None:                                   return;
                case JudgeType.Perfect:     image.sprite = sprites[5]; break;
                case JudgeType.LazyPerfect: image.sprite = sprites[4]; break;
                case JudgeType.Great:       image.sprite = sprites[3]; break;
                case JudgeType.Good:        image.sprite = sprites[2]; break;
                case JudgeType.Bad:         image.sprite = sprites[1]; break;
                case JudgeType.Miss:        image.sprite = sprites[0]; break;
            }
            prevType = _type;
        }

        image.color = Color.white;
        rt.sizeDelta = sizeDeltaCache * .75f;
        sequence.Restart();
    }
}