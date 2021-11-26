using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSystem : MonoBehaviour
{
    public int keyIndex;
    private RectTransform rt;
    public Queue<Note> notes = new Queue<Note>();
    public Queue<ColNote> cNotes = new Queue<ColNote>();
    private ColNote curColNote;
    private Note curNote;
    private bool isCheckComplate = true;
    private float diff = 0f;
    public TextMeshProUGUI difftext;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2( GlobalSetting.NoteStartPos + ( GlobalSetting.NoteWidth * keyIndex ) + 
                                         ( GlobalSetting.NoteBlank * keyIndex ) + GlobalSetting.NoteBlank, 0f );

        StartCoroutine( Process() );
        StartCoroutine( CheckOutLine() );
    }

    IEnumerator Process()
    {
        yield return new WaitUntil( () => !isCheckComplate );

        if ( Input.GetKeyDown( KEY.Keys[( KeyAction )keyIndex] ) )
        {
            if ( diff < 150f )
            {
                GameManager.Combo++;
                InGame.nPool.Despawn( curNote );
                //InGame.cPool.Despawn( curColNote );
                isCheckComplate = true;
            }
        }
        StartCoroutine( Process() );
    }

    IEnumerator CheckOutLine()
    {
        yield return new WaitUntil( () => !isCheckComplate );

        if ( curNote.timing - InGame.__time < 0 && diff > 150f )
        {
            //GameManager.Combo = 0;
            InGame.nPool.Despawn( curNote );
            //InGame.cPool.Despawn( curColNote );
            isCheckComplate = true;
        }

        StartCoroutine( CheckOutLine() );
    }

    private void FixedUpdate()
    {
        if ( isCheckComplate && notes.Count >= 1 )
        {
            curNote = notes.Dequeue();
            //curColNote = cNotes.Dequeue();

            isCheckComplate = false;
        }

        if ( !isCheckComplate )
            diff = Mathf.Abs( InGame.__time - curNote.originTiming );

        if ( keyIndex == 5 )
            difftext.text = string.Format( "{0}", diff );
    }
}
