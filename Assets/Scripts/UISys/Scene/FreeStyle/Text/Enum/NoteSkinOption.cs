using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSkinOption : OptionText
{
    public Image left, center, right;

    private void OnEnable()
    {
        curIndex = ( int )SkinManager.CurrentNoteSkin.type;
        ChangeText( texts[curIndex] );

        left.sprite   = SkinManager.CurrentNoteSkin.left.normal;
        center.sprite = SkinManager.CurrentNoteSkin.center.normal;
        right.sprite  = SkinManager.CurrentNoteSkin.right.normal;
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
        SkinManager.CurrentNoteSkin = SkinManager.Inst.NoteSkins[curIndex];
        left.sprite   = SkinManager.CurrentNoteSkin.left.normal;
        center.sprite = SkinManager.CurrentNoteSkin.center.normal;
        right.sprite  = SkinManager.CurrentNoteSkin.right.normal;
    }
}
