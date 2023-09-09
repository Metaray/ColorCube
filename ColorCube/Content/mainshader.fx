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


struct ColoredVertexesVsInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct ColorParticlesInstanceVsInput
{
    float4 Position : POSITION1;
    float4 Color : COLOR0;
};

struct ColorParticlesQuadBaseVsInput
{
    float3 Position : POSITION0;
};

struct ColoredVertexesVsOutput
{
    float4 Position : SV_Position;
    float3 Color : COLOR;
};


ColoredVertexesVsOutput ColoredVertexesVs(ColoredVertexesVsInput input)
{
    ColoredVertexesVsOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color.rgb;
    return output;
}

float4 ColoredVertexesPs(ColoredVertexesVsOutput input) : SV_Target
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


ColoredVertexesVsOutput ColorParticlesVs(ColorParticlesQuadBaseVsInput quad, ColorParticlesInstanceVsInput instance)
{
    float4 projPosition = mul(instance.Position, WorldViewProjection);
    projPosition.xy += quad.Position.xy * projPosition.w * InvScreenSize;

    ColoredVertexesVsOutput output;
    output.Position = projPosition;
    output.Color = instance.Color.rgb;
    return output;
}

float4 ColorParticlesPs(ColoredVertexesVsOutput input) : SV_Target
{
    return float4(input.Color, 1);
}

technique ColorParticles
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL ColorParticlesVs();
        PixelShader = compile PS_SHADERMODEL ColorParticlesPs();
    }
}
