using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer( typeof( TitleAttribute ) )]
public class TitleDrawer : PropertyDrawer
{
    public override void OnGUI( Rect _position, SerializedProperty _property, GUIContent _label )
    {
        Rect labelRect = new Rect( _position.x, _position.y, EditorGUIUtility.labelWidth, _position.height );
        Rect fieldRect = new Rect( _position.x + EditorGUIUtility.labelWidth, _position.y,_position.width - EditorGUIUtility.labelWidth, _position.height );

        EditorGUI.LabelField( labelRect, _label );
        bool prevState = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField( fieldRect, _property, GUIContent.none, true );
        GUI.enabled = prevState;
    }

    public override float GetPropertyHeight( SerializedProperty _property, GUIContent _label )
    {
        return EditorGUI.GetPropertyHeight( _property, _label, true );
    }
}