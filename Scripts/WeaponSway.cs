using Godot;
using System;

public partial class WeaponSway : Node3D
{
	[ExportGroup("Sway Settings")]
	[Export] public float SwayAmount = 0.02f;
	[Export] public float MaxSway = 0.06f;
	[Export] public float SwaySmoothness = 10.0f;

	[ExportGroup("Bob Settings")]
	[Export] public float BobFrequency = 2.0f;
	[Export] public float BobAmplitude = 0.01f;
	[Export] public CharacterBody3D Player;

	private Vector3 _initialPosition;
	private float _bobTimer = 0.0f;

	public override void _Ready()
	{
		_initialPosition = Position;
	}

	public override void _Input(InputEvent @event)
	{
		// Handle Mouse Sway (Lag)
		if (@event is InputEventMouseMotion motion)
		{
			float moveX = -motion.Relative.X * SwayAmount;
			float moveY = motion.Relative.Y * SwayAmount;

			moveX = Mathf.Clamp(moveX, -MaxSway, MaxSway);
			moveY = Mathf.Clamp(moveY, -MaxSway, MaxSway);

			Vector3 targetPos = new Vector3(
				_initialPosition.X + moveX,
				_initialPosition.Y + moveY,
				_initialPosition.Z
			);

			// Apply immediate sway for responsiveness
			Position = Position.Lerp(targetPos, 0.1f);
		}
	}

	public override void _Process(double delta)
	{
		// Handle Weapon Bob (Walking movement)
		Vector3 targetBob = Vector3.Zero;

		if (Player.Velocity.Length() > 0.1f && Player.IsOnFloor())
		{
			_bobTimer += (float)delta * Player.Velocity.Length() * BobFrequency;
			targetBob.Y = Mathf.Sin(_bobTimer) * BobAmplitude;
			targetBob.X = Mathf.Cos(_bobTimer * 0.5f) * BobAmplitude; // Figure-8 pattern
		}
		else
		{
			_bobTimer = 0; // Reset bob when stopped
		}

		// Combine Sway Return + Bobbing
		Vector3 finalTarget = _initialPosition + targetBob;
		Position = Position.Lerp(finalTarget, (float)delta * SwaySmoothness);
	}
}
