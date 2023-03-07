using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/SnowballRefill")]
    [Tracked]
    public class SnowballRefill : Entity
    {
		public static ParticleType P_Shatter;

		public static ParticleType P_Regen;

		public static ParticleType P_Glow;

		public static ParticleType P_ShatterTwo;

		public static ParticleType P_RegenTwo;

		public static ParticleType P_GlowTwo;

		private Sprite sprite;

		private Sprite flash;

		private Image outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Level level;

		private SineWave sine;

		public bool twoDashes;

		public bool metalMode;

		public bool superBounceMode;

		private bool oneUse;

		private ParticleType p_shatter;

		private ParticleType p_regen;

		private ParticleType p_glow;

		public SnowballRefill(Vector2 position, bool twoDashes, bool oneUse, bool metalMode, bool superBounceMode)
            : base(position)
        {
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            this.twoDashes = twoDashes;
            this.oneUse = oneUse;
            if (twoDashes)
            {
                p_shatter = P_ShatterTwo;
                p_regen = P_RegenTwo;
                p_glow = P_GlowTwo;
            }
            else
            {
                p_shatter = P_Shatter;
                p_regen = P_Regen;
                p_glow = P_Glow;
            }
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));
			Add(sprite = FlavorsHelperModule.spriteBank.Create("snowballrefill"));
			Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            base.Depth = -100;
            this.metalMode = metalMode;
            this.superBounceMode = superBounceMode;
			if (twoDashes)
			{
				sprite.Play("twodashidle");
			}
			else if(metalMode)
			{
				sprite.Play("metalidle");
			}
			else if (superBounceMode)
            {
				sprite.Play("superbounceidle");
            }
            
        }

        public SnowballRefill(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"), data.Bool("MetalMode"), data.Bool("SuperBounceMode"))
		{

		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			base.Update();
			UpdateY();
			light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
			bloom.Alpha = light.Alpha * 0.8f;
			if (base.Scene.OnInterval(2f) && sprite.Visible)
			{
				//sprite.Play("flash", restart: true);
				sprite.Visible = true;
			}
		}

		private void UpdateY()
		{
			Sprite obj = sprite;
			Sprite obj2 = sprite;
			float num2 = (bloom.Y = sine.Value * 2f);
			float num5 = (obj.Y = (obj2.Y = num2));
		}

		public override void Render()
		{
			if (sprite.Visible)
			{
				sprite.DrawOutline();
			}
			base.Render();
		}

		public void OnSnowball(PowerUpSnowball snowball)
        {
			if (twoDashes && !snowball.twoDashState)
			{
				snowball.twoDashState = true;
			}
			else if (metalMode && !snowball.metalState)
			{
				snowball.metalState = true;
			}
			else if(superBounceMode && !snowball.superBounceState)
            {
				snowball.superBounceState = true;
            }
			Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", snowball.Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Collidable = false;
			Add(new Coroutine(SnowballRefillRoutine(snowball)));		
		}
		public void OnSnowballTwo(PowerUpSnowballTwo snowball)
		{
			if (twoDashes && !snowball.twoDashState)
			{
				snowball.twoDashState = true;
			}
			else if (metalMode && !snowball.metalState)
			{
				snowball.metalState = true;
			}
			else if (superBounceMode && !snowball.superBounceState)
			{
				snowball.superBounceState = true;
			}
			Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", snowball.Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Collidable = false;
			Add(new Coroutine(SnowballTwoRefillRoutine(snowball)));
		}
		private IEnumerator SnowballRefillRoutine(PowerUpSnowball snowball)
		{
			yield return null;
			sprite.Visible = false;
			Depth = 8999;
			yield return 0.05f;
			SlashFx.Burst(Position, 95f);
			RemoveSelf();
			
		}
		private IEnumerator SnowballTwoRefillRoutine(PowerUpSnowballTwo snowball)
		{
			yield return null;
			sprite.Visible = false;
			Depth = 8999;
			yield return 0.05f;
			SlashFx.Burst(Position, 95f);
			RemoveSelf();

		}
	}
}
