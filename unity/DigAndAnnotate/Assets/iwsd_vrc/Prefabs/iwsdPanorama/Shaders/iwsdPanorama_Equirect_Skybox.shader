// iwsd panorama image skybox
//
// functionally, subset of iwsd/Panorama
//
// Copyright naqtn. MIT license

Shader "iwsd/Panorama_Skybox" {

    Properties {
        _MainTex ("MainTex", 2D) = "gray" {}

        _RotationYInDegrees ("Y-axis rotation", Range(0,360) ) = 0

	[Enum(Equirectangular_Mono,0,Equirectangular_OverUnder_LeftRight,1)]
	_ImageLayout ("Image layout", Float) = 1

        [Enum(No, 0, UseLeftOnly, 1, UseRightOnly, 2)]
	_ForceNonStereo ("Force non-stereoscopic", Float) = 0

        [MaterialToggle]
	_Flipped ("Flipped", Float ) = 1

        [MaterialToggle]
	_GammaToLinear ("GammaToLinear(degamma)", Float ) = 0
    }

    SubShader {

        Tags
        {
            "RenderType"="Background"
            "Queue"="Background"
            "PreviewType"="SkyBox"
        }

        Pass {
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 // for texture LOD sampling (tex2Dlod)

            #include "UnityCG.cginc"
            #include "iwsdPanoramaFuncs.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
	    fixed _RotationYInDegrees;
	    fixed _ImageLayout;
            fixed _ForceNonStereo;
            fixed _Flipped;
	    fixed _GammaToLinear;


            struct appdata {
                float4 vertex : POSITION;
                float3 direction : TEXCOORD0;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float3 direction : TEXCOORD0;
            };


            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.direction = v.direction;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float3 direction = i.direction;

		direction = RotateYInDegrees(direction, _RotationYInDegrees);

                direction = direction * lerp(float3(+1,+1,+1), float3(+1,+1,-1), _Flipped);

                float2 uv = lerp( CoordsLatLonMono(direction), CoordsLatLonOverUnder(direction, _ForceNonStereo), _ImageLayout);

                uv = TRANSFORM_TEX(uv, _MainTex);

                fixed4 col = tex2Dlod(_MainTex, float4(uv.x, uv.y, 0, 0));

		if (_GammaToLinear > 0) {
		    col.rgb = GammaToLinearSpace(col.rgb);
		}

                return col;
            }

            ENDCG
        }
    }

    Fallback Off
}
