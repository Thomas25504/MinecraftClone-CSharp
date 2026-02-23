#version 330 core
layout (location = 0) in vec3 aPosition;

uniform mat4 projection;

void main()
{
    gl_Position = projection * vec4(aPosition, 1.0);
}