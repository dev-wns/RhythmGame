using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptionButton : OptionBindReturnBase
{
	protected override void Awake()
	{
		base.Awake();

		type = OptionType.Button;
	}

	public override void Return()
	{
		SoundManager.Inst.PlaySfx( SoundSfxType.Return );
		Process();
	}

	public override void Process() { }
}
