Shader "Instanced/CollisionShader" {
	Properties { }
	SubShader {
		Tags {"Queue"="Geometry" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5
			#include "UnityCG.cginc"
			
			// varaibles
			StructuredBuffer<float3> Positions;
			StructuredBuffer<float3> Collisions;
			Texture2D<float4> ColourMap;
			SamplerState linear_clamp_sampler;
			float maxValue;
			float scale;

			// shader data
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 colour : TEXCOORD1;
				float3 normal : NORMAL;
			};

			// vertex shader
			v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
				// set the position
				float3 worldVertPos = mul(unity_ObjectToWorld, v.vertex * scale) + Positions[instanceID];
				// set shader data 
				v2f o;
				o.uv = v.texcoord;
				o.normal = v.normal;
				o.pos = UnityObjectToClipPos(worldVertPos);

				// caluclate the average distance
				float dist = distance(Collisions[instanceID], Positions[instanceID]);
				float averageDist = dist / maxValue;
				// calcualte the color of the current pixel
				o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(averageDist, 0.5), 0);
				return o;	
			}

			// fragment shader
			float4 frag (v2f i) : SV_Target
			{
				// set the color of the current pixel
				return float4(i.colour, 1);
			}

			ENDCG
		}
	}
}