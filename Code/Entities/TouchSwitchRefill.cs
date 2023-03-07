using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/TouchSwitchRefill")]
    [Tracked]
    [TrackedAs(typeof(Refill))]
    public class TouchSwitchRefill : Refill
    {
        public TouchSwitchRefill(Vector2 position, bool twoDashes, bool oneUse)
            : base(position, twoDashes, oneUse)
        {
            
        }

        public TouchSwitchRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"))
        {
        }
        
        public bool AllTouchSwitchesHit()
        {
            Level level = this.Scene as Level;
            foreach (TouchSwitch touchswitch in level.Tracker.GetEntities<TouchSwitch>())
            {
                if (!touchswitch.Switch.Activated)
                {
                    return false;
                }
            }
            return true;
        }
        public override void Update()
        {
            DynamicData refillData = DynamicData.For(this);
            Sprite sprite = refillData.Get<Sprite>("sprite");
            Image outline = refillData.Get<Image>("outline"); 
            if (!AllTouchSwitchesHit())
            {
                sprite.Visible = false;
                outline.Visible = true;
                Collidable = false;
            }
            else
            {
                sprite.Visible = true;
                outline.Visible = false;
                Collidable = true;
            }
            refillData.Set("sprite", sprite);
            refillData.Set("outline", outline);
            base.Update();
        }
    }
}
