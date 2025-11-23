using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	[ExportCategory("Movement")]
	[Export] public float Speed = 5.0f;
	[Export] public float SprintSpeed = 9.0f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public float Gravity = 9.8f;
	[Export] public float MouseSensitivity = 0.003f;
	[Export] public float Acceleration = 10.0f;
	[Export] public float AirControl = 2.0f;

	[ExportCategory("References")]
	[Export] public Node3D Head;
	[Export] public Camera3D Camera;

	private Vector2 _mouseInput;

	public override void _Ready()
	{
		// Lock mouse to center of screen
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		// Handle Mouse Look
		if (@event is InputEventMouseMotion motion)
		{
			RotateY(-motion.Relative.X * MouseSensitivity);
			Head.RotateX(-motion.Relative.Y * MouseSensitivity);

			// Clamp looking up and down (90 degrees)
			Vector3 rot = Head.Rotation;
			rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
			Head.Rotation = rot;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add Gravity
		if (!IsOnFloor())
		{
			velocity.Y -= Gravity * (float)delta;
		}

		// Handle Jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get Input Direction
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		// Determine Speed (Sprint vs Walk)
		float currentSpeed = Input.IsActionPressed("sprint") ? SprintSpeed : Speed;

		// Apply Movement with Acceleration (Smoother feel)
		if (direction != Vector3.Zero)
		{
			float accel = IsOnFloor() ? Acceleration : AirControl; // Less control in air
			velocity.X = Mathf.Lerp(velocity.X, direction.X * currentSpeed, accel * (float)delta);
			velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * currentSpeed, accel * (float)delta);
		}
		else
		{
			// Friction / Stopping
			float friction = IsOnFloor() ? Acceleration : AirControl;
			velocity.X = Mathf.Lerp(velocity.X, 0, friction * (float)delta);
			velocity.Z = Mathf.Lerp(velocity.Z, 0, friction * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
