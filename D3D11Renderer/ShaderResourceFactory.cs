using RenderingPipe.Resources;
using System;

namespace D3D11
{
    public class ShaderResourceFactory
    {
        public static ShaderResource CreateFromSource(ShaderStage stage, string source = SAMPLE_SHADER)
        {
            switch (stage)
            {
                case ShaderStage.Vertex:
                    return ShaderResource.Create(ShaderStage.Vertex
                        , SharpDX.D3DCompiler.ShaderBytecode.Compile(source, "VS", "vs_4_0"));

                case ShaderStage.Pixel:
                    return ShaderResource.Create(ShaderStage.Pixel
                        , SharpDX.D3DCompiler.ShaderBytecode.Compile(source, "PS", "ps_4_0"));

                default:
                    throw new NotImplementedException();
            }
        }

        #region THROUGH_SHADER
        public const String THROUGH_SHADER = @"
struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
};

struct VS_OUT
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

typedef VS_OUT PS_IN;

VS_OUT VS(VS_IN input)
{
	VS_OUT output = (VS_OUT)0;
	
	output.pos = input.pos;
	output.col = input.col;
	
	return output;
}

float4 PS(PS_IN input ): SV_TARGET0
{
    return input.col;
}
";
        #endregion

        #region WVP_SHADER
        public static string WVP_SHADER = @"
struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

cbuffer c0 {
    row_major float4x4 world;
    row_major float4x4 view;
    row_major float4x4 projection;
};

PS_IN VS( float4 pos: POSITION,  float4 col: COLOR )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(pos, mul(world, mul(view, projection)));
	output.col = col;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.col;
}
";
        #endregion

        #region SAMPLE_SHADER
        public const String SAMPLE_SHADER = @"
Texture2D	tex0		: register( t0 );
SamplerState	sample0		: register( s0 );


struct VS_IN
{
    float4 pos: POSITION;
    float4 normal: NORMAL;
    float4 color: COLOR;
    float2 tex: TEXCOORD0;
};

struct VS_OUT
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
    float2 tex: TEXCOORD0;
};

typedef VS_OUT PS_IN;

struct PS_OUT
{
    float4 color: SV_Target;
};

cbuffer c0 {
    row_major float4x4 world;
    row_major float4x4 view;
    row_major float4x4 projection;
    float4 diffuse;
};

VS_OUT VS(VS_IN input)
{
	VS_OUT output = (VS_OUT)0;
	
	output.pos = mul(input.pos, mul(world, mul(view, projection)));
    output.color = diffuse;
    output.tex = input.tex;
	
	return output;
}

PS_OUT PS(PS_IN input)
{
    PS_OUT output = (PS_OUT)0;

    float4 color= tex0.Sample(sample0, input.tex);
    output.color=color * input.color;

	return output;
}
";
        #endregion
    }
}
