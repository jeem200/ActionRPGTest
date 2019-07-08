// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom Shaders/SimpleGrassShader"
{
    Properties {
        _Color("Color (RGBA)", COLOR) = (1, 1, 1, 1)
    }
    SubShader 
    {
        CGPROGRAM
        #pragma surface surf Lambert
        
        struct Input {
            fixed4 color : COLOR;
        };
        
        fixed4 _Color;
        
        void surf (Input IN, inout SurfaceOutput o) 
        {
            o.Albedo = _Color;
        }
    ENDCG
}