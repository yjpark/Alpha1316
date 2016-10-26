Shader "Sprites/Alpha 1316"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
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
                half2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;

            // This is the only special logic for the ETC1WithAlpha rendering
            fixed4 frag(v2f IN) : SV_Target
            {
                float2 color_coord = IN.texcoord * 0.8125;

                fixed4 c = tex2D(_MainTex, color_coord);

                float texX = IN.texcoord.x;

                int segment = (int)((texX * 16.0 + 0.4) / 2.8);
                float center = (1.0 + (2.8 * segment)) / 16.0;

                float2 alpha_coord;
                alpha_coord.x = 0.8125 + 0.1875 * clamp(0.5 + (texX - center) * 13.0 / 3.0, 0, 1);
                alpha_coord.y = color_coord.y;

                int index = segment;
                if (segment >= 3) {
                    // Note: on the android devices, if using
                    //`int index = segment % 3;`, the index might have wrong
                    // value when segment is 3;
                    // Probably it's because the mod() is converted to float parameter
                    // and caused precision problem.
                    index -= 3;

                    float tmp = alpha_coord.y;
                    alpha_coord.y = alpha_coord.x;
                    alpha_coord.x = tmp;
                }

                fixed4 alpha = tex2D(_MainTex, alpha_coord);

                half a = index == 0 ? alpha.r : (index == 1 ? alpha.g : alpha.b);

                //ETC1 Compression will bring some messy pixels in complex
                //images, this can solve that in many cases.
                //You may want to create a copy and tweak the parameter here to
                //get better result.
                if (a < 0.5) {
                    a = a * a;
                    if (a < 0.15) {
                        a = 0;
                    }
                } else {
                    a = 1 - a;
                    a = 1 - a * a;
                    if (a > 0.85) {
                        a = 1;
                    }
                }

                c.a = a;
                c.rgb *= c.a;

                c *= IN.color;
                return c;
            }
        ENDCG
        }
    }
}
