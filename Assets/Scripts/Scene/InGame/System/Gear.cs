using UnityEngine;

public class Gear : MonoBehaviour
{
    [SerializeField] private InGame scene;
    [Header( "Gear" )]
    public Transform judge;
    public Transform panel;
    public Transform sideLeft, sideRight;

    public RectTransform hitCount;

    public Transform helpTransform;
    public Transform healthBGTransform;
    public Transform healthRendererTransform;

    private void Awake()
    {
        scene.OnInitialize += Initialize;
    }

    private void OnDestroy()
    {
        scene.OnInitialize -= Initialize;
    }

    private void Initialize( in Song _song )
    {
        UpdatePosition();

        if ( GameSetting.BGAOpacity == 0 )
        {
            panel.gameObject.SetActive( false );
            sideLeft.GetComponent<SpriteRenderer>().color  = Color.black;
            sideRight.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            if ( GameSetting.PanelOpacity == 0 )
                panel.gameObject.SetActive( false );
            else
            {
                panel.GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
                panel.localScale = new Vector3( GameSetting.GearWidth, Global.UI.Screen.Height );
                panel.position   = new Vector2( GameSetting.GearOffsetX, 0f );
            }
        }

        sideLeft.position                = new Vector3( GameSetting.GearStartPos, 0f );
        sideRight.position               = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth, 0f );
        helpTransform.position           = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 5f,  ( -Global.UI.Screen.Height * .5f ) + 50f, 0f );
        healthBGTransform.position       = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 17f, ( -Global.UI.Screen.Height * .5f ) + ( helpTransform.localScale.y * .5f ), 0f );
        healthRendererTransform.position = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 33f, ( -Global.UI.Screen.Height * .5f ) + helpTransform.localScale.y, 0f );
    }

    private void UpdatePosition()
    {
        judge.position   = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos );
        judge.localScale = new Vector3( GameSetting.GearWidth, judge.localScale.y );
    }
}
