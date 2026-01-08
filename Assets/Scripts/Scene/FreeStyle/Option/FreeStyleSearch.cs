using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeStyleSearch : MonoBehaviour
{
    [SerializeField] private FreeStyle scene;

    [Header( "InputField" )]
    [SerializeField] private TMP_InputField field;
    private string prevText = string.Empty;
    private readonly float SearchWaitTime = 1f;
    
    //public static bool IsSearching;
    //private static string SearchText = string.Empty;

    [Header( "Image") ]
    [SerializeField] private CanvasGroup timerGroup;
    [SerializeField] private Image timer;
    [SerializeField] private Image check;
    [SerializeField] private Image cross;

    public  Action OnSearch;
    private Coroutine corLateSearch;
    //private SceneAction prevAction;

    private void Awake()
    {
        field.SetTextWithoutNotify( Browser.Inst.SearchText );

        timerGroup.alpha = 0f;
        check.enabled    = true;
        cross.enabled    = false;
    }

    private void LateUpdate()
    {
        timer.fillAmount += Time.deltaTime;
        if ( scene.CurrentAction == SceneAction.Main )
        {
            if ( Browser.Inst.IsSearching && field.interactable && Input.GetKeyDown( KeyCode.Escape ) )
            {
                if ( scene.CurrentAction == SceneAction.Main )
                     field.text = string.Empty;
            }

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

    public void SearchEvent()
    {
        string previous = prevText.Replace( " ", string.Empty );
        string current  = field.text.Replace( " ", string.Empty );
        if ( string.Compare( previous, current, StringComparison.OrdinalIgnoreCase ) != 0 )
        {
            timerGroup.alpha = 1f;
            timer.fillAmount = 0f;
            check.enabled = false;
            cross.enabled = false;

            scene.IsInputLock = true;
            if ( !ReferenceEquals( corLateSearch, null ) )
            {
                StopCoroutine( corLateSearch );
                corLateSearch = null;
            }

            corLateSearch = StartCoroutine( UpdateSearchSongs() );
            AudioManager.Inst.Play( previous.Length < current.Length ? SFX.keyboard_Input : SFX.Keyboard_Backspace );
        }

        prevText = field.text;
    }

    private IEnumerator UpdateSearchSongs()
    {
        yield return YieldCache.WaitForSeconds( SearchWaitTime );

        ImmediateSearch();
        scene.IsInputLock = false;
    }

    public void ImmediateSearch()
    {
        timerGroup.alpha = 0f;
        if ( Browser.Inst.Search( field.text ) == 0 )
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
    }
}