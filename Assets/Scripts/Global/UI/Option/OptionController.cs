using UnityEngine;


public class OptionController : ScrollHide
{
    public RectTransform selectUI;
    public bool isRememberIndex = true;

    protected override void Start()
    {
        base.Start();

        OptionProcess();
        while ( CurrentOption.type == OptionType.Title )
            NextMove();
    }

    private void OnDisable()
    {
        if ( !isRememberIndex )
            Select( 0 );
    }

    protected override void Select( int _pos )
    {
        base.Select( _pos );

        PreviousOption?.ActiveOutline( false );
        CurrentOption?.ActiveOutline( true );
        SetSelectUIParent();
    }

    private void SetSelectUIParent()
    {
        if ( selectUI == null )
            return;

        var option = CurrentOption.transform as RectTransform;
        selectUI.SetParent( option );
        selectUI.anchoredPosition = Vector2.zero;
    }

    protected virtual void Update() => CurrentOption?.InputProcess();

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
        if ( CurrentOption.type == OptionType.Title )
        {
            if ( CurrentIndex == 0 ) NextMove();
            else PrevMove();
        }
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
        if ( CurrentOption.type == OptionType.Title )
        {
            if ( CurrentIndex == Length ) PrevMove();
            else NextMove();
        }
    }

    private void OptionProcess()
    {
        SetSelectUIParent();
        PreviousOption?.ActiveOutline( false );
        CurrentOption?.ActiveOutline( true );
    }
}
