#if OPENGL
    #define SV_Position POSITION
    #define SV_Target COLOR
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_3
    #define PS_SHADERMODEL ps_4_0_level_9_3
#endif

float4x4 WorldViewProjection;
float2 InvScreenSize;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float3 Color : NORMAL;
    float2 Displacemet : TEXCOORD;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float3 Color : COLOR;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    float4 projPosition = mul(input.Position, WorldViewProjection);
    projPosition.xy += input.Displacemet * projPosition.w * InvScreenSize;

    VertexShaderOutput output;
    output.Position = projPosition;
    output.Color = input.Color;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    return float4(input.Color, 1);
}

technique SpatialColorizationRgb
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
