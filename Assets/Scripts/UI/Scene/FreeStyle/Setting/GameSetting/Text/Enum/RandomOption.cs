using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class RandomOption : OptionText
{
    public TextMeshProUGUI previewText;

    private void OnEnable()
    {
        CurrentIndex = ( int )GameSetting.CurrentRandom;
        ChangeText( texts[CurrentIndex] );
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
        GameSetting.CurrentRandom = ( GameRandom )CurrentIndex;
        ChangeText( texts[CurrentIndex] );
        previewText.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
    }
}