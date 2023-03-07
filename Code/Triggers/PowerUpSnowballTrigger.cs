using Celeste;
using Celeste.Mod.Entities;
using FlavorsHelper.Entities;
using Microsoft.Xna.Framework;

namespace FlavorsHelper.Triggers
{
    [CustomEntity(new string[] { "FlavorsHelper/PowerUpSnowballTrigger" })]
	public class PowerUpSnowballTrigger : Trigger
    {
		public float SnowballSpeed;

		public float SnowballBounceSpeed;

		public float SnowballSuperBounceSpeed;

		public bool MetalMode;

		public bool TwoDashMode;

		public bool SuperBounceMode;

		public PowerUpSnowball snowball;

		public PowerUpSnowballTwo snowballtwo;
		public PowerUpSnowballTrigger(EntityData data, Vector2 offset)
		: base(data, offset)
		{
			SnowballSpeed = data.Float("SnowballSpeed");
			SnowballBounceSpeed = data.Float("SnowballBounceSpeed");
			SnowballSuperBounceSpeed = data.Float("SnowballSuperBounceSpeed");
			MetalMode = data.Bool("MetalMode");
			TwoDashMode = data.Bool("TwoDashMode");
			SuperBounceMode = data.Bool("SuperBounceMode");
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (base.Scene.Entities.FindFirst<PowerUpSnowball>() == null)
			{
				base.Scene.Add(snowball = new PowerUpSnowball());
				snowball.BaseSnowballSpeed = SnowballSpeed;
				snowball.SnowballSpeed = SnowballSpeed;
				snowball.SnowballBounceSpeed = SnowballBounceSpeed;
				snowball.SnowballSuperBounceSpeed = SnowballSuperBounceSpeed;
				snowball.twoDashState = TwoDashMode;
				snowball.superBounceState = SuperBounceMode;
				snowball.metalState = MetalMode;
			}
			if (base.Scene.Entities.FindFirst<PowerUpSnowballTwo>() == null)
			{
				base.Scene.Add(snowballtwo = new PowerUpSnowballTwo());
				snowballtwo.BaseSnowballSpeed = SnowballSpeed;
				snowballtwo.SnowballSpeed = SnowballSpeed;
				snowballtwo.SnowballBounceSpeed = SnowballBounceSpeed;
				snowballtwo.SnowballSuperBounceSpeed = SnowballSuperBounceSpeed;
				snowballtwo.twoDashState = TwoDashMode;
				snowballtwo.superBounceState = SuperBounceMode;
				snowballtwo.metalState = MetalMode;
			}
			RemoveSelf();
		}
	}
}
