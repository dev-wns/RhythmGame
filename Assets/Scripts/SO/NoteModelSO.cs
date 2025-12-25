using UnityEngine;

[CreateAssetMenu( fileName = "NoteModelSO", menuName = "Scriptable Objects/Note/NoteModelSO" )]
public class NoteModelSO : ScriptableObject
{
    public PreviewNoteRenderer block;
    public PreviewNoteRenderer blockFade;
    public PreviewNoteRenderer onlyBody;
    public PreviewNoteRenderer round;
    public PreviewNoteRenderer roundFade;
    public PreviewNoteRenderer circle;

    public PreviewNoteRenderer GetModel( NoteModel _type )
    {
        return _type switch 
        {
            NoteModel.Block     => block,
            NoteModel.BlockFade => blockFade,
            NoteModel.OnlyBody  => onlyBody,
            NoteModel.Round     => round,
            NoteModel.RoundFade => roundFade,
            NoteModel.Circle    => circle,
            _                   => block,
        };
    }
}
