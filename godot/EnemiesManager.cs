using Godot;
using System;
using System.Collections.Generic;

public partial class EnemiesManager : Node
{
	[Export]
	public PackedScene TopEnemyScene { get; set; }

	[Export]
	public PackedScene SideEnemyScene { get; set; }

	[Export]
	public Timer SpawnTimer { get; set; }
	
	[Export]
	public Timer BenchmarkTimer { get; set; }
	
	private int benchmarkValue = 10;
	
	private float benchmark_sum = 0;

	private static readonly int[] WorkloadTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "workload_500").AsInt32Array();
	private static readonly float[] SpawnXTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "spawn_x").AsFloat32Array();
	private static readonly float[] SpawnYTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "spawn_y").AsFloat32Array();
	private static readonly byte[] SpawnTopTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "spawn_top").AsByteArray();
	private static readonly byte[] HeavyTickTable =
		ClassDB.ClassCallStatic("BenchmarkRandomTable", "heavy_tick").AsByteArray();

	private int spawnCursor = 0;
	private int sideCursor = 0;
	private int tickCursor = 0;
	private int workloadCursor = 0;

	public override void _Ready()
	{
		if (SpawnTimer != null)
			SpawnTimer.Timeout += OnTimeout;
			
		if (BenchmarkTimer != null)
			BenchmarkTimer.Timeout += OnBenchmarkTimeout;
	}

	private void OnTimeout()
	{
		bool isTop = SpawnTopTable[sideCursor++ & (SpawnTopTable.Length - 1)] != 0;
		SpawnEnemy(isTop);
	}
	
	private void OnBenchmarkTimeout()
	{
		for (int i = 0; i < benchmarkValue; i++)
			SpawnEnemy(true);
		for (int i = 0; i < benchmarkValue; i++)
			SpawnEnemy(false);
		benchmarkValue += 10;
	}

	private void SpawnEnemy(bool isTop)
	{
		var parent = GetParent();
		if (parent == null) return;

		var enemy = (Enemy)(isTop ? TopEnemyScene : SideEnemyScene).Instantiate();
		parent.AddChild(enemy);

		// 2) Ставим позицию, НЕ трогая Basis/Transform
		Vector3 worldPos = isTop
			? new Vector3(SpawnXTable[spawnCursor & (SpawnXTable.Length - 1)], 0f, -800f)
			: new Vector3(0f, SpawnYTable[spawnCursor & (SpawnYTable.Length - 1)], -800f);
		spawnCursor++;

		enemy.GlobalPosition = worldPos;
	}

	public override void _PhysicsProcess(double delta)
	{
		var input = Input.Singleton;
		if (input.IsActionJustPressed("benchmark_spawn"))
		{
			for (int i = 0; i < 50; i++)
				SpawnEnemy(true);
			for (int i = 0; i < 50; i++)
				SpawnEnemy(false);
		}
		
		if (HeavyTickTable[tickCursor++ & (HeavyTickTable.Length - 1)] != 0)
		{
			float acc = 0f;
			var localData = new List<float>();
			int[] table = WorkloadTable;
			int cursor = workloadCursor;

			for (int i = 0; i < 500; i++)
			{
				int l = table[cursor++ & (table.Length - 1)];
				float rad = l * (Mathf.Pi / 180f); // 0.0174533f
				float x = MathF.Sin(rad) * MathF.Sqrt(l + 1f);
				acc += MathF.Pow(x, 1.5f);
				localData.Add(acc);
			}

			workloadCursor = cursor;
			benchmark_sum = acc;
		}
	}
}
