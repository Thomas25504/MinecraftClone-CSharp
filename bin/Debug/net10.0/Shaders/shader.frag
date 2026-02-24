#version 330 core
in vec2 TexCoord;
in float Brightness;

out vec4 FragColor;

uniform sampler2D atlas;

void main()
{
    vec4 texColor = texture(atlas, TexCoord);
    FragColor = vec4(texColor.rgb * Brightness, texColor.a);
}