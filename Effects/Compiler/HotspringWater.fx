float time;
float2 screenSize;
float2 offset;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture3;
sampler2D samplerTex3 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 st = screenSpace.xy;

    float power = tex2D(samplerTex2, st).a;
    float4 color = tex2D(samplerTex, st);

    float factor = (0.8 + sin(power * 30.0 + time) * 0.1);
    float bright = ((color.r + color.b + color.g) / 3.0);

    float4 color2 = float4(0.0, 0.0, 0.0, color.a);

    color2.r = lerp(color.r, 0.8 * bright, (power + factor) / 2.0);
    color2.g = lerp(color.g, 1.3 * bright, (power + factor) / 2.0);
    color2.b = lerp(color.b, 1.2 * bright, (power + factor) / 2.0);

    return color2 * ((1.0 - power * 0.8) + tex2D(samplerTex3, st) * 0.1f * bright);
}

technique Technique1
{
    pass PrimitivesPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};