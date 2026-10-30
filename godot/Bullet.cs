using Godot;
using System;

public partial class Bullet : Area3D
{
	[Export]
	public float Speed { get; set; } = 1000f;

	[Export]
	public int Damage { get; set; } = 300;

	public override void _Ready()
	{
		// Подписываемся на сигнал area_entered
		Connect("area_entered", new Callable(this, nameof(OnBodyEntered)));
	}

	private void OnBodyEntered(Area3D area)
	{
		// Если попали в игрока — нанести урон и удалить пулю
		if (area is Player player)
		{
			player.EmitSignal("TakeDamage", Damage);
			QueueFree();
		}
		// Если попали во врага — уведомить врага и удалить пулю
		else if (area is Enemy enemy)
		{
			enemy.EmitSignal("BulletHit");
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Двигаемся вперёд по локальной оси Z
		var tf = GlobalTransform;
		var forward = tf.Basis.Z.Normalized();

		tf.Origin += forward * Speed * (float)delta;
		GlobalTransform = tf;

		// Удаляем пулю, если ушла слишком далеко по Z
		if (Math.Abs(tf.Origin.Z) > 1000f)
			QueueFree();
	}
}
