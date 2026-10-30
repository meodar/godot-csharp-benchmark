using Godot;
using System;

public partial class Land : Node
{
	[Export] public Node3D LandTerrain { get; set; }
	[Export] public Node3D SecondLandTerrain { get; set; }
	[Export] public Camera3D Camera { get; set; }
	[Export] public Player Player { get; set; }

	private CameraMode cameraMode = CameraMode.Top;
	private float lastSidePos = 0f;
	private float lastTopPos = 0f;

	// Pure C# event for camera view changes
	public event Action<CameraMode> CameraViewChanged;

	public override void _Ready()
	{
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "warm_up");

		// Subscribe Player.ChangeCameraMode to the event
		if (Player != null)
			CameraViewChanged += Player.ChangeCameraMode;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Update terrains
		UpdateTerrain(LandTerrain, delta);
		UpdateTerrain(SecondLandTerrain, delta);

		// Toggle camera view
		if (Input.Singleton.IsActionJustPressed("camera_change") && Camera != null)
		{
			if (cameraMode == CameraMode.Top)
			{
				// Top -> Side
				var tf = Camera.GlobalTransform;
				tf.Basis = tf.Basis.Rotated(Vector3.Forward, Mathf.Pi / 2);
				tf.Basis = tf.Basis.Rotated(Vector3.Right, Mathf.Pi / 2);
				tf.Origin += new Vector3(1000f, -1000f, 0f);
				Camera.GlobalTransform = tf;

				// Move player to last saved position
				var ptf = Player.GlobalTransform;
				float savedTopX = ptf.Origin.X;
				ptf.Origin = new Vector3(0f, lastSidePos, ptf.Origin.Z);
				Player.GlobalTransform = ptf;

				cameraMode = CameraMode.Side;
				lastTopPos = savedTopX;
			}
			else
			{
				// Side -> Top
				var tf = Camera.GlobalTransform;
				tf.Basis = tf.Basis.Rotated(Vector3.Right, -Mathf.Pi / 2);
				tf.Basis = tf.Basis.Rotated(Vector3.Forward, -Mathf.Pi / 2);
				tf.Origin += new Vector3(-1000f, 1000f, 0f);
				Camera.GlobalTransform = tf;

				var ptf = Player.GlobalTransform;
				float savedSideY = ptf.Origin.Y;
				ptf.Origin = new Vector3(lastTopPos, 0f, ptf.Origin.Z);
				Player.GlobalTransform = ptf;

				cameraMode = CameraMode.Top;
				lastSidePos = savedSideY;
			}

			// Notify subscribers
			CameraViewChanged?.Invoke(cameraMode);
		}
	}

	private void UpdateTerrain(Node3D terrain, double delta)
	{
		if (terrain == null)
			return;

		var tf = terrain.GlobalTransform;
		tf.Origin = new Vector3(tf.Origin.X, tf.Origin.Y, tf.Origin.Z + 80f * (float)delta);
		if (tf.Origin.Z >= 32000f)
			tf.Origin = new Vector3(tf.Origin.X, tf.Origin.Y, tf.Origin.Z - 64000f);
		terrain.GlobalTransform = tf;
	}
}
