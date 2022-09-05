using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSkinOption : OptionText
{
    public Image left, center, right;

    private void OnEnable()
    {
        curIndex = ( int )GameSetting.CurrentNoteSkin.type;
        ChangeText( texts[curIndex] );

        left.sprite   = GameSetting.CurrentNoteSkin.left.normal;
        center.sprite = GameSetting.CurrentNoteSkin.center.normal;
        right.sprite  = GameSetting.CurrentNoteSkin.right.normal;
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )NoteSkinType.Count; i++ )
        {
            texts.Add( ( ( NoteSkinType )i ).ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.CurrentNoteSkin = GameSetting.Inst.NoteSkins[curIndex];
        left.sprite   = GameSetting.CurrentNoteSkin.left.normal;
        center.sprite = GameSetting.CurrentNoteSkin.center.normal;
        right.sprite  = GameSetting.CurrentNoteSkin.right.normal;
    }
}
