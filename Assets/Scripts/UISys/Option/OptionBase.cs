using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionBase : MonoBehaviour, IOption, IKeyControl
{
    [Header( "Type" )]
    public SceneAction actionType = SceneAction.Option;
    public OptionType type { get; protected set; }

    [Header("Infomation")]
    public TMPro.TextMeshProUGUI infoObject;
    [TextArea]
    public string infoText;

    protected Scene CurrentScene { get; private set; }
    private Outline outline;

    protected virtual void Awake()
    {
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        outline = GetComponent<Outline>();

        if ( outline is null ) return;
        outline.effectDistance = new Vector2( 5f, -5f );
        //outline.effectColor    = Color.yellow;
        ActiveOutline( false );
    }

    public void ActiveOutline( bool _isActive )
    {
        if ( outline is null ) return;
        outline ??= GetComponent<Outline>();
        outline.enabled = _isActive;
    }

    public void UpdateInfoText()
    {
        if ( infoObject == null ) return;
        infoObject.text = infoText;
    }

    public abstract void Process();

    public virtual void KeyBind()
    {
        CurrentScene ??= GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }

    public virtual void KeyRemove()
    {
        CurrentScene ??= GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }
}
