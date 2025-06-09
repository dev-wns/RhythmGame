using System;

public class KeyConvertOption : OptionText
{
    public Action OnChangeOption;

    private void OnEnable()
    {
        CurrentIndex = GameSetting.HasFlag( GameMode.ConvertKey ) ? 1 : 0;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )BooleanOption.Count; i++ )
        {
            texts.Add( ( ( BooleanOption )i ).ToString() );
        }
    }

    public override void Process()
    {
        if ( CurrentIndex == 0 ) GameSetting.CurrentGameMode &= ~GameMode.ConvertKey;
        else                     GameSetting.CurrentGameMode |=  GameMode.ConvertKey;

        OnChangeOption?.Invoke();
    }
}
