using System.Text;
using UnityEngine;

public class ResolutionOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )SystemSetting.CurrentResolution;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )Resolution.Count; i++ )
        {
            builder.Clear();

            Vector2 screen = SystemSetting.Inst.ResolustionToVector( ( Resolution )i );
            builder.Append( ( int )screen.x ).Append( "x" ).Append( ( int )screen.y );

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.CurrentResolution = ( Resolution )CurrentIndex;
        SystemSetting.Inst.ApplyScreenSetting();
    }
}
