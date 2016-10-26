# A1316_FUNCTIONS() #
```
fixed4 a1316_get_color(sampler2D tex, float2 uv)
{
    float2 color_coord = uv * 0.8125;

    fixed4 c = tex2D(tex, color_coord);

    float texX = uv.x;

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

    fixed4 alpha = tex2D(tex, alpha_coord);

    c.a = index == 0 ? alpha.r : (index == 1 ? alpha.g : alpha.b);
    return c;
}

fixed4 a1316_get_color_with_fix(sampler2D tex, float2 uv)
    fixed4 c = a1316_get_color(tex, uv);
    half a = c.a;

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
    return c;
}
```
