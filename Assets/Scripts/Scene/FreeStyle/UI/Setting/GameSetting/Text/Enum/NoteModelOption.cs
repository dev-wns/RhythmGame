using System;

public class NoteModelOption : OptionText
{
    public event Action<NoteModel> OnChangedNoteModel;

    private void OnEnable()
    {
        CurrentIndex = ( int ) GameSetting.CurrentNoteModel;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int ) NoteModel.Count; i++ )
        {
            switch ( (NoteModel)i)
            {
                case NoteModel.Block:     texts.Add( "벽돌" );             break;
                case NoteModel.BlockFade: texts.Add( "벽돌( Fade )" );     break;
                case NoteModel.OnlyBody:  texts.Add( "벽돌( OnlyBody )" ); break;
                case NoteModel.Round:     texts.Add( "둥그스름" );         break;
                case NoteModel.RoundFade: texts.Add( "둥그스름( Fade )" ); break;
                case NoteModel.Circle:    texts.Add( "서클" );             break;
            }
        }
    }

    public override void Process()
    {
        GameSetting.CurrentNoteModel = ( NoteModel )CurrentIndex;
        ChangeText( texts[CurrentIndex] );
        OnChangedNoteModel?.Invoke( GameSetting.CurrentNoteModel );
    }
}
