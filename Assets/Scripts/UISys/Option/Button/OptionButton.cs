using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptionButton : OptionBindReturnBase
{
	[Header( "Button" )]
	public bool isPlaySfxSound = true;

	protected override void Awake()
	{
		base.Awake();

		type = OptionType.Button;
	}

	public override void Return()
	{
		if ( isPlaySfxSound )
			 SoundManager.Inst.Play( SoundSfxType.MenuClick );

        Process();
	}

	public override void Process() { }
}
