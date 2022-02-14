using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class RandomOption : OptionText
{
    public TMPro.TextMeshProUGUI settingText;

    private void OnEnable()
    {
        curIndex = ( int )GameSetting.CurrentRandom;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )GameRandom.Count; i++ )
        {
            var text = ( ( GameRandom )i ).ToString();
            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );
            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.CurrentRandom = ( GameRandom )curIndex;
        settingText.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        Debug.Log( ( GameRandom )curIndex );
    }
}