#version 330 core
in vec2 TexCoord;
in float Brightness;

out vec4 FragColor;

uniform sampler2D atlas;

void main()
{
    vec4 texColor = texture(atlas, TexCoord);
    
    // Discard fully transparent pixels instead of blending
    if (texColor.a < 0.5)
        discard;
    
    FragColor = vec4(texColor.rgb * Brightness, texColor.a);
}