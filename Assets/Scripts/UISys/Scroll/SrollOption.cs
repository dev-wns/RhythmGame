using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollOption : ScrollBase
{
    [Header( "ScrollOption" )]
    public RectTransform contents;
    [SerializeField] protected List<OptionBase> options = new List<OptionBase>();
    protected OptionBase CurrentOption { get; private set; }
    protected OptionBase PreviousOption { get; private set; }

    protected virtual void Awake()
    {
        for ( int i = 0; i < contents.childCount; i++ )
        {
            var option = contents.GetChild( i );
            if ( option.TryGetComponent( out OptionBase optionBase ) )
                 options.Add( optionBase );
            else
                Debug.LogWarning( $"The {option.name} does not have OptionBase component." );
        }
        Length = options.Count;
        Select( 0 );
    }

    protected override void Select( int _pos )
    {
        base.Select( _pos );

        if ( Length <= 0 ) return;

        PreviousOption = CurrentOption;
        CurrentOption  = options[_pos];
    }

    public override void PrevMove()
    {
        base.PrevMove();

        CurrentOption  = options[CurrentIndex];
        PreviousOption = options[PreviousIndex];
    }

    public override void NextMove()
    {
        base.NextMove();
         
        CurrentOption  = options[CurrentIndex];
        PreviousOption = options[PreviousIndex];
    }
}
