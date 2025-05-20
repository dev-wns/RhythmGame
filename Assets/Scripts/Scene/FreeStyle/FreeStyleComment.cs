using System.IO;
using TMPro;
using UnityEngine;

public class FreeStyleComment : MonoBehaviour
{
    private Scene scene;
    public GameObject canvas;
    public TMP_InputField field;
    public FreeStyleMainScroll mainScroll;

    private string path;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        field.SetTextWithoutNotify( string.Empty );
        mainScroll.OnSelectSong += UpdateComment;
    }

    private void Update()
    {
        if ( scene.CurrentAction != ActionType.Comment )
            return;

        if ( ( Input.GetMouseButtonDown( 0 ) && field.interactable ) ||
             ( Input.GetKeyDown( KeyCode.Return ) && field.interactable ) )
        {
            field.ActivateInputField();
            field.MoveTextEnd( false );
        }
    }

    public void EnableInputField()
    {
        field.interactable = true;
        field.ActivateInputField();
        field.MoveTextEnd( false );
    }

    public void DisableInputField()
    {
        field.interactable = false;
        field.DeactivateInputField();
        if ( field.text.Trim() == string.Empty )
            field.text = "None";
    }

    private void UpdateComment( Song _song )
    {
        path = Path.Combine( Path.GetDirectoryName( _song.filePath ), $"{Path.GetFileNameWithoutExtension( _song.filePath )}_Comment.txt" );
        if ( File.Exists( path ) )
        {
            using ( StreamReader reader = new StreamReader( path ) )
            {
                var txt = reader.ReadToEnd().Split( ':' )[1].Trim();
                field.text = txt == string.Empty ? "None" : txt;
            }
        }
        else
        {
            field.text = "None";
        }
    }

    public void ReviseComment()
    {
        FileMode mode = File.Exists( path ) ? FileMode.Truncate : FileMode.Create;
        using ( FileStream stream = new FileStream( path, mode ) )
        {
            using ( StreamWriter writer = new StreamWriter( stream, System.Text.Encoding.UTF8 ) )
            {
                writer.Write( $"Comment: {( field.text.Trim() == string.Empty ? "None" : field.text )}" );
            }
        }
    }
}