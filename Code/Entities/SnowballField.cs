using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/SnowballField")]
    [Tracked]
    public class SnowballField : Solid
    {
		public float Flash;

		public float Solidify;

		public bool Flashing;

		private float solidifyDelay;

		public bool SpeedUp;

		public bool SlowDown;

		public bool DestroySnowball;

		public bool KillPlayer;

		public Color color;

		public float SpeedMultiplier;

		private List<Vector2> particles = new List<Vector2>();

		private List<SeekerBarrier> adjacent = new List<SeekerBarrier>();

		private float[] speeds = new float[3] { 12f, 20f, 40f };

		public SnowballField(Vector2 position, float width, float height, bool speedUp, bool slowDown, bool destroySnowball, bool killPlayer, float speedMultiplier)
            : base(position, width, height, safe: false)
        {

            Collidable = false;
            for (int i = 0; (float)i < base.Width * base.Height / 16f; i++)
            {
                particles.Add(new Vector2(Calc.Random.NextFloat(base.Width - 1f), Calc.Random.NextFloat(base.Height - 1f)));
            }
            SpeedUp = speedUp;
            SlowDown = slowDown;
            DestroySnowball = destroySnowball;
            KillPlayer = killPlayer;
			SpeedMultiplier = speedMultiplier;
        }

        public SnowballField(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Bool("speedup"), data.Bool("slowdown"), data.Bool("destroysnowball"), data.Bool("killplayer"), data.Float("speedmultiplier"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
		}

		public override void Update()
		{
			if (Flashing)
			{
				Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
				if (Flash <= 0f)
				{
					Flashing = false;
				}
			}
			else if (solidifyDelay > 0f)
			{
				solidifyDelay -= Engine.DeltaTime;
			}
			else if (Solidify > 0f)
			{
				Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
			}
			int num = speeds.Length;
			float height = base.Height;
			int i = 0;
			for (int count = particles.Count; i < count; i++)
			{
				Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
				value.Y %= height - 1f;
				particles[i] = value;
			}
			base.Update();
		}

		public override void Render()
		{
			if (SpeedUp)
            {
				color = Color.Green * 0.5f;
			}
			else if (SlowDown)
            {
				color = Color.Red * 0.5f;
            }
			else if (DestroySnowball)
            {
				color = Color.White * 0.5f;
            }
			else if (KillPlayer)
            {
				color = Color.Gold * 0.5f;
            }
			foreach (Vector2 particle in particles)
			{
				Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
			}
			if (Flashing)
			{
				Draw.Rect(base.Collider, Color.White * Flash * 0.5f);
			}
		}
		public void OnSnowball(PowerUpSnowball snowball)
        {
			if (SpeedUp)
            {
				snowball.SnowballSpeed = snowball.SnowballSpeed * SpeedMultiplier;
            }
			if (SlowDown)
            {
				snowball.SnowballSpeed = snowball.SnowballSpeed / SpeedMultiplier;
            }
			if (DestroySnowball)
            {
				snowball.Destroy();
            }
			if (KillPlayer)
            {
				Level level = snowball.Scene as Level;
				Player player = level.Tracker.GetEntity<Player>();
				player.Die(Vector2.Zero);
            }
        }
		public void OnSnowballTwo(PowerUpSnowballTwo snowball)
		{
			if (SpeedUp)
			{
				snowball.SnowballSpeed = snowball.SnowballSpeed * SpeedMultiplier;
			}
			if (SlowDown)
			{
				snowball.SnowballSpeed = snowball.SnowballSpeed / SpeedMultiplier;
			}
			if (DestroySnowball)
			{
				snowball.Destroy();
			}
			if (KillPlayer)
			{
				Level level = snowball.Scene as Level;
				Player player = level.Tracker.GetEntity<Player>();
				player.Die(Vector2.Zero);
			}
		}
	}
}
