// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Gellypop/Gel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_GelTex ("Gel Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
			CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#include "UnityCG.cginc"

			#ifdef UNITY_INSTANCING_ENABLED

				UNITY_INSTANCING_CBUFFER_START(PerDrawSprite)
					// SpriteRenderer.Color while Non-Batched/Instanced.
					fixed4 unity_SpriteRendererColorArray[UNITY_INSTANCED_ARRAY_SIZE];
					// this could be smaller but that's how bit each entry is regardless of type
					float4 unity_SpriteFlipArray[UNITY_INSTANCED_ARRAY_SIZE];
				UNITY_INSTANCING_CBUFFER_END

				#define _RendererColor unity_SpriteRendererColorArray[unity_InstanceID]
				#define _Flip unity_SpriteFlipArray[unity_InstanceID]

			#endif // instancing

			CBUFFER_START(UnityPerDrawSprite)
			#ifndef UNITY_INSTANCING_ENABLED
				fixed4 _RendererColor;
				float4 _Flip;
			#endif
			CBUFFER_END

			// Material Color.
			fixed4 _Color;


			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 gel_texcoord : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 gel_texcoord : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 _GelTex_ST;


			v2f SpriteVert(appdata_t IN)
			{
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID (IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			#ifdef UNITY_INSTANCING_ENABLED
				IN.vertex.xy *= _Flip.xy;
			#endif

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.gel_texcoord = TRANSFORM_TEX(IN.texcoord, _GelTex);
				OUT.color = IN.color * _Color * _RendererColor;

				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _GelTex;


			fixed4 SampleTexture (sampler2D tex, float2 uv)
			{
				fixed4 color = tex2D (tex, uv);
				return color;
			}

			fixed4 SpriteFrag(v2f IN) : SV_Target
			{
				fixed4 c = SampleTexture (_MainTex, IN.texcoord) * IN.color;
				fixed4 g = SampleTexture (_GelTex, IN.gel_texcoord) * IN.color;
				return g * c;
			}
			ENDCG
        }
    }
}
