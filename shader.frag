#version 330 core

in vec2 vUV;
out vec4 FragColor;

uniform sampler2D atlas;

void main()
{
    FragColor = texture(atlas, vUV);
}