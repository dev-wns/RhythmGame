using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class FreeStyleSearch : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField field;
    public GameObject noSearchResultText;

    private Scene scene;
    public Action OnSearch;
    private Coroutine lateSearchCoroutine;
    private readonly float SearchWaitTime = 1f;
    private static string SearchText = string.Empty;

    private void Awake()
    {
        field.SetTextWithoutNotify( SearchText );
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }

    private void Update()
    {
        if ( scene.CurrentAction != ActionType.Search )
            return;

        if ( ( Input.GetMouseButtonDown( 0 ) && field.interactable ) ||
             ( Input.GetKeyDown( KeyCode.Return ) && field.interactable ) )
        {
            field.ActivateInputField();
            field.MoveTextEnd( false );
        }
    }

    public void Clear()
    {
        SearchText = string.Empty;
        field.text = string.Empty;
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
    }

    public void Search()
    {
        scene.IsInputLock = true;
        if ( !ReferenceEquals( lateSearchCoroutine, null ) )
        {
            StopCoroutine( lateSearchCoroutine );
            lateSearchCoroutine = null;
        }

        lateSearchCoroutine = StartCoroutine( UpdateSearchSongs() );
    }

    private IEnumerator UpdateSearchSongs()
    {
        yield return YieldCache.WaitForSeconds( SearchWaitTime );
        string searchText = SearchText.Replace( " ", string.Empty );
        string fieldText  = field.text.Replace( " ", string.Empty );
        if ( string.Compare( searchText, fieldText, StringComparison.OrdinalIgnoreCase ) == 0 )
        {
            scene.IsInputLock = false;
            yield break;
        }

        SearchText = field.text;
        NowPlaying.Inst.Search( fieldText );
        noSearchResultText.SetActive( NowPlaying.Inst.SearchCount == 0 );

        if ( NowPlaying.Inst.SearchCount != 0 )
        {
            //NowPlaying.Inst.UpdateSong( 0 );
            OnSearch?.Invoke();
            scene.IsInputLock = false;
        }
    }
}
