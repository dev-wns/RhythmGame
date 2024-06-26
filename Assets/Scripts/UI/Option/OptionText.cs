using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

[RequireComponent( typeof( ScrollBase ) )]
public abstract class OptionText : OptionBase
{
    [Header("Text")]
    public List<string> texts;
    public TextMeshProUGUI valueText;
    public bool isReturnProcess = true;
    public bool isActive;
    private ScrollBase scroller;

    public int CurrentIndex 
    {
        get => scroller.CurrentIndex;
        set { scroller.CurrentIndex = value; } 
    }

    protected override void Awake()
    {
        base.Awake();

        if ( !TryGetComponent( out scroller ) )
             Debug.LogError( $"The {gameObject.name} does not have ScrollBase component." );

        type = OptionType.Text;
        scroller.IsLoop = true;

        CreateObject();
        scroller.Length = texts.Count;
    }

    protected abstract void CreateObject();

    public override void InputProcess()
    {
        if ( isReturnProcess && Input.GetKeyDown( KeyCode.Return ) )
        {
            SoundManager.Inst.Play( SoundSfxType.MenuClick );
            Process();
        }

        InputAction( KeyCode.LeftArrow,  scroller.PrevMove );
        InputAction( KeyCode.RightArrow, scroller.NextMove );
    }

    public void PrevClick() 
    {
        scroller.PrevMove();
        SoundManager.Inst.Play( SoundSfxType.Slider );
        ChangeText( texts[CurrentIndex] );
        Process();
    }

    public void NextClick() 
    {
        scroller.NextMove();
        SoundManager.Inst.Play( SoundSfxType.Slider );
        ChangeText( texts[CurrentIndex] );
        Process();
    }

    private void InputAction( KeyCode _keyCode, Action _action )
    {
        if ( Input.GetKeyDown( _keyCode ) )
        {
            _action?.Invoke();
            SoundManager.Inst.Play( SoundSfxType.Slider );
            ChangeText( texts[CurrentIndex] );

            if ( !isReturnProcess )
                 Process();
        }
    }

    protected void ChangeText( string _text )
    {
        if ( valueText == null ) return; 

        valueText.text = _text;
    }
}