using System.Text;

public class SoundDriverOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = AudioManager.Inst.CurrentDriverIndex;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        var drivers = AudioManager.Inst.Drivers;
        for ( int i = 0; i < drivers.Count; i++ )
        {
            string text = drivers[i].name;
            var split = text.Split( '(' );

            builder.Clear();
            builder.Append( split[0] );
            if ( split.Length > 1 )
            {
                builder.Append( "\n" );
                for ( int j = 1; j < split.Length; j++ )
                {
                    builder.Append( "(" );
                    builder.Append( split[j] );
                }
            }

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        AudioManager.Inst.CurrentDriverIndex = CurrentIndex;
    }
}
