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
    private Sequence moveHideSequence;
    private Vector3 initPosCache;

    private JudgeType prevType = JudgeType.None;

    private void Awake()
    {
        image = GetComponent<Image>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        initPosCache = transform.position;
    }

    private void Start()
    {
        moveHideSequence = DOTween.Sequence();

        moveHideSequence.Pause().SetAutoKill( false );
        moveHideSequence.AppendCallback( () => transform.position = initPosCache );
        moveHideSequence.Append( transform.DOMoveY( initPosCache.y + 30f, .15f ) );
        moveHideSequence.AppendInterval( .5f );
        moveHideSequence.Append( image.DOFade( 0f, .5f ) );
    }

    private void OnDestroy()
    {
        moveHideSequence.Kill();
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
        moveHideSequence.Restart();
    }
}