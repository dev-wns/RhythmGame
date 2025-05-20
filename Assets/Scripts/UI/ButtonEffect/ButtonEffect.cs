using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent( typeof( Image ) )]
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public virtual void OnPointerEnter( PointerEventData eventData )
    {

    }

    public virtual void OnPointerExit( PointerEventData eventData )
    {

    }
}
