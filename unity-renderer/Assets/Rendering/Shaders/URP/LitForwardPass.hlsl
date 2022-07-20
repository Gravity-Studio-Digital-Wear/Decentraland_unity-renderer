#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "Lighting.hlsl"
#include "FadeDithering.hlsl"
#include "Assets/Rendering/Shaders/Outline/OutlinesIncludeV2.hlsl"

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 texcoord1   : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uvAlbedoNormal           : TEXCOORD0; //Albedo, Normal UVs
    //float4 uvMetallicEmissive       : TEXCOORD1; //Metallic, Emissive UVs
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD2;
#endif

    float3 normalWS                 : TEXCOORD3;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
#endif
    float3 viewDirWS                : TEXCOORD5;

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

	//NOTE(Brian): needed for FadeDithering
	float4 positionSS               : TEXCOORD8;

    float4 screenPos                : TEXCOORD9;

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

float SobelFineTuning(float Sobel, float Threshold, float Tightening, float Strength)
{
    float outValue = 0;
    outValue = smoothstep(0, Threshold, Sobel);
    outValue = pow(outValue, Tightening);
    outValue = mul(outValue, Strength);
    return outValue;
}

float SobelOutlines(
    float2 UV, float Thickness, 
    float DepthStrength, float DepthTightening, float DepthThreshold, 
    float AcuteDepthThreshold, float AcuteStartDot, 
    float NormalStrength, float NormalTightening, float NormalThreshold, 
    float FarNormalThreshold, float FarNormalStart, float FarNormalEnd)
{
    float depth;
    float3 depthNormal;
    CalculateDepthNormal_float(UV, depth, depthNormal);

    float3 viewDirectionFromScreen;
    ViewDirectionFromScreenUV_float(UV, viewDirectionFromScreen);

    float tempA = dot(depthNormal, viewDirectionFromScreen);
    tempA = 1 - tempA;

    tempA = smoothstep(AcuteStartDot, 1, tempA);
    tempA = lerp(DepthThreshold, AcuteDepthThreshold, tempA);

    float depthRaw;
    GetSceneDepthRaw(UV, depthRaw);
    tempA = mul(depthRaw, tempA);///

    float depthSobelA;
    DepthSobel_float(UV, Thickness, depthSobelA);

    float resultA = SobelFineTuning(depthSobelA, tempA, DepthTightening, DepthStrength);//////////////////////////////////////////////// Result A

    float depthEye;
    GetSceneDepthEye(UV, depthEye);

    float tempB = smoothstep(FarNormalStart, FarNormalEnd, depthEye);
    tempB = lerp(NormalThreshold, FarNormalThreshold, tempB);///

    float normalSobel;
    NormalsSobel_float(UV, Thickness, normalSobel);

    float resultB = SobelFineTuning(normalSobel, tempB, NormalTightening, NormalStrength);//////////////////////////////////////////////// Result B

    return max(resultA, resultB);
}

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    float2 uvs[] = { TRANSFORM_TEX(input.texcoord, _BaseMap), TRANSFORM_TEX(input.texcoord1, _BaseMap)};
    output.uvAlbedoNormal.xy = uvs[saturate(_BaseMapUVs)];
    output.uvAlbedoNormal.zw = uvs[saturate(_NormalMapUVs)];
    //output.uvMetallicEmissive.xy = uvs[saturate(_MetallicMapUVs)];
    //output.uvMetallicEmissive.zw = uvs[saturate(_EmissiveMapUVs)];

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        real sign = input.tangentOS.w * GetOddNegativeScale();
        half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        output.tangentWS = tangentWS;
    #endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.positionCS = vertexInput.positionCS;

	//NOTE(Brian): needed for FadeDithering
	output.positionSS = ComputeScreenPos(vertexInput.positionCS);
    output.screenPos = ComputeScreenPos(vertexInput.positionCS);

    return output;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_PARALLAXMAP)
    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirTS = input.viewDirTS;
    #else
        half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
    #endif
        ApplyPerPixelDisplacement(viewDirTS, input.uvAlbedoNormal.xy);
    #endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceDataWithUV2(input.uvAlbedoNormal.xy, input.uvAlbedoNormal.zw, input.uvAlbedoNormal.xy, input.uvAlbedoNormal.zw, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

    half4 color = UniversalFragmentPBR(inputData, surfaceData);

    float outlines = 0;// SobelOutlines();
    float2 screenPosition = input.screenPos.xy / input.screenPos.w;

    outlines = SobelOutlines(
            screenPosition, _OutlineThickness / 100,
            _OutlineDepthValues.x, _OutlineDepthValues.y, _OutlineDepthValues.z, 
            _OutlineAcuteDepth, _OutlineAcuteDotStart, 
            _OutlineNormalValues.x, _OutlineNormalValues.y, _OutlineNormalValues.z,
            _OutlineFarNormalValues.x, _OutlineFarNormalValues.y, _OutlineFarNormalValues.z) * _OutlineColor.w;

    color.rgb = MixFog(color.rgb, inputData.fogCoord);    
    color.a = OutputAlpha(color.a, _Surface);    
	color = fadeDithering(color, input.positionWS, input.positionSS);
    color = lerp(color, _OutlineColor, outlines);

    return color;
}


#endif
