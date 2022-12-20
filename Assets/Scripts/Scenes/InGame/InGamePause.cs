using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePause : OptionController
{
    public RectTransform selectUI;

    protected override void Awake()
    {
        base.Awake();
        IsLoop = true;
    }

    private void MoveSelectPosition()
    {
        var option = CurrentOption.transform as RectTransform;
        selectUI.SetParent( option );
        selectUI.anchoredPosition = Vector2.zero;
    }

    private void OnEnable()
    {
        Select( 0 );
        MoveSelectPosition();
    }
    public override void PrevMove()
    {
        base.PrevMove();

        MoveSelectPosition();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    public override void NextMove()
    {
        base.NextMove();

        MoveSelectPosition();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }
}
