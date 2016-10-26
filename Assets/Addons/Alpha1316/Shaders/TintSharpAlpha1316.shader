Shader "Alpha1316/Tint (Sharp)"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;

				return OUT;
			}

			sampler2D _MainTex;

            //SILP: A1316_GET_COLOR_SHARP()
            fixed4 a1316_get_color(sampler2D tex, float2 uv)                                        //__SILP__
            {                                                                                       //__SILP__
                float2 color_coord = uv * 0.8125;                                                   //__SILP__
                                                                                                    //__SILP__
                fixed4 c = tex2D(tex, color_coord);                                                 //__SILP__
                                                                                                    //__SILP__
                float texX = uv.x;                                                                  //__SILP__
                                                                                                    //__SILP__
                int segment = (int)((texX * 16.0 + 0.4) / 2.8);                                     //__SILP__
                float center = (1.0 + (2.8 * segment)) / 16.0;                                      //__SILP__
                                                                                                    //__SILP__
                float2 alpha_coord;                                                                 //__SILP__
                alpha_coord.x = 0.8125 + 0.1875 * clamp(0.5 + (texX - center) * 13.0 / 3.0, 0, 1);  //__SILP__
                alpha_coord.y = color_coord.y;                                                      //__SILP__
                                                                                                    //__SILP__
                int index = segment;                                                                //__SILP__
                if (segment >= 3) {                                                                 //__SILP__
                    // Note: on the android devices, if using                                       //__SILP__
                    //`int index = segment % 3;`, the index might have wrong                        //__SILP__
                    // value when segment is 3;                                                     //__SILP__
                    // Probably it's because the mod() is converted to float parameter              //__SILP__
                    // and caused precision problem.                                                //__SILP__
                    index -= 3;                                                                     //__SILP__
                                                                                                    //__SILP__
                    float tmp = alpha_coord.y;                                                      //__SILP__
                    alpha_coord.y = alpha_coord.x;                                                  //__SILP__
                    alpha_coord.x = tmp;                                                            //__SILP__
                }                                                                                   //__SILP__
                                                                                                    //__SILP__
                fixed4 alpha = tex2D(tex, alpha_coord);                                             //__SILP__
                                                                                                    //__SILP__
                c.a = index == 0 ? alpha.r : (index == 1 ? alpha.g : alpha.b);                      //__SILP__
                return c;                                                                           //__SILP__
            }                                                                                       //__SILP__
                                                                                                    //__SILP__
            fixed4 a1316_get_color_sharp(sampler2D tex, float2 uv)                                  //__SILP__
            {                                                                                       //__SILP__
                fixed4 c = a1316_get_color(tex, uv);                                                //__SILP__
                half a = c.a;                                                                       //__SILP__
                                                                                                    //__SILP__
                //ETC1 Compression will bring some messy pixels in complex                          //__SILP__
                //images, this can solve that in many cases.                                        //__SILP__
                //You may want to create a copy and tweak the parameter here to                     //__SILP__
                //get better result.                                                                //__SILP__
                if (a < 0.5) {                                                                      //__SILP__
                    a = a * a;                                                                      //__SILP__
                    if (a < 0.15) {                                                                 //__SILP__
                        a = 0;                                                                      //__SILP__
                    }                                                                               //__SILP__
                } else {                                                                            //__SILP__
                    a = 1 - a;                                                                      //__SILP__
                    a = 1 - a * a;                                                                  //__SILP__
                    if (a > 0.85) {                                                                 //__SILP__
                        a = 1;                                                                      //__SILP__
                    }                                                                               //__SILP__
                }                                                                                   //__SILP__
                c.a = a;                                                                            //__SILP__
                return c;                                                                           //__SILP__
            }                                                                                       //__SILP__

			fixed4 SampleSpriteTexture (float2 uv)
			{
				//fixed4 color = tex2D (_MainTex, uv);
				fixed4 color = a1316_get_color_sharp (_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
