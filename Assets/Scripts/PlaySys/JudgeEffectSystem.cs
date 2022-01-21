using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffectSystem : NumberAtlasBase
{
    private Judgement judge;
    private SpriteRenderer rdr;

    protected override void Awake()
    {
        base.Awake();
        rdr = GetComponent<SpriteRenderer>();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HitEffect;

        rdr.color = Color.clear;
    }

    private void HitEffect( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None:                                 break;
            case JudgeType.Perfect:     rdr.sprite = sprites[5]; break;
            case JudgeType.LazyPerfect: rdr.sprite = sprites[4]; break;
            case JudgeType.Great:       rdr.sprite = sprites[3]; break;
            case JudgeType.Good:        rdr.sprite = sprites[2]; break;
            case JudgeType.Bad:         rdr.sprite = sprites[1]; break;
            case JudgeType.Miss:        rdr.sprite = sprites[0]; break;
        }

        rdr.color = Color.white;
    }
}