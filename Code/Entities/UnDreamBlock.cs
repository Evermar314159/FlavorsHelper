using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using System;

namespace FlavorsHelper.Entities
{
    [CustomEntity("FlavorsHelper/UnDreamBlock")]
	[Tracked]
    [TrackedAs(typeof(DreamBlock))]
    public class UnDreamBlock : DreamBlock
    {
        
        public struct DreamParticle
        {
            public Vector2 Position;

            public int Layer;

            public Color Color;

            public float TimeOffset;
        }

        public bool isOpposite;

        public bool isActivated;

        public bool twoDashes;

        public Color activeBackColor;

        public Color disabledBackColor;

        private Color activeLineColor = Color.White;

        private Color disabledLineColor = Calc.HexToColor("6a8480");

        private bool playerHasDreamDash = true;

        private Vector2? node;

        private LightOcclude occlude;

        private MTexture[] particleTextures;

        public DreamParticle[] particles;

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

        private int randomSeed;

        public bool isAnUnDreamBlock;

        public float dreamDashMultiplier;

		public UnDreamBlock(Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse, bool below, bool isOpposite, bool twoDashes, int Depth, Color activeBackColor, Color disabledBackColor)
            : base(position, width, height, node, fastMoving, oneUse, below)
        {
            DreamBlock dreamBlock = this;
            DynamicData dreamBlockData = DynamicData.For(dreamBlock);
            this.Depth = Depth;
            this.node = node;
            this.fastMoving = fastMoving;
            this.oneUse = oneUse;
            this.isOpposite = isOpposite;
            this.twoDashes = twoDashes;
            this.activeBackColor = activeBackColor;
            this.disabledBackColor = disabledBackColor;
            isAnUnDreamBlock = true;
            dreamDashMultiplier = 1f;
            if (below)
            {
                base.Depth = 5000;
            }
            SurfaceSoundIndex = 11;
            particleTextures = new MTexture[4]
            {
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
            GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
            };
        }
        public UnDreamBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), data.Bool("fastMoving"), data.Bool("oneUse"), data.Bool("below"), data.Bool("isOpposite"), data.Bool("twoDashes"), data.Int("Depth"), data.HexColor("activeBackColor"), data.HexColor("disabledBackColor"))
		{

		}

    }
}
