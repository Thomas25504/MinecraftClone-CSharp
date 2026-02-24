using OpenTK.Mathematics;

namespace OpenTKTest;

public class Camera
{
    public Vector3 Position = new Vector3(0.0f, 0.0f, 3.0f); // Default position
    public float AspectRatio; // Set this when initializing the camera

    public float _pitch = 0f; // Rotation around the X-axis
    public float _yaw = -90f; // Rotation around the Y-axis (initialized to -90 to look towards the negative Z-axis)
    private Vector3 _front = new Vector3(0f, 0f, 0f); // This will be calculated based on pitch and yaw
    private Vector3 _up = Vector3.UnitY; // World up vector (Y-axis)
    private Vector3 _right = Vector3.UnitX; // This will be calculated as the cross product of front and up
    private float _fov = 75f; // Field of view in degrees

    public Vector3 Front => _front; // Direction the camera is looking at
    public Vector3 Up => _up; // Up direction of the camera
    public Vector3 Right => _right; // Right direction of the camera

    public Camera(Vector3 startPosition, float aspectRatio)
    {
        Position = startPosition;
        AspectRatio = aspectRatio;
        UpdateVectors();
    }

    // Properties to get and set pitch and yaw with clamping for pitch
    public float Pitch
    {
        get => _pitch;
        set
        {
            _pitch = Math.Clamp(value, -89f, 89f);
            UpdateVectors();
        }
    }

    // Yaw can wrap around
    public float Yaw
    {
        get => _yaw;
        set
        {
            _yaw = value;
            UpdateVectors();
        }
    }

    // Method to get the view matrix based on the current position and orientation of the camera
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + _front, _up);
    }

    // Method to get the projection matrix based on the current field of view and aspect ratio
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(_fov),
            AspectRatio,
            0.1f,
            1000f
        );
    }

    // Method to update the front, right, and up vectors based on the current pitch and yaw
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