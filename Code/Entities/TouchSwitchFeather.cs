using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Monocle;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/TouchSwitchFeather")]
    [Tracked]
    [TrackedAs(typeof(FlyFeather))]
    public class TouchSwitchFeather : FlyFeather
    {
        public int playerState;
        public TouchSwitchFeather(Vector2 position, bool shielded, bool singleUse)
        : base(position, shielded, singleUse)
        {
            
        }
        public TouchSwitchFeather(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("shielded"), data.Bool("singleUse"))
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
            DynamicData featherData = DynamicData.For(this);
            Sprite sprite = featherData.Get<Sprite>("sprite");
            Image outline = featherData.Get<Image>("outline");
            if (!AllTouchSwitchesHit() || playerState == 19)
            {
                Collidable = false;
                outline.Visible = true;
                sprite.Visible = false;
            }
            else
            {
                Collidable = true;
                outline.Visible = false;
                sprite.Visible = true;
            }
            featherData.Set("sprite", sprite);
            featherData.Set("outline", outline);
            base.Update();
        }
        
    }
}
