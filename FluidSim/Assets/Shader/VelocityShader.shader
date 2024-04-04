Shader "Instanced/VelocityShader" {
	Properties { }
	SubShader {
		Tags {"Queue"="Geometry" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5
			#include "UnityCG.cginc"
			
			StructuredBuffer<float3> Positions;
			StructuredBuffer<float3> Velocities;
			Texture2D<float4> ColourMap;
			SamplerState linear_clamp_sampler;
			float maxValue;
			float scale;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 colour : TEXCOORD1;
				float3 normal : NORMAL;
			};

			v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
				float3 worldVertPos = mul(unity_ObjectToWorld, v.vertex * scale) + Positions[instanceID];
				v2f o;
				o.uv = v.texcoord;
				o.normal = v.normal;
				o.pos = UnityObjectToClipPos(worldVertPos);

				float speed = length(Velocities[instanceID]);
				float speedT = saturate(speed / maxValue);
				o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(speedT, 0.5), 0);
				return o;	
			}

			float4 frag (v2f i) : SV_Target
			{
				return float4(i.colour, 1);
			}

			ENDCG
		}
	}
}