using Godot;
using System;

public partial class Enemy : Area3D
{
	[Export]
	public float Speed { get; set; } = 200f;

	[Export]
	public EnemyType EnemyType { get; set; } = EnemyType.Top;

	[Export]
	public EnemyMovement EnemyMovement { get; set; } = EnemyMovement.Lin;

	[Export]
	public Vector3 MovementMod { get; set; } = new Vector3(1f, 1f, 1f);

	[Export]
	public Timer ShootingTimer { get; set; }

	[Export]
	public PackedScene BulletScene { get; set; }

	[Export]
	public Marker3D ShootingPoint { get; set; }

	private double timeElapsed = 0.0;

	private static readonly int[] WorkloadTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "workload_10k").AsInt32Array();
	private static readonly float[] MovementModTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "movement_mod").AsFloat32Array();
	private static readonly byte[] MovementKindTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "movement_kind").AsByteArray();

	private int workloadCursor = 0;

	// GD0201: имя делегата должно заканчиваться на 'EventHandler'
	[Signal]
	public delegate void BulletHitEventHandler();

	public override void _Ready()
	{
		// Параметры движения берутся из предгенерированной таблицы
		int spawnIndex = ClassDB.ClassCallStatic("BenchmarkRandomTable", "next_spawn_index").AsInt32();
		int modMask = MovementModTable.Length - 1;
		MovementMod = new Vector3(
			MovementModTable[(spawnIndex * 3) & modMask],
			MovementModTable[(spawnIndex * 3 + 1) & modMask],
			MovementModTable[(spawnIndex * 3 + 2) & modMask]
		);
		EnemyMovement = (EnemyMovement)MovementKindTable[spawnIndex & (MovementKindTable.Length - 1)];

		workloadCursor = spawnIndex;

		// Подписываемся на таймер стрельбы
		if (ShootingTimer != null)
			ShootingTimer.Timeout += OnShootTimeout;

		// Подписываемся на собственный сигнал попадания
		BulletHit += OnBulletHit;
	}

	private void OnShootTimeout()
	{
		if (ShootingPoint != null && BulletScene != null)
		{
			// Инстанцируем пулю и добавляем её в ту же ноду-родителя
			var bullet = (Area3D)BulletScene.Instantiate();
			bullet.GlobalTransform = ShootingPoint.GlobalTransform;
			GetParent().AddChild(bullet);
		}
	}

	private void OnBulletHit()
	{
		// При попадании — удаляем врага
		QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		timeElapsed += delta;
		float t = (float)timeElapsed;
		float dz = Speed * (float)delta;
		
		double sum = 0.0;
		int[] table = WorkloadTable;
		int cursor = workloadCursor;
		for (int n = 0; n < 7_000; n++)
		{
			int i = table[cursor++ & (table.Length - 1)];
			sum += Math.Sqrt(i * Math.PI) * Math.Cos(i * 0.01);
		}
		workloadCursor = cursor;

		if (sum < 0.0)
		{
			GD.Print("weird");
		}

		var tf = GlobalTransform;
		// берём модификаторы движения из Vector3
		float a = MovementMod.X;
		float b = MovementMod.Y;
		float c = MovementMod.Z;

		switch (EnemyMovement)
		{
			case EnemyMovement.Lin:
				tf.Origin += new Vector3(0, 0, dz);
				break;

			case EnemyMovement.Cos:
				{
					float phase = a * (float)Math.Cos(b * t + c) * 500f;
					tf.Origin = new Vector3(
						EnemyType == EnemyType.Top  ? phase : tf.Origin.X,
						EnemyType == EnemyType.Side ? phase : tf.Origin.Y,
						tf.Origin.Z + dz
					);
				}
				break;

			case EnemyMovement.Sin:
				{
					float phase = a * (float)Math.Sin(b * t + c) * 500f;
					tf.Origin = new Vector3(
						EnemyType == EnemyType.Top  ? phase : tf.Origin.X,
						EnemyType == EnemyType.Side ? phase : tf.Origin.Y,
						tf.Origin.Z + dz
					);
				}
				break;

			case EnemyMovement.Log:
				{
					float newZ = tf.Origin.Z + dz;
					float v = a * (float)Math.Log(Math.Max(Math.Abs(newZ), 1f) + c) * 200f;
					tf.Origin = new Vector3(
						EnemyType == EnemyType.Top  ? v : tf.Origin.X,
						EnemyType == EnemyType.Side ? v : tf.Origin.Y,
						newZ
					);
				}
				break;

			case EnemyMovement.Sqrt:
				{
					float newZ = tf.Origin.Z + dz;
					float v = a * ((float)Math.Sqrt(Math.Abs(newZ)) + c) * 100f;
					tf.Origin = new Vector3(
						EnemyType == EnemyType.Top  ? v : tf.Origin.X,
						EnemyType == EnemyType.Side ? v : tf.Origin.Y,
						newZ
					);
				}
				break;
		}

		GlobalTransform = tf;

		if (tf.Origin.Z >= 800f)
			QueueFree();

	}
}
