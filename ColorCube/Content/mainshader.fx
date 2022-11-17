#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float2 InvScreenSize;

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : NORMAL0;
	float4 Displacemet : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    //viewPosition += input.Displacemet;
    float4 projPosition = mul(viewPosition, Projection);
    projPosition.xy += input.Displacemet.xy * projPosition.w * InvScreenSize.xy;

    VertexShaderOutput output;
    output.Position = projPosition;
    output.Color = input.Color;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

technique SpatialColorizationRgb
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
