// iwsd panorama shader
// for six sided camera system
//
// Copyright naqtn. MIT license


Shader "iwsd/Panorama_SixSided" {
    
    Properties {
        [NoScaleOffset] _FrontTex ("Front Texture", 2D) = "gray" {}
        [NoScaleOffset] _BackTex  ("Back Texture", 2D) = "gray" {}
        [NoScaleOffset] _LeftTex  ("Left Texture", 2D) = "gray" {}
        [NoScaleOffset] _RightTex ("Right Texture", 2D) = "gray" {}
        [NoScaleOffset] _UpTex    ("Up Texture", 2D) = "gray" {}
        [NoScaleOffset] _DownTex  ("Down Texture", 2D) = "gray" {}

        [Enum(UnityEngine.Rendering.CullMode)]
	_Cull ("Culling face (CullMode)", Float) = 1

	_ObjectCenterWeight ("Object center weight", Range(0,1) ) = 0
        _ObjectDirectionWeight ("Object space direction weight", Range(0,1) ) = 0
        _RotationYInDegrees ("Y-axis rotation", Range(0,360) ) = 0

        [MaterialToggle]
	_Flipped ("Flipped", Float ) = 0

        [MaterialToggle]
	_GammaToLinear ("GammaToLinear(degamma)", Float ) = 0
    }
    
    SubShader {

        Tags
        {
        }
	
        Pass {
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 // for texture LOD sampling (tex2Dlod)
            
            #include "UnityCG.cginc"
            #include "iwsdPanoramaFuncs.cginc"

	    sampler2D _FrontTex;
	    sampler2D _BackTex;
	    sampler2D _LeftTex;
	    sampler2D _RightTex;
	    sampler2D _UpTex;
	    sampler2D _DownTex;
            fixed _ObjectCenterWeight;
            fixed _ObjectDirectionWeight;
	    fixed _RotationYInDegrees;
            fixed _Flipped;
	    fixed _GammaToLinear;
            

	    struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
		float4 dirModel : TEXCOORD1;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		o.dirModel = v.vertex - float4(0,0,0,1);
                return o;
            }

	    
            fixed4 frag(v2f i) : SV_Target {
                float3 origin = lerp(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz, _ObjectCenterWeight);
                float3 direction = lerp(i.posWorld.xyz - origin.xyz, i.dirModel.xyz, _ObjectDirectionWeight);
		direction = RotateYInDegrees(direction, _RotationYInDegrees);
                direction = direction * lerp(float3(+1,+1,+1), float3(+1,+1,-1), _Flipped);

		float fovInDegrees = 90;
		float fovInRad = fovInDegrees * UNITY_PI / 180.0;
		fixed4 outOfRangeColor = fixed4(0,0,0,0);

                fixed4 col
		    = ProjectTex(float3(+direction.x, +direction.y, +direction.z), fovInRad, _RightTex, outOfRangeColor)
		    + ProjectTex(float3(+direction.z, +direction.y, -direction.x), fovInRad, _FrontTex, outOfRangeColor)
		    + ProjectTex(float3(-direction.z, +direction.y, +direction.x), fovInRad, _BackTex, outOfRangeColor)
		    + ProjectTex(float3(-direction.x, +direction.y, -direction.z), fovInRad, _LeftTex, outOfRangeColor)
		    + ProjectTex(float3(+direction.y, -direction.z, -direction.x), fovInRad, _UpTex, outOfRangeColor)
		    + ProjectTex(float3(-direction.y, +direction.z, -direction.x), fovInRad, _DownTex, outOfRangeColor);

		if (_GammaToLinear > 0) {
		    col.rgb = GammaToLinearSpace(col.rgb);
		}

                return col;
            }

            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
