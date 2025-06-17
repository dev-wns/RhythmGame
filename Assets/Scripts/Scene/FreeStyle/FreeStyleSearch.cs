using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeStyleSearch : MonoBehaviour
{
    [Header( "InputField" )]
    public TMP_InputField field;

    private static string SearchText = string.Empty;
    private readonly float SearchWaitTime = 1f;

    [Header( "Image") ]
    public CanvasGroup timerGroup;
    public Image timer;
    public Image check;
    public Image cross;


    public  Action OnSearch;
    private Coroutine corLateSearch;

    private void Awake()
    {
        field.SetTextWithoutNotify( SearchText );

        timerGroup.alpha = 0f;
        check.enabled    = true;
        cross.enabled    = false;
    }

    private void LateUpdate()
    {
        timer.fillAmount += Time.deltaTime;
        if ( NowPlaying.CurrentScene.CurrentAction == ActionType.Main )
        {
            if ( !field.isFocused )
            {
                field.interactable = true;
                field.ActivateInputField();
            }

            if ( field.text.Length != field.selectionStringFocusPosition )
                 field.MoveTextEnd( false );
        }
        else
        {
            if ( field.isFocused )
            {
                field.interactable = false;
                field.DeactivateInputField();
            }
        }
    }

    public void SearchEvnet()
    {
        string searchText = SearchText.Replace( " ", string.Empty );
        string fieldText  = field.text.Replace( " ", string.Empty );
        if ( string.Compare( searchText, fieldText, StringComparison.OrdinalIgnoreCase ) != 0 )
        {
            timerGroup.alpha = 1f;
            timer.fillAmount = 0f;
            check.enabled = false;
            cross.enabled = false;

            NowPlaying.CurrentScene.IsInputLock = true;
            if ( !ReferenceEquals( corLateSearch, null ) )
            {
                StopCoroutine( corLateSearch );
                corLateSearch = null;
            }

            corLateSearch = StartCoroutine( UpdateSearchSongs() );
        }

        SearchText = field.text;
    }

    private IEnumerator UpdateSearchSongs()
    {
        yield return YieldCache.WaitForSeconds( SearchWaitTime );

        timerGroup.alpha = 0f;
        if ( NowPlaying.Inst.Search( field.text ) == 0 )
        {
            check.enabled = false;
            cross.enabled = true;
        }
        else
        {
            check.enabled = true;
            cross.enabled = false;
        }

        OnSearch?.Invoke();
        NowPlaying.CurrentScene.IsInputLock = false;
    }
}