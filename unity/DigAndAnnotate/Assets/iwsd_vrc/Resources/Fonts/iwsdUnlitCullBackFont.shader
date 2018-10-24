// unlit cull back font shader
//
// http://haraken.hatenablog.com/entry/2016/11/29/TextMesh%E3%81%A7%E3%82%AA%E3%83%96%E3%82%B8%E3%82%A7%E3%82%AF%E3%83%88%E3%82%92%E9%80%8F%E9%81%8E%E3%81%95%E3%81%9B%E3%81%AA%E3%81%84%E6%96%B9%E6%B3%95
//
// Based on build in shader "GUI/Text Shader"
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
//
// copied from license.txt
// 
// Copyright (c) 2016 Unity Technologies
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


Shader "Unlit/UnlitCullBackFont"
{
	Properties {
		_MainTex ("Font Texture", 2D) = "white" {}
		_Color ("Text Color", Color) = (1,1,1,1)
	}

	SubShader {

		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
		}
		// Lighting Off Cull Off ZTest Always ZWrite Off
		Lighting Off Cull Back ZTest LEqual ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color;
				col.a *= tex2D(_MainTex, i.texcoord).a;
				return col;
			}
			ENDCG
		}
	}
}
