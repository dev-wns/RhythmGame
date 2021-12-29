using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomButton : MonoBehaviour, IOptionButton
{
	public OptionType type { get; set; } = OptionType.Button;

	public int key { get; set; }

	public UnityEvent CustomEvent;

	private void Awake()
	{
		if ( CustomEvent == null )
			 CustomEvent = new UnityEvent();
	}

	public virtual void Process()
	{
		CustomEvent.Invoke();
	}
}
