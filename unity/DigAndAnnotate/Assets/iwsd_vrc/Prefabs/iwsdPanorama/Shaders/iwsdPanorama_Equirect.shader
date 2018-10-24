// iwsd panorama shader
// handles panoramic formated texture
//
// Copyright naqtn. MIT license


Shader "iwsd/Panorama" {

    Properties {
        _MainTex ("MainTex", 2D) = "gray" {}

        [Enum(UnityEngine.Rendering.CullMode)]
	_Cull ("Culling face", Float) = 1

	// Where original panorama view point origin is placed to.
	// 0.0 : camera center "like a window"
	// 1.0 : model center "paint on object"
        _ObjectCenterWeight ("Object center weight", Range(0,1) ) = 0

	// Coordinate that panorama image direction is aligned to :
	// World space (0.0) - object space (1.0)
        _ObjectDirectionWeight ("Object space direction weight", Range(0,1) ) = 0

	// Original Panorama view point origin is camera or model center in degree.
        _RotationYInDegrees ("Y-axis rotation", Range(0,360) ) = 0

	// VRChat Panorama camera is Over_Under L_R
	//  Equirectangular_OverUnder_LeftRight, side by side
	[Enum(Equirectangular_Mono,0,Equirectangular_OverUnder_LeftRight,1)]
	_ImageLayout ("Image layout", Float) = 1

	// force non-stereoscopic (monaural. use single image to both eyes)
        [Enum(No, 0, UseLeftOnly, 1, UseRightOnly, 2)]
	_ForceNonStereo ("Force non-stereoscopic", Float) = 0

        // Mirror flip
        [MaterialToggle]
	_Flipped ("Flipped", Float ) = 0

        [MaterialToggle]
	_GammaToLinear ("GammaToLinear(degamma)", Float ) = 0
    }

    SubShader {

	// TODO Transparent variation
        // Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Pass {
            // Blend SrcAlpha OneMinusSrcAlpha

            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 // for texture LOD sampling (tex2Dlod)

            #include "UnityCG.cginc"
            #include "iwsdPanoramaFuncs.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _ObjectCenterWeight;
            fixed _ObjectDirectionWeight;
	    fixed _RotationYInDegrees;
	    fixed _ImageLayout;
            fixed _ForceNonStereo;
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

		// Origin of direction vector (= where you put view point of panorama image)
                float3 origin = lerp(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz, _ObjectCenterWeight);

		// Direction to this fragment
                float3 direction = lerp(i.posWorld.xyz - origin.xyz, i.dirModel.xyz, _ObjectDirectionWeight);

		direction = RotateYInDegrees(direction, _RotationYInDegrees);

		// Flip z if needed (= mirrored)
                direction = direction * lerp(float3(+1,+1,+1), float3(+1,+1,-1), _Flipped);

                // Extract uv depending on panorama image format
                float2 uv = lerp( CoordsLatLonMono(direction), CoordsLatLonOverUnder(direction, _ForceNonStereo), _ImageLayout);

                uv = TRANSFORM_TEX(uv, _MainTex);

		// Get color from texture
                // use tex2Dlod to avoid seam. with LOD = 0
                // https://answers.unity.com/questions/755222/how-do-i-fix-texture-seam-from-uv-spherical-mappin.html
                fixed4 col = tex2Dlod(_MainTex, float4(uv.x, uv.y, 0, 0));

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
