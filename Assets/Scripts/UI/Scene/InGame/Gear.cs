using UnityEngine;

public class Gear : MonoBehaviour
{
    public Transform judge;
    public Transform panel;
    public Transform sideLeft, sideRight;

    public RectTransform hitCount;

    public Transform helpTransform;
    public Transform healthBGTransform;
    public Transform healthRendererTransform;

    private void Start()
    {
        UpdatePosition();

        if ( GameSetting.PanelOpacity <= .01f ? false : true )
             panel.GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
        else
             panel.gameObject.SetActive( false );

        panel.localScale   = new Vector3( GameSetting.GearWidth, Screen.height );
        panel.position     = new Vector2( GameSetting.GearOffsetX, 0f );
        sideLeft.position  = new Vector3( GameSetting.GearStartPos, 0f );
        sideRight.position = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth, 0f );


        helpTransform.position           = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 5f, ( -Screen.height * .5f ) + 50f, 0f );
        healthBGTransform.position       = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 17f, ( -Screen.height * .5f ) + ( helpTransform.localScale.y * .5f ), 0f );
        healthRendererTransform.position = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 33f, ( -Screen.height * .5f ) + helpTransform.localScale.y, 0f );

        //hitCount.anchoredPosition = new Vector2( helpTransform.position.x + ( hitCount.sizeDelta.x * .5f ) + 51f, 
        //                                         helpTransform.position.y + ( hitCount.sizeDelta.y * .5f ) + 26f );
    }

    private void UpdatePosition()
    {
        judge.position   = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos );
        judge.localScale = new Vector3( GameSetting.GearWidth, judge.localScale.y );
    }
}
