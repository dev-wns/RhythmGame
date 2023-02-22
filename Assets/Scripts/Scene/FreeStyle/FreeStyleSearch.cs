using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FreeStyleSearch : MonoBehaviour
{
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
        KeyBind();
    }

    private void Update()
    {
        if ( ( Input.GetMouseButtonDown( 0 )      && field.interactable ) ||
             ( Input.GetKeyDown( KeyCode.Escape ) && field.interactable ) )
        {
            field.ActivateInputField();
            field.MoveTextEnd( false );
        }
    }

    private void EnableInputField()
    {
        field.interactable = true;
        field.ActivateInputField();
        field.MoveTextEnd( false );
    }

    private void DisableInputField()
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
            NowPlaying.Inst.UpdateSong( 0 );
            OnSearch?.Invoke();
            scene.IsInputLock = false;
        }
    }

    void KeyBind()
    {
        scene.Bind( ActionType.Main,   KeyCode.F2,     EnableInputField );
        scene.Bind( ActionType.Search, KeyCode.F2,     DisableInputField );
        scene.Bind( ActionType.Search, KeyCode.Escape, DisableInputField );
    }
}
