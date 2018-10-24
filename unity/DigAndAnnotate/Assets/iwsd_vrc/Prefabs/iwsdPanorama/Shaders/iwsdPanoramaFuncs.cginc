// iwsd Panorama common functions
//
// Copyright naqtn. MIT license

#define M_PI    3.14159265358979323846
#define M_1_PI  0.318309886183790671538 // 1/pi
#define M_1_2PI 0.159154943091895335769 // 1/(2pi)
#define M_2_PI  0.636619772367581343076 // 2/pi

////////////////////////////////////////////////////////////////////////////////

/*
 * Latitude Longitude (Equirectangular, LatLong, Cylindrical) panorama, 
 * Over-Under (above below) stereo
 *
 * float forceNonStereo: No, 0, UseLeftOnly, 1, UseRightOnly, 2
 */
float2 CoordsLatLonOverUnder(float3 direction, float forceNonStereo) {
    float latitude = acos(normalize(direction).y); 
    float longitude = atan2(direction.z, direction.x);

    float2 coords = float2(longitude, latitude); // (-PI..+PI, PI..0)
    coords = coords * float2(M_1_2PI, M_1_PI);  // => (-0.5..0.5, 1..0)
    coords = float2(0.5, 1.0) - coords; // => (1..0, 0..1)

    // unity_StereoEyeIndex 0 for left eye
    float eyeIdx = (forceNonStereo == 0.0)? unity_StereoEyeIndex: ((forceNonStereo == 1.0)? 0: 1);

    return (coords + float2(0, (1 - eyeIdx))) * float2(1.0, 0.5);  // (, 0..0.5) or (, 0.5..1)
}


/*
 * Latitude Longitude panorama, 
 * monoral
 */
float2 CoordsLatLonMono(float3 direction) {
    float latitude = acos(normalize(direction).y);
    float longitude = atan2(direction.z, direction.x);
    
    float2 coords = float2(longitude, latitude);
    coords = coords * float2(M_1_2PI, M_1_PI);
    coords = float2(0.5, 1.0) - coords;

    return coords;
}


////////////////////////////////////////////////////////////////////////////////

float3 RotateYInDegrees(float3 vec, float degree) {
    // if (degree == 0.0) {
    // return vec;
    // }

    float theta = degree * M_PI / 180.0;
    float sint, cost;
    sincos(theta, sint, cost);

    // float3x3 rx = float3x3(
    //     1, 0, 0,
    //     0, cost, -sint,
    //     0, sint, cost
    // );
    float3x3 ry = float3x3(
	cost, 0, sint,
   	0, 1, 0,
	    -sint, 0, cost
    );
    // float3x3 rz = float3x3(
    //     cost, -sint, 0,
    //     sint, cost, 0,
    //     0, 0, 1
    // );

    return mul(ry, vec);
}


////////////////////////////////////////////////////////////////////////////////

// project texture at rigiht side (+, 0, 0)
//
// equi angle
fixed4 ProjectTex(float3 direction, float fovInRad, sampler2D tex, fixed4 outOfRangeColor) {
    float u0 = atan2(direction.z, direction.x);
    float v0 = atan2(direction.y, +direction.x);
    
    float u = (- u0 / fovInRad + 1.0/2.0);
    float v = (+ v0 / fovInRad + 1.0/2.0);
    
    return ((0 <= u) && (u <= 1.0) && (0 <= v) && (v <= 1.0))?
        tex2Dlod(tex, float4(u, v, 0, 0)):
	outOfRangeColor;
}

// as plate
fixed4 Project2Tex_X(float3 direction, float fovInRad, sampler2D tex, fixed4 outOfRangeColor) {

    float planeDistance = 1.0 / tan(fovInRad/2.0) / 2.0;

    float phi = atan2(-direction.z, direction.x);
    // float theta = atan2(direction.y, length(direction.xz));
    float theta = atan2(direction.y, sqrt(direction.x*direction.x+direction.z*direction.z));

    float u = planeDistance * tan(phi) + 1.0/2.0;
    float v = planeDistance * tan(theta)/cos(phi) + 1.0/2.0;

    return ((0 <= direction.x) && (0 <= u) && (u <= 1.0) && (0 <= v) && (v <= 1.0))?
        tex2Dlod(tex, float4(u, v, 0, 0)):
	outOfRangeColor;
}

fixed4 Project2Tex(float3 direction, float fovInRad, sampler2D tex, fixed4 outOfRangeColor) {
    float planeDistance = 1.0 / tan(fovInRad/2.0) / 2.0;

    float phi = atan2(direction.x, direction.z);
    float theta = atan2(direction.y, length(direction.xz));

    float u = planeDistance * tan(phi) + 1.0/2.0;
    float v = planeDistance * tan(theta)/cos(phi) + 1.0/2.0;

    return ((0 <= direction.z) && (0 <= u) && (u <= 1.0) && (0 <= v) && (v <= 1.0))?
        tex2Dlod(tex, float4(u, v, 0, 0)):
	outOfRangeColor;
}
