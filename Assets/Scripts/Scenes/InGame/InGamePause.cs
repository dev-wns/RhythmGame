using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePause : ScrollOption, IKeyBind
{
    public RectTransform selectUI;

    private InGame scene;

    protected override void Awake()
    {
        base.Awake();
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        KeyBind();
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

    public void KeyBind()
    {
        scene.Bind( ActionType.Pause, KeyCode.UpArrow,   () => PrevMove() );
        scene.Bind( ActionType.Pause, KeyCode.DownArrow, () => NextMove() );

        scene.Bind( ActionType.Pause, KeyCode.Return, () => CurrentOption.Process() );
        scene.Bind( ActionType.Pause, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MenuClick ) );
    }
}
