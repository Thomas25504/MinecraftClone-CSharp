using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Shader : IDisposable
{
    public readonly int Handle; // OpenGL handle for the shader program
    private bool _disposed = false; // to track whether Dispose has been called

    public Shader(string vertexPath, string fragmentPath)
    {
        // load source code from files
        string vertexSource = File.ReadAllText(vertexPath);
        string fragmentSource = File.ReadAllText(fragmentPath);

        // compile vertex shader
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);
        GL.CompileShader(vertexShader);
        CheckCompileErrors(vertexShader, "VERTEX");

        // compile fragment shader
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSource);
        GL.CompileShader(fragmentShader);
        CheckCompileErrors(fragmentShader, "FRAGMENT");

        // link both shaders into a program
        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        CheckCompileErrors(Handle, "PROGRAM");

        // clean up individual shaders, they're linked now so we don't need them
        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use() // activate the shader program
    {
        GL.UseProgram(Handle);
    }

    // utility methods for setting uniforms
    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, value);
    }

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location, transpose: false, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(location, vector);
    }

    // checks for shader compilation/linking errors and throws exceptions with the error log
    private void CheckCompileErrors(int shader, string type)
    {
        if (type == "PROGRAM")
        {
            GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(shader);
                throw new Exception($"Shader program linking error:\n{infoLog}");
            }
        }
        else
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"{type} shader compile error:\n{infoLog}");
            }
        }
    }

    // implements IDisposable to allow for proper cleanup of OpenGL resources
    public void Dispose()
    {
        if (!_disposed)
        {
            GL.DeleteProgram(Handle);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    // finalizer to ensure resources are freed if Dispose is not called
    ~Shader()
    {
        if (!_disposed)
            GL.DeleteProgram(Handle);
    }
}