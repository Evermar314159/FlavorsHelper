using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using LightOcclude = Celeste.LightOcclude;
using Player = Celeste.Player;
using Dust = Celeste.Dust;
using TheoCrystal = Celeste.TheoCrystal;
using Level = Celeste.Level;
using Input = Celeste.Input;
using Strawberry = Celeste.Strawberry;
using DisplacementRenderer = Celeste.DisplacementRenderer;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using On.Celeste;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/UnDreamBlock")]
    [Tracked]
    public class UnDreamBlock : Celeste.DreamBlock
    {
		private struct DreamParticle
		{
			public Vector2 Position;

			public int Layer;

			public Color Color;

			public float TimeOffset;
		}

		private static readonly Color activeBackColor = Color.Black;

		private static readonly Color disabledBackColor = Calc.HexToColor("1f2e2d");

		private static readonly Color activeLineColor = Color.White;

		private static readonly Color disabledLineColor = Calc.HexToColor("6a8480");

		private bool playerHasDreamDash;
        
		private Vector2? node;

		public Vector2 DashDir;

		private Celeste.LightOcclude occlude;

		private MTexture[] particleTextures;

		private DreamParticle[] particles;

		private float whiteFill;

		private float whiteHeight = 1f;

		private Vector2 shake;

		private float animTimer;

		private Shaker shaker;

		private bool fastMoving;

		private bool oneUse;

		private float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

		private float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

		private float wobbleEase;
        
		public bool ConserveSpeed;
		
		public float DashSpeed;

        public UnDreamBlock(Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse, bool below)
		: base(position, width, height, node, fastMoving, oneUse, below)
		{
			base.Depth = -11000;
			this.node = node;
			this.fastMoving = fastMoving;
			this.oneUse = oneUse;
			if (below)
			{
				base.Depth = 5000;
			}
			SurfaceSoundIndex = 11;
			particleTextures = new MTexture[4]
			{
            Celeste.GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            Celeste.GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            Celeste.GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            Celeste.GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
			};
		}

		


		public UnDreamBlock(Celeste.EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), data.Bool("fastMoving"), data.Bool("oneUse"), data.Bool("below"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			playerHasDreamDash = !SceneAs<Celeste.Level>().Session.Inventory.DreamDash;
			if (playerHasDreamDash && node.HasValue)
			{
				Vector2 start = Position;
				Vector2 end = node.Value;
				float num = Vector2.Distance(start, end) / 12f;
				if (fastMoving)
				{
					num /= 3f;
				}
				Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);
				tween.OnUpdate = delegate (Tween t)
				{
					if (Collidable)
					{
						MoveTo(Vector2.Lerp(start, end, t.Eased));
					}
					else
					{
						MoveToNaive(Vector2.Lerp(start, end, t.Eased));
					}
				};
				Add(tween);
			}
			if (!playerHasDreamDash)
			{
				Add(occlude = new LightOcclude());
			}
			Setup();
		}

		public new void Setup()
		{
			particles = new DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
				particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
				particles[i].TimeOffset = Calc.Random.NextFloat();
				particles[i].Color = Color.LightGray * (0.5f + (float)particles[i].Layer / 2f * 0.5f);
				if (playerHasDreamDash)
				{
					switch (particles[i].Layer)
					{
						case 0:
							particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
							break;
						case 1:
							particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
							break;
						case 2:
							particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
							break;
					}
				}
			}
		}

		public new void OnPlayerExit(Player player)
		{
			Dust.Burst(player.Position, player.Speed.Angle(), 16, null);
			Vector2 vector = Vector2.Zero;
			if (CollideCheck(player, Position + Vector2.UnitX * 4f))
			{
				vector = Vector2.UnitX;
			}
			else if (CollideCheck(player, Position - Vector2.UnitX * 4f))
			{
				vector = -Vector2.UnitX;
			}
			else if (CollideCheck(player, Position + Vector2.UnitY * 4f))
			{
				vector = Vector2.UnitY;
			}
			else if (CollideCheck(player, Position - Vector2.UnitY * 4f))
			{
				vector = -Vector2.UnitY;
			}
			_ = vector != Vector2.Zero;
			if (oneUse)
			{
				OneUseDestroy();
			}
		}

		private void OneUseDestroy()
		{
			Collidable = (Visible = false);
			DisableStaticMovers();
			RemoveSelf();
		}

		public override void Update()
		{
			base.Update();
			if (playerHasDreamDash)
			{
				animTimer += 6f * Engine.DeltaTime;
				wobbleEase += Engine.DeltaTime * 2f;
				if (wobbleEase > 1f)
				{
					wobbleEase = 0f;
					wobbleFrom = wobbleTo;
					wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
				}
				SurfaceSoundIndex = 12;
			}
		}

		public new bool BlockedCheck()
		{
			TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
			if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
			{
				return true;
			}
			Player player = CollideFirst<Player>();
			if (player != null && !TryActorWiggleUp(player))
			{
				return true;
			}
			return false;
		}

		private bool TryActorWiggleUp(Entity actor)
		{
			bool collidable = Collidable;
			Collidable = true;
			for (int i = 1; i <= 4; i++)
			{
				if (!actor.CollideCheck<Celeste.Solid>(actor.Position - Vector2.UnitY * i))
				{
					actor.Position -= Vector2.UnitY * i;
					Collidable = collidable;
					return true;
				}
			}
			Collidable = collidable;
			return false;
		}

		public override void Render()
		{
			Camera camera = SceneAs<Celeste.Level>().Camera;
			if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom)
			{
				return;
			}
			Draw.Rect(shake.X + base.X, shake.Y + base.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
			Vector2 position = SceneAs<Level>().Camera.Position;
			for (int i = 0; i < particles.Length; i++)
			{
				int layer = particles[i].Layer;
				Vector2 position2 = particles[i].Position;
				position2 += position * (0.3f + 0.25f * (float)layer);
				position2 = PutInside(position2);
				Color color = particles[i].Color;
				MTexture mTexture;
				switch (layer)
				{
					case 0:
						{
							int num2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
							mTexture = particleTextures[3 - num2];
							break;
						}
					case 1:
						{
							int num = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
							mTexture = particleTextures[1 + num];
							break;
						}
					default:
						mTexture = particleTextures[2];
						break;
				}
				if (position2.X >= base.X + 2f && position2.Y >= base.Y + 2f && position2.X < base.Right - 2f && position2.Y < base.Bottom - 2f)
				{
					mTexture.DrawCentered(position2 + shake, color);
				}
			}
			if (whiteFill > 0f)
			{
				Draw.Rect(base.X + shake.X, base.Y + shake.Y, base.Width, base.Height * whiteHeight, Color.White * whiteFill);
			}
			WobbleLine(shake + new Vector2(base.X, base.Y), shake + new Vector2(base.X + base.Width, base.Y), 0f);
			WobbleLine(shake + new Vector2(base.X + base.Width, base.Y), shake + new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f);
			WobbleLine(shake + new Vector2(base.X + base.Width, base.Y + base.Height), shake + new Vector2(base.X, base.Y + base.Height), 1.5f);
			WobbleLine(shake + new Vector2(base.X, base.Y + base.Height), shake + new Vector2(base.X, base.Y), 2.5f);
			Draw.Rect(shake + new Vector2(base.X, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
		}

		private Vector2 PutInside(Vector2 pos)
		{
			if (pos.X > base.Right)
			{
				pos.X -= (float)Math.Ceiling((pos.X - base.Right) / base.Width) * base.Width;
			}
			else if (pos.X < base.Left)
			{
				pos.X += (float)Math.Ceiling((base.Left - pos.X) / base.Width) * base.Width;
			}
			if (pos.Y > base.Bottom)
			{
				pos.Y -= (float)Math.Ceiling((pos.Y - base.Bottom) / base.Height) * base.Height;
			}
			else if (pos.Y < base.Top)
			{
				pos.Y += (float)Math.Ceiling((base.Top - pos.Y) / base.Height) * base.Height;
			}
			return pos;
		}

		private void WobbleLine(Vector2 from, Vector2 to, float offset)
		{
			float num = (to - from).Length();
			Vector2 vector = Vector2.Normalize(to - from);
			Vector2 vector2 = new Vector2(vector.Y, 0f - vector.X);
			Color color = (playerHasDreamDash ? activeLineColor : disabledLineColor);
			Color color2 = (playerHasDreamDash ? activeBackColor : disabledBackColor);
			if (whiteFill > 0f)
			{
				color = Color.Lerp(color, Color.White, whiteFill);
				color2 = Color.Lerp(color2, Color.White, whiteFill);
			}
			float num2 = 0f;
			int num3 = 16;
			for (int i = 2; (float)i < num - 2f; i += num3)
			{
				float num4 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
				if ((float)(i + num3) >= num)
				{
					num4 = 0f;
				}
				float num5 = Math.Min(num3, num - 2f - (float)i);
				Vector2 vector3 = from + vector * i + vector2 * num2;
				Vector2 vector4 = from + vector * ((float)i + num5) + vector2 * num4;
				Draw.Line(vector3 - vector2, vector4 - vector2, color2);
				Draw.Line(vector3 - vector2 * 2f, vector4 - vector2 * 2f, color2);
				Draw.Line(vector3, vector4, color);
				num2 = num4;
			}
		}

		private float LineAmplitude(float seed, float index)
		{
			return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
		}

		private float Lerp(float a, float b, float percent)
		{
			return a + (b - a) * percent;
		}

		public new IEnumerator Activate()
		{
			Level level = SceneAs<Level>();
			yield return 1f;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			Add(shaker = new Shaker(on: true, delegate (Vector2 t)
			{
				shake = t;
			}));
			shaker.Interval = 0.02f;
			shaker.On = true;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
			{
				whiteFill = Ease.CubeIn(p2);
				yield return null;
			}
			shaker.On = false;
			yield return 0.5f;
			ActivateNoRoutine();
			whiteHeight = 1f;
			whiteFill = 1f;
			for (float p2 = 1f; p2 > 0f; p2 -= Engine.DeltaTime * 0.5f)
			{
				whiteHeight = p2;
				if (level.OnInterval(0.1f))
				{
					for (int i = 0; (float)i < Width; i += 4)
					{
						level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(X + (float)i, Y + Height * whiteHeight + 1f));
					}
				}
				if (level.OnInterval(0.1f))
				{
					level.Shake();
				}
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
				yield return null;
			}
			while (whiteFill > 0f)
			{
				whiteFill -= Engine.DeltaTime * 3f;
				yield return null;
			}
		}

		public new void ActivateNoRoutine()
		{
			if (!playerHasDreamDash)
			{
				playerHasDreamDash = true;
				Setup();
				Remove(occlude);
			}
			whiteHeight = 0f;
			whiteFill = 0f;
			if (shaker != null)
			{
				shaker.On = false;
			}
		}

		public new void FootstepRipple(Vector2 position)
		{
			if (playerHasDreamDash)
			{
				DisplacementRenderer.Burst burst = (base.Scene as Level).Displacement.AddBurst(position, 0.5f, 0f, 40f);
				burst.WorldClipCollider = base.Collider;
				burst.WorldClipPadding = 1;
			}
		}

		public UnDreamBlock(Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse)
			: this(position, width, height, node, fastMoving, oneUse, below: false)
		{
		}

		public new void DeactivateNoRoutine()
		{
			if (playerHasDreamDash)
			{
				playerHasDreamDash = false;
				Setup();
				if (occlude == null)
				{
					occlude = new LightOcclude();
				}
				Add(occlude);
				whiteHeight = 1f;
				whiteFill = 0f;
				if (shaker != null)
				{
					shaker.On = false;
				}
				SurfaceSoundIndex = 11;
			}
		}

		public new IEnumerator Deactivate()
		{
			Level level = SceneAs<Level>();
			yield return 1f;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			if (shaker == null)
			{
				shaker = new Shaker(on: true, delegate (Vector2 t)
				{
					shake = t;
				});
			}
			Add(shaker);
			shaker.Interval = 0.02f;
			shaker.On = true;
			for (float alpha2 = 0f; alpha2 < 1f; alpha2 += Engine.DeltaTime)
			{
				whiteFill = Ease.CubeIn(alpha2);
				yield return null;
			}
			shaker.On = false;
			yield return 0.5f;
			DeactivateNoRoutine();
			whiteHeight = 1f;
			whiteFill = 1f;
			for (float alpha2 = 1f; alpha2 > 0f; alpha2 -= Engine.DeltaTime * 0.5f)
			{
				whiteHeight = alpha2;
				if (level.OnInterval(0.1f))
				{
					for (int i = 0; (float)i < Width; i += 4)
					{
						level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(X + (float)i, Y + Height * whiteHeight + 1f));
					}
				}
				if (level.OnInterval(0.1f))
				{
					level.Shake();
				}
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
				yield return null;
			}
			while (whiteFill > 0f)
			{
				whiteFill -= Engine.DeltaTime * 3f;
				yield return null;
			}
		}

		public new IEnumerator FastDeactivate()
		{
			Level level = SceneAs<Level>();
			yield return null;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			if (shaker == null)
			{
				shaker = new Shaker(on: true, delegate (Vector2 t)
				{
					shake = t;
				});
			}
			Add(shaker);
			shaker.Interval = 0.02f;
			shaker.On = true;
			for (float alpha = 0f; alpha < 1f; alpha += Engine.DeltaTime * 3f)
			{
				whiteFill = Ease.CubeIn(alpha);
				yield return null;
			}
			shaker.On = false;
			yield return 0.1f;
			DeactivateNoRoutine();
			whiteHeight = 1f;
			whiteFill = 1f;
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, TopCenter, Vector2.UnitX * Width / 2f, Color.White, (float)Math.PI);
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, BottomCenter, Vector2.UnitX * Width / 2f, Color.White, 0f);
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterLeft, Vector2.UnitY * Height / 2f, Color.White, 4.712389f);
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterRight, Vector2.UnitY * Height / 2f, Color.White, (float)Math.PI / 2f);
			level.Shake();
			yield return 0.1f;
			while (whiteFill > 0f)
			{
				whiteFill -= Engine.DeltaTime * 3f;
				yield return null;
			}
		}

		public new IEnumerator FastActivate()
		{
			Level level = SceneAs<Level>();
			yield return null;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			if (shaker == null)
			{
				shaker = new Shaker(on: true, delegate (Vector2 t)
				{
					shake = t;
				});
			}
			Add(shaker);
			shaker.Interval = 0.02f;
			shaker.On = true;
			for (float alpha = 0f; alpha < 1f; alpha += Engine.DeltaTime * 3f)
			{
				whiteFill = Ease.CubeIn(alpha);
				yield return null;
			}
			shaker.On = false;
			yield return 0.1f;
			ActivateNoRoutine();
			whiteHeight = 1f;
			whiteFill = 1f;
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, TopCenter, Vector2.UnitX * Width / 2f, Color.White, (float)Math.PI);
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Width, BottomCenter, Vector2.UnitX * Width / 2f, Color.White, 0f);
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterLeft, Vector2.UnitY * Height / 2f, Color.White, 4.712389f);
			level.ParticlesFG.Emit(Strawberry.P_WingsBurst, (int)Height, CenterRight, Vector2.UnitY * Height / 2f, Color.White, (float)Math.PI / 2f);
			level.Shake();
			yield return 0.1f;
			while (whiteFill > 0f)
			{
				whiteFill -= Engine.DeltaTime * 3f;
				yield return null;
			}
		}
		public static void Load()
		{
			On.Celeste.Player.DreamDashUpdate += Player_UnDreamDashUpdate;
			On.Celeste.Player.DreamDashEnd += Player_UnDreamDashEnd;
			IL.Celeste.Player.DreamDashBegin += Player_UnDreamDashBegin;
		}
		public static void Unload()
		{
			On.Celeste.Player.DreamDashUpdate -= Player_UnDreamDashUpdate;
			On.Celeste.Player.DreamDashEnd -= Player_UnDreamDashEnd;
			IL.Celeste.Player.DreamDashBegin -= Player_UnDreamDashBegin;
		}
		private static void Player_UnDreamDashBegin(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			if (cursor.TryGotoNext(MoveType.After, (Instruction instr) => instr.MatchStfld<Player>("Speed")))
			{
				cursor.Index--;
				cursor.Emit(OpCodes.Ldarg_0);
				cursor.EmitDelegate<Func<Vector2, Player, Vector2>>(delegate (Vector2 origSpeed, Player player)
				{
					UnDreamBlock unDreamBlockV = player.CollideFirst<UnDreamBlock>(player.Position + Vector2.UnitX * Math.Sign(player.Speed.X)) ?? player.CollideFirst<UnDreamBlock>(player.Position + Vector2.UnitY * Math.Sign(player.Speed.Y));
					return (unDreamBlockV != null && unDreamBlockV.ConserveSpeed) ? (player.Speed * Math.Sign(unDreamBlockV.DashSpeed)) : origSpeed;
				});
			}
		}

		private static void Player_UnDreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player self)
		{
			orig.Invoke(self);
			DynamicData.For(self).Set("lastDreamSpeed", 0f);
		}

		private static int Player_UnDreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
		{
			UnDreamBlock currentUnDreamBlock = self.CollideFirst<UnDreamBlock>();
			if (currentUnDreamBlock != null)
			{
				DynamicData dyn = DynamicData.For(self);
				if (!currentUnDreamBlock.ConserveSpeed)
				{
					float lastDreamSpeed = dyn.Get<float>("lastDreamSpeed");
					if (lastDreamSpeed != currentUnDreamBlock.DashSpeed)
					{
						self.Speed = self.DashDir * currentUnDreamBlock.DashSpeed;
						dyn.Set("lastDreamSpeed", currentUnDreamBlock.DashSpeed * 1f);
					}
				}
				
			}
			return orig.Invoke(self);
		}
		private bool DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self)
		{
			return true;
		}
	}
}
