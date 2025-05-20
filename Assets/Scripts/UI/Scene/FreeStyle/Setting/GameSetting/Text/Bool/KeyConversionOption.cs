using System;

public class KeyConversionOption : OptionText
{
    public Action OnChangeOption;

    private void OnEnable()
    {
        CurrentIndex = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) ? 1 : 0;
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
        if ( CurrentIndex == 0 ) GameSetting.CurrentGameMode &= ~GameMode.KeyConversion;
        else GameSetting.CurrentGameMode |= GameMode.KeyConversion;

        OnChangeOption?.Invoke();
    }
}
