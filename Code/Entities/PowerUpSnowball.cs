using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/PowerUpSnowball")]
    [Tracked]
    public class PowerUpSnowball : Entity
    {
		private const float ResetTime = 0.8f;

		public Sprite sprite;

		private TouchSwitchCollider touchCollider;

		private SnowballRefillCollider refillCollider;

		private SnowballFieldCollider fieldCollider;

		private BounceBlockCollider blockCollider;

		public float resetTimer;

		public Level level;

		public SineWave sine;

		public float atY;

		public SoundSource spawnSfx;

		private Collider bounceCollider;

		public float SnowballDirection;

		public float SnowballSpeed;

		public float BaseSnowballSpeed;

		public float SnowballBounceSpeed;

		public float SnowballSuperBounceSpeed;

		public bool SnowballOnScreen;

		public bool twoDashState;

		public bool metalState;

		public bool superBounceState;

		public Holdable Hold;

		public float airTime;

		public PowerUpSnowball()
		{
			base.Depth = -12500;
			base.Collider = new Hitbox(12f, 9f, -5f, -2f);
			bounceCollider = new Hitbox(12f, 6f, -6f, -8f);
			Add(new PlayerCollider(OnPlayer));
			Add(new PlayerCollider(OnPlayerBounce, bounceCollider));
			Add(touchCollider = new TouchSwitchCollider(OnTouchSwitch, new Hitbox(6f, 12f, -10f, -10f)));
			Add(refillCollider = new SnowballRefillCollider(OnRefill, new Hitbox(6f, 12f, -10f, -10f)));
			Add(fieldCollider = new SnowballFieldCollider(OnField, new Hitbox(6f, 12f, -10f, -10f)));
			Add(blockCollider = new BounceBlockCollider(OnBounceBlock, new Hitbox(6f, 12f, -10f, -10f)));
			Add(sine = new SineWave(0.5f, 0f));
			Add(sprite = FlavorsHelperModule.spriteBank.Create("snowball"));
			Add(Hold = new Holdable(0.3f));
			sprite.Play("spin");
			Add(spawnSfx = new SoundSource());
			twoDashState = false;
			metalState = false;
		}
		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			this.Visible = false;
		}
		public virtual void ResetPosition()
		{
			airTime = 0f;
			SnowballSpeed = BaseSnowballSpeed;
			twoDashState = false;
			metalState = false;
			superBounceState = false;
			Visible = true;
			SnowballOnScreen = true;
			Player player = level.Tracker.GetEntity<Player>();
			if (player != null)
			{
				spawnSfx.Play("event:/game/04_cliffside/snowball_spawn");
				Collidable = (Visible = true);
				resetTimer = 0f;
				float startDistance = 0f;
				if ((float)player.Facing == -1)
				{
					SnowballDirection = 1f;
					startDistance = 15f;
				}
				else if ((float)player.Facing == 1)
				{
					SnowballDirection = -1f;
					startDistance = 15f;
				}
				base.X = player.CenterX + startDistance * (float)player.Facing;
				
				float num = (atY = (base.Y = player.CenterY));
				sine.Reset();
				sprite.Play("spin");
			}
			else
			{
				resetTimer = 0.05f;
			}
		}
		public virtual void Destroy()
		{
			airTime = 0f;
			SnowballOnScreen = false;
			Collidable = false;
            if (twoDashState)
            {
				sprite.Play("pinkbreak");
            }
            else if (superBounceState)
            {
				sprite.Play("superbouncebreak");
            }
            else
            {
				sprite.Play("break");
            }
		}
		
		private void OnPlayer(Player player)
		{
			OnPlayerBounce(player);
		}
		private void OnPlayerBounce(Player player)
		{
			DynamicData playerData = DynamicData.For(player);
			if (!CollideCheck(player))
			{
                if (twoDashState)
                {
					player.Dashes = 2;
                }
				SnowballBounce(base.Top - 10f, player);
				if (!metalState)
                {
					Destroy();
				}
				Audio.Play("event:/game/general/thing_booped", Position);
			}
		}
		public override void Update()
		{
			base.Update();
			Player player = level.Tracker.GetEntity<Player>();
			if (SnowballOnScreen)
            {
				airTime += Engine.DeltaTime;
            }
			if (twoDashState && SnowballOnScreen)
			{
				sprite.Play("pinkspin");
			}
			if (metalState && SnowballOnScreen)
            {
				sprite.Play("metalspin");
            }
			if (superBounceState && SnowballOnScreen)
            {
				sprite.Play("superbouncespin");
            }
			foreach (TouchSwitch touchswitch in level.Tracker.GetEntities<TouchSwitch>())
            {
				OnTouchSwitch(touchswitch);
            }
			foreach(SnowballRefill refill in level.Tracker.GetEntities<SnowballRefill>())
            {
				OnRefill(refill);
            }
			foreach(SnowballField field in level.Tracker.GetEntities<SnowballField>())
            {
				OnField(field);
            }
			foreach(CustomBounceBlock block in level.Tracker.GetEntities<CustomBounceBlock>())
            {
				OnBounceBlock(block);
            }
			base.X -= SnowballSpeed * Engine.DeltaTime * SnowballDirection;
			if (player == Hold.Holder)
			{
				sprite.Stop();
				base.Y = player.Position.Y - 20f;
				base.X = player.Position.X;
				SnowballDirection = -1f * (float)player.Facing;
				atY = player.Position.Y - 20f;
			}
            else
            {
				base.Y = atY;
			}
			if (base.X < level.Camera.Left - 30f || base.X > level.Camera.Right + 30f)
            {
				SnowballOnScreen = false;
				airTime = 0f;
            }
			if (Input.Grab.Pressed && !SnowballOnScreen)
			{

				ResetPosition();
			}
		}
		public override void Render()
		{
			sprite.DrawOutline();
			base.Render();
		}
		public void OnTouchSwitch(TouchSwitch touchswitch)
        {
			if (touchCollider.Check(touchswitch))
            {
				touchswitch.TurnOn();
			}
        }
		public void OnRefill(SnowballRefill refill)
        {
			if (refillCollider.Check(refill))
            {
				refill.OnSnowball(this);
            }
        }
		public void OnField(SnowballField field)
        {
			if (fieldCollider.Check(field))
            {
				field.OnSnowball(this);
            }
        }
		public void OnBounceBlock(CustomBounceBlock block)
        {
			if (blockCollider.Check(block))
            {
				block.OnSnowball(this);
				block.ToggleSprite();
            }
        }
		public void SnowballBounce(float fromY, Player player)
        {
			DynamicData playerData = DynamicData.For("player");
			if (player.StateMachine.State == 4 && player.CurrentBooster != null)
			{
				player.CurrentBooster.PlayerReleased();
				player.CurrentBooster = null;
			}
			Collider collider = player.Collider;
			player.Collider = new Hitbox(8f, 11f, -4f, -11f);
			player.MoveV(fromY - player.Bottom);
			if (!player.Inventory.NoRefills)
			{
				player.RefillDash();
			}
			player.RefillStamina();
			player.StateMachine.State = 0;
			playerData.Set("jumpGraceTimer", 0f);
			playerData.Set("varJumpTimer", 0.2f);
			player.AutoJump = true;
			player.AutoJumpTimer = 0.1f;
			playerData.Set("dashAttackTimer", 0f);
			playerData.Set("gliderBoostTimer", 0f);
			playerData.Set("wallSlideTimer", 1.2f);
			playerData.Set("wallBoostTimer", 0f);
			playerData.Set("varJumpSpeed", -185f);
            if (superBounceState)
            {
				player.Speed.Y = -(SnowballSuperBounceSpeed);
            }
            else
            {
				player.Speed.Y = -(SnowballBounceSpeed);
			}
			playerData.Set("launched", false);
			level.DirectionalShake(-Vector2.UnitY, 0.1f);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			player.Sprite.Scale = new Vector2(0.5f, 1.5f);
			player.Collider = collider;
		}
		
	}
}
