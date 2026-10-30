using Godot;
using System;

public partial class Player : Area3D
{
	[Export]
	public float Speed { get; set; } = 5.0f;

	[Export]
	public int Health { get; set; } = 100;

	[Export]
	public PackedScene BulletScene { get; set; }

	[Export]
	public Marker3D ShootingPoint { get; set; }

	// Предполагаем, что enum CameraMode определён в другом скрипте
	private CameraMode cameraMode = CameraMode.Top;

	[Signal]
	public delegate void TakeDamageEventHandler(int damage);

	public void ChangeCameraMode(CameraMode mode)
	{
		cameraMode = mode;
	}

	// Обработчик сигнала TakeDamage
	public void OnTakeDamage(int damage)
	{
		Health = Math.Max(Health - damage, 0);
		if (Health <= 0)
		{
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var input = Input.Singleton;
		// берем текущий глобальный transform
		var tf = GlobalTransform;

		// Движение по камере
		switch (cameraMode)
		{
			case CameraMode.Top:
				if (input.IsActionPressed("ui_up"))    tf.Origin += new Vector3(0, 0, -Speed * (float)delta);
				if (input.IsActionPressed("ui_down"))  tf.Origin += new Vector3(0, 0,  Speed * (float)delta);
				if (input.IsActionPressed("ui_left"))  tf.Origin += new Vector3(-Speed * (float)delta, 0, 0);
				if (input.IsActionPressed("ui_right")) tf.Origin += new Vector3( Speed * (float)delta, 0, 0);
				break;

			case CameraMode.Side:
				if (input.IsActionPressed("ui_up"))    tf.Origin += new Vector3(0,  Speed * (float)delta, 0);
				if (input.IsActionPressed("ui_down"))  tf.Origin += new Vector3(0, -Speed * (float)delta, 0);
				if (input.IsActionPressed("ui_left"))  tf.Origin += new Vector3(0, 0,  Speed * (float)delta);
				if (input.IsActionPressed("ui_right")) tf.Origin += new Vector3(0, 0, -Speed * (float)delta);
				break;
		}

		// Стрельба
		if (input.IsActionJustPressed("fire") && BulletScene != null && ShootingPoint != null)
		{
			var bullet = (Bullet)BulletScene.Instantiate();
			bullet.GlobalTransform = ShootingPoint.GlobalTransform;
			GetParent().AddChild(bullet);
		}

		GlobalTransform = tf;
	}
}
