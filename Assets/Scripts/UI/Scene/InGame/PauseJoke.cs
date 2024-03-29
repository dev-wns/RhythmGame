using TMPro;
using UnityEngine;

public class PauseJoke : MonoBehaviour
{
    private TextMeshProUGUI jokeText;
    private string[] jokeList = { "����" }; //{ "����", "��Ÿ", "�", "����", "Ʈ��", "����" };

    private void Awake()
    {
        if ( !TryGetComponent<TextMeshProUGUI>( out jokeText ) )
             Destroy( this );
    }

    private void OnEnable()
    {
        jokeText.text = jokeList[Random.Range( 0, jokeList.Length - 1 )];
    }
}
