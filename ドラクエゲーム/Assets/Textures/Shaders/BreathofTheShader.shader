Shader "Custom Shaders/BreathofTheShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		//ambient light is applied uniformly to all surfaces on the object
		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		//controls the size of the specular reflection
		_Glossiness("Glossiness", Float) = 32
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		//control how smoothly the rim blends when approaching unlit parts of the surface
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1		
	}
	SubShader
	{
		Pass
		{
			//setup the pass to use Forward rendering, and only receive data on the main directional light and ambient light.
			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//compile multiple versions of this shader depending on lighting settings
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			//files below include macros and functions to assist with lighting and shadows
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

			struct appdata
			{
				float4 vertex : POSITION;				
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;	
				//macro found in Autolight.cginc. Declares a vector4 into the TEXCOORD2 semantic with varying precision depending on platform target
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			float4 _AmbientColor;

			float4 _SpecularColor;
			float _Glossiness;		

			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;	

            
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);		
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//defined in Autolight.cginc. Assigns the above shadow coordinate by transforming the vertex from world space to shadow-map space
				TRANSFER_SHADOW(o)
				return o;
			}
			
			

            //allows GPU instancing (useful when there are a gorillion objects sharing shaders and materials, such as grass)
            

			float4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				//lighting below is calculated using Blinn-Phong, with values thresholded to create the cel-shaded look
				//https://en.wikipedia.org/wiki/Blinn-Phong_shading_model

				//calculate illumination from directional light
				//_WorldSpaceLightPos0 is a vector pointing the OPPOSITE direction of the main directional light
				float NdotL = dot(_WorldSpaceLightPos0, normal);

				//samples the shadow map, returning a value in the 0...1 range, where 0 is in the shadow, and 1 is not
				float shadow = SHADOW_ATTENUATION(i);
				//partition the intensity into light and dark, smoothly interpolated between the two to avoid a jagged break
				float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);	
				//multiply by the main directional light's intensity and color
				float4 light = lightIntensity * _LightColor0;

				//calculate specular reflection.
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);
				//multiply _Glossiness by itself to allow artist to use smaller glossiness values in the inspector
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;				

				//calculate rim lighting
				float rimDot = 1 - dot(viewDir, normal);
				//we only want rim to appear on the lit side of the surface, so multiply it by NdotL, raised to a power to smoothly blend it
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;

				float4 sample = tex2D(_MainTex, i.uv);

				return (light + _AmbientColor + specular + rim) * _Color * sample;
			}
			ENDCG
		}

		//shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}