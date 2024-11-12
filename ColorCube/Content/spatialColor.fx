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


struct ColoredVertexesVsInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct ColoredVertexesPsInput
{
    float4 Position : SV_Position;
    float3 Color : COLOR;
};


ColoredVertexesPsInput ColoredVertexesVs(ColoredVertexesVsInput input)
{
    ColoredVertexesPsInput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color.rgb;
    return output;
}

float4 ColoredVertexesPs(ColoredVertexesPsInput input) : SV_Target
{
    return float4(input.Color, 1);
}

technique ColoredVertexes
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL ColoredVertexesVs();
        PixelShader = compile PS_SHADERMODEL ColoredVertexesPs();
    }
}
