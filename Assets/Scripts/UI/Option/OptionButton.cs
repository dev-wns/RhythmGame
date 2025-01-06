using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptionButton : OptionBase
{
	[Header( "Button" )]
	public bool isPlaySfxSound = true;
	public UnityEvent ButtonEvent;

	protected override void Awake()
	{
		base.Awake();

		type = OptionType.Button;
		ButtonEvent ??= new UnityEvent();
	}

    public override void InputProcess()
    {
		if ( Input.GetKeyDown( KeyCode.Return ) )
		{
			if ( isPlaySfxSound )
				 AudioManager.Inst.Play( SFX.MenuClick );

			Process();
		}
    }

	public override void Process() 
	{
		ButtonEvent?.Invoke();
	}
}