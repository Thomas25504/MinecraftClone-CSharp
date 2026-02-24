using OpenTK.Mathematics;

namespace OpenTKTest;

public class Camera
{
    public Vector3 Position = new Vector3(0.0f, 0.0f, 3.0f);
    public float AspectRatio;

    public float _pitch = 0f;
    public float _yaw = -90f;
    private Vector3 _front = new Vector3(0f, 0f, 0f);
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;
    private float _fov = 75f;

    public Vector3 Front => _front;
    public Vector3 Up => _up;
    public Vector3 Right => _right;

    public Camera(Vector3 startPosition, float aspectRatio)
    {
        Position = startPosition;
        AspectRatio = aspectRatio;
        UpdateVectors();
    }

    public float Pitch
    {
        get => _pitch;
        set
        {
            _pitch = Math.Clamp(value, -89f, 89f);
            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => _yaw;
        set
        {
            _yaw = value;
            UpdateVectors();
        }
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + _front, _up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(_fov),
            AspectRatio,
            0.1f,
            1000f
        );
    }

    private void UpdateVectors()
    {
        float pitchRad = MathHelper.DegreesToRadians(_pitch);
        float yawRad = MathHelper.DegreesToRadians(_yaw);

        _front = Vector3.Normalize(new Vector3(
            MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            MathF.Sin(pitchRad),
            MathF.Cos(pitchRad) * MathF.Sin(yawRad)
        ));

        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}