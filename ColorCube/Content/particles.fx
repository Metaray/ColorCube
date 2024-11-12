#if OPENGL
    #define SV_Position POSITION
    #define SV_Target COLOR
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_3
    #define PS_SHADERMODEL ps_4_0_level_9_3
#endif


//float4x4 WorldViewProjection;
float4x4 WorldView;
float4x4 Projection;
//float2 InvScreenSize;


struct ColorParticlesInstanceVsInput
{
    float4 Position : POSITION1;
    float4 Color : COLOR0;
};

struct ColorParticlesQuadBaseVsInput
{
    float3 Position : POSITION0;
};

struct ColoredVertexesPsInput
{
    float4 Position : SV_Position;
    float3 Color : COLOR;
};

ColoredVertexesPsInput ColorParticlesVs(ColorParticlesQuadBaseVsInput quad, ColorParticlesInstanceVsInput instance)
{
    float4 projPosition;
    
    //projPosition = mul(instance.Position, WorldViewProjection);
    //projPosition.xy += quad.Position.xy * projPosition.w * InvScreenSize;
    
    float4 viewPosition = mul(instance.Position, WorldView);
    viewPosition.xy += quad.Position.xy;
    projPosition = mul(viewPosition, Projection);

    ColoredVertexesPsInput output;
    output.Position = projPosition;
    output.Color = instance.Color.rgb;
    return output;
}

float4 ColorParticlesPs(ColoredVertexesPsInput input) : SV_Target
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
