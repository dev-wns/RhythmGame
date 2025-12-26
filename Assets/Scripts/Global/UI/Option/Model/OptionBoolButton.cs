using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public abstract class OptionBoolButton : OptionBase, IPointerDownHandler
{
    public bool CurrentBool { get; protected set; }
    public TextMeshProUGUI title;

    protected static readonly Color DisableColor = new Color( 1f, 1f, 1f, .25f );

    protected override void Awake()
    {
        base.Awake();
        type = OptionType.BoolButton;
    }

    public override void InputProcess() { }
    public override void Process() => CurrentBool = !CurrentBool;
    public void OnPointerDown( PointerEventData _eventData ) => Process();
}
