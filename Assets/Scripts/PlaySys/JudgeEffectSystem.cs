using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class JudgeEffectSystem : NumberAtlasBase
{
    private Judgement judge;
    private SpriteRenderer rdr;
    private Sequence moveHideSequence;
    private Vector3 initPosCache;

    protected override void Awake()
    {
        base.Awake();
        rdr = GetComponent<SpriteRenderer>();
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
        moveHideSequence.Append( rdr.DOFade( 0f, .5f ) );
    }

    private void OnDestroy()
    {
        moveHideSequence.Kill();
    }
    private void HitEffect( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None:                                 return;
            case JudgeType.Perfect:     rdr.sprite = sprites[5]; break;
            case JudgeType.LazyPerfect: rdr.sprite = sprites[4]; break;
            case JudgeType.Great:       rdr.sprite = sprites[3]; break;
            case JudgeType.Good:        rdr.sprite = sprites[2]; break;
            case JudgeType.Bad:         rdr.sprite = sprites[1]; break;
            case JudgeType.Miss:        rdr.sprite = sprites[0]; break;
        }

        rdr.color = Color.white;
        moveHideSequence.Restart();
    }
}