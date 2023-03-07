using Celeste;
using Celeste.Mod.Entities;
using FlavorsHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using FlavorsHelper;

namespace FlavorsHelper.Triggers
{
	public class UnDreamBlockTrigger
    {
		[CustomEntity(new string[] { "FlavorsHelper/ActivateUnDreamBlocksTrigger" })]
		public class ActivateUnDreamBlocksTrigger : Trigger
		{
			private bool rumble;

			private bool activate;

			private bool fastAnimation;

			public float dreamDashBoost;

			public float starFlyDreamDashSpeed;

			public bool soundEffectOn;

			public ActivateUnDreamBlocksTrigger(EntityData data, Vector2 offset)
				: base(data, offset)
			{
				rumble = data.Bool("fullRoutine");
				activate = data.Bool("activate", defaultValue: true);
				fastAnimation = data.Bool("fastAnimation");
				dreamDashBoost = data.Float("dreamDashBoost");
				starFlyDreamDashSpeed = data.Float("starFlyDreamDashSpeed");
				soundEffectOn = data.Bool("soundEffectOn");
			}

			public override void OnEnter(Player player)
			{
				Level level = base.Scene as Level;
				Session session = level.Session;
				DynamicData sessionData = DynamicData.For(session);
				sessionData.Set("isEnabled", true);
                DynamicData playerData = DynamicData.For(player);
				playerData.Set("soundEffectOn", soundEffectOn);
				playerData.Set("dreamDashBoost", dreamDashBoost);
				playerData.Set("starFlyDreamDashSpeed", starFlyDreamDashSpeed);
				playerData.Set("isEnabled", true);
                foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
				{
					DynamicData dreamblockData = DynamicData.For(dreamblock);
					dreamblockData.Set("isAnUnDreamBlock", false);
					dreamblockData.Set("dreamDashMultiplier", 1f);
					if (dreamblock is UnDreamBlock == false)
					{
						dreamblockData.Set("isActivated", true);
					}
				}
				foreach (UnDreamBlock undreamblock in level.Tracker.GetEntities<UnDreamBlock>())
				{
					undreamblock.isAnUnDreamBlock = true;
				}
				if (activate)
				{
					foreach (UnDreamBlock entity in level.Tracker.GetEntities<UnDreamBlock>())
					{
						if (!entity.isOpposite)
                        {
							entity.isActivated = true;
							if (rumble)
							{
								if (fastAnimation)
								{
									entity.Add(new Coroutine(entity.FastActivate()));
								}
								else
								{
									entity.Add(new Coroutine(entity.Activate()));
								}
							}
							else
							{
								entity.ActivateNoRoutine();
							}
						}
                        if (entity.isOpposite)
                        {
							entity.isActivated = false;
							if (rumble)
							{
								if (fastAnimation)
								{
									entity.Add(new Coroutine(entity.FastDeactivate()));
								}
								else
								{
									entity.Add(new Coroutine(entity.Deactivate()));
								}
							}
							else
							{
								entity.DeactivateNoRoutine();
							}
						}
					}
					return;
				}
				foreach (UnDreamBlock entity2 in level.Tracker.GetEntities<UnDreamBlock>())
				{
					if (!entity2.isOpposite)
                    {
						entity2.isActivated = false;
						if (rumble)
						{
							if (fastAnimation)
							{
								entity2.Add(new Coroutine(entity2.FastDeactivate()));
							}
							else
							{
								entity2.Add(new Coroutine(entity2.Deactivate()));
							}
						}
						else
						{
							entity2.DeactivateNoRoutine();
						}
					}
					if (entity2.isOpposite)
					{
						entity2.isActivated = true;
						if (rumble)
						{
							if (fastAnimation)
							{
								entity2.Add(new Coroutine(entity2.FastActivate()));
							}
							else
							{
								entity2.Add(new Coroutine(entity2.Activate()));
							}
						}
						else
						{
							entity2.ActivateNoRoutine();
						}
					}
				}
			}
		}
	}
}
