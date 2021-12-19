Shader "NewUnlitShader"
{
    Properties
    {
        _Color ("MainColor", Color) = (1,1,1,0.5)
        _SpecColor ("Spec Color", Color) = (1,1,1,1)
        _Emission ("Emmisive Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range( 0.01, 1)) = 0.7
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Material
            {
                Diffuse[_Color]
                Ambient[_Color]
                Shininess[_Color]
                Specular[_Color]
                Emission[_Color]
            }
            Lighting On
            SeparateSpecular On
            SetTexture[_MainTex]
            {
                constantColor[_Color]
                Combine texture * primary DOUBLE, texture * constant
            }
        }
    }
}
