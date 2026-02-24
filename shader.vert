#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aUV;

out vec2 vUV;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vUV = aUV;
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}