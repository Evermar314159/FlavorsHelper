using Celeste.Mod;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using FlavorsHelper.Entities;
using MonoMod.Utils;
using System;
using Platform = Celeste.Platform;
using System.Collections.Generic;
using Celeste.Mod.CommunalHelper.Entities;

namespace FlavorsHelper
{
    public class FlavorsHelperModule : EverestModule {

        public static SpriteBank spriteBank;

        // Only one alive module instance can exist at any given time.
        public static FlavorsHelperModule Instance;


        public FlavorsHelperModule() {
            Instance = this;
        }

        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load()
        {
            //IL.Celeste.Player.SuperWallJump += Player_SuperWallJump;
            On.Celeste.Session.ctor += Session_ctor;
            On.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;
            On.Celeste.Player.DreamDashUpdate += Player_DreamDashUpdate;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.OnCollideH += Player_OnCollideH;
            On.Celeste.Player.OnCollideV += Player_OnCollideV;
            On.Celeste.Player.StarFlyUpdate += Player_StarFlyUpdate;
            On.Celeste.Player.DreamDashEnd += Player_DreamDashEnd;
            On.Celeste.Player.CreateTrail += Player_CreateTrail;
            On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
            On.Celeste.Solid.MoveVExact += Solid_MoveVExact;
            On.Celeste.Solid.MoveHExact += Solid_MoveHExact;
            On.Celeste.Player.ctor += Player_ctor;
            On.Celeste.Refill.ctor_Vector2_bool_bool += Refill_ctor;
            On.Celeste.DreamBlock.ctor_Vector2_float_float_Nullable1_bool_bool_bool += DreamBlock_ctor;
        }

       

        // Optional, initialize anything after Celeste has initialized itself properly.
        public override void Initialize() {
        }

        // Optional, do anything requiring either the Celeste or mod content here. 
        // Usually involves Spritebanks or custom particle effects.
        public override void LoadContent(bool firstLoad) 
        {
            base.LoadContent(firstLoad);
            spriteBank = new SpriteBank(Celeste.GFX.Game, "Graphics/FlavorsHelper/CustomSprites.xml");
        }

        // Unload the entirety of your mod's content. Free up any native resources.
        public override void Unload()
        {
            //IL.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
            On.Celeste.Session.ctor -= Session_ctor;
            On.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;
            On.Celeste.Player.DreamDashUpdate -= Player_DreamDashUpdate;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.OnCollideH -= Player_OnCollideH;
            On.Celeste.Player.OnCollideV -= Player_OnCollideV;
            On.Celeste.Player.StarFlyUpdate -= Player_StarFlyUpdate;
            On.Celeste.Player.DreamDashEnd -= Player_DreamDashEnd;
            On.Celeste.Player.CreateTrail -= Player_CreateTrail;
            On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
            On.Celeste.Solid.MoveVExact -= Solid_MoveVExact;
            On.Celeste.Solid.MoveHExact -= Solid_MoveHExact;
            On.Celeste.Player.ctor -= Player_ctor;
            On.Celeste.Refill.ctor_Vector2_bool_bool -= Refill_ctor;
            On.Celeste.DreamBlock.ctor_Vector2_float_float_Nullable1_bool_bool_bool -= DreamBlock_ctor;
        }
        public static bool isNextToUnDreamBlock(UnDreamBlock undreamblock)
        {
            Level level = undreamblock.Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            return player.CollideCheck(undreamblock, player.Position + (5f * Vector2.UnitX)) || player.CollideCheck(undreamblock, player.Position - (5f * Vector2.UnitX));

        }
        public static bool isNextToDreamBlock(DreamBlock dreamblock)
        {
            Level level = dreamblock.Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            return player.CollideCheck(dreamblock, player.Position + (5f * Vector2.UnitX)) || player.CollideCheck(dreamblock, player.Position - (5f * Vector2.UnitX));

        }
        public static bool isAboveBelowUnDreamBlock(UnDreamBlock undreamblock)
        {
            Level level = undreamblock.Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            return player.CollideCheck(undreamblock, player.Position + (5f * Vector2.UnitY)) || player.CollideCheck(undreamblock, player.Position - (5f * Vector2.UnitY));

        }
        public static bool isAboveBelowDreamBlock(DreamBlock dreamblock)
        {
            Level level = dreamblock.Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            return player.CollideCheck(dreamblock, player.Position + (5f * Vector2.UnitY)) || player.CollideCheck(dreamblock, player.Position - (5f * Vector2.UnitY));

        }
        private static float DetermineWallBouncingSpeedFactor()
        {
            Level level = Engine.Scene as Level;
            foreach (UnDreamBlock undreamblock in level.Tracker.GetEntities<UnDreamBlock>())
            {
                if (isNextToUnDreamBlock(undreamblock) && undreamblock.isActivated)
                {
                    return 1.5f;
                }
            }
            foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
            {
                DynamicData dreamblockData = DynamicData.For(dreamblock);
                bool isAnUnDreamBlock = dreamblockData.Get<bool>("isAnUnDreamBlock");
                if (isNextToDreamBlock(dreamblock) && !isAnUnDreamBlock)
                {
                    return 1.5f;
                }
            }
            return 1f;
        }
        private void ToggleUnDreamBlock(UnDreamBlock undreamblock)
        {
            undreamblock.isActivated = !undreamblock.isActivated;
            if (undreamblock.isActivated)
            {
                undreamblock.ActivateNoRoutine();
            }
            else
            {
                undreamblock.DeactivateNoRoutine();
            }
        }
        public void Session_ctor(On.Celeste.Session.orig_ctor orig, Session session)
        {
            orig(session);
            DynamicData sessionData = DynamicData.For(session);
            sessionData.Set("isEnabled", false);
        }
        
        public void Refill_ctor(On.Celeste.Refill.orig_ctor_Vector2_bool_bool orig, Refill refill, Vector2 position, bool twoDashes, bool oneUse)
        { 
            orig(refill, position, twoDashes, oneUse);
            Level level = refill.Scene as Level;
            if (level != null)
            {
                Session session = level.Session;
                DynamicData sessionData = DynamicData.For(session);
                bool isEnabled = sessionData.Get<bool>("isEnabled");
                if (isEnabled)
                {
                    refill.Depth = -15000;
                }
            }
        }
        public void DreamBlock_ctor(On.Celeste.DreamBlock.orig_ctor_Vector2_float_float_Nullable1_bool_bool_bool orig, DreamBlock dreamblock, Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse, bool below)
        {
            orig(dreamblock, position, width, height, node, fastMoving, oneUse, below);
            Level level = dreamblock.Scene as Level;
            if (level != null)
            {
                Session session = level.Session;
                DynamicData sessionData = DynamicData.For(session);
                bool isEnabled = sessionData.Get<bool>("isEnabled");
                if (isEnabled)
                {
                    DynamicData dreamblockData = DynamicData.For(dreamblock);
                    dreamblockData.Set("isActivated", true);
                    dreamblockData.Set("isAnUnDreamBlock", false);
                }
            } 
        }
        public void Player_ctor(On.Celeste.Player.orig_ctor orig, Player player, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(player, position, spriteMode);
            DynamicData playerData = DynamicData.For(player);
            playerData.Set("timer", 0f);
            playerData.Set("superWallBounced", false);
            playerData.Set("dashCounter", 0);
            playerData.Set("starFlyDreamDash", false);
            //Sprite dreamDashSprite = FlavorsHelperModule.spriteBank.Create("colordreamDash");
            //player.Add(dreamDashSprite);
            //playerData.Set("dreamDashSprite", dreamDashSprite);
            playerData.Set("soundEffectOn", false);
        }
        public void Solid_MoveHExact(On.Celeste.Solid.orig_MoveHExact orig, Solid solid, int move)
        {
            Level level = solid.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData solidData = DynamicData.For(solid);
                HashSet<Actor> riders = solidData.Get<HashSet<Actor>>("riders");
                solid.GetRiders();
                float right = solid.Right;
                float left = solid.Left;
                Player player = null;
                player = solid.Scene.Tracker.GetEntity<Player>();
                if (player != null && Input.MoveX.Value == Math.Sign(move) && Math.Sign(player.Speed.X) == Math.Sign(move) && !riders.Contains(player) && solid.CollideCheck(player, solid.Position + Vector2.UnitX * move - Vector2.UnitY))
                {
                    player.MoveV(1f);
                }
                solid.X += move;
                solid.MoveStaticMovers(Vector2.UnitX * move);
                if (solid.Collidable)
                {
                    foreach (Actor entity in solid.Scene.Tracker.GetEntities<Actor>())
                    {
                        if (!entity.AllowPushing)
                        {
                            continue;
                        }
                        bool collidable = entity.Collidable;
                        entity.Collidable = true;
                        if (!entity.TreatNaive && solid.CollideCheck(entity, solid.Position) && !(solid is DreamMoveBlock && entity.CollideCheck(solid)))
                        {
                            int moveH = ((move <= 0) ? (move - (int)(entity.Right - left)) : (move - (int)(entity.Left - right)));
                            solid.Collidable = false;
                            entity.MoveHExact(moveH, entity.SquishCallback, solid);
                            entity.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        }
                        else if (riders.Contains(entity) && !(solid is DreamMoveBlock && entity.CollideCheck(solid)))
                        {
                            solid.Collidable = false;
                            if (entity.TreatNaive)
                            {
                                entity.NaiveMove(Vector2.UnitX * move);
                            }
                            else
                            {
                                entity.MoveHExact(move);
                            }
                            entity.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        }
                        entity.Collidable = collidable;
                    }
                }
                riders.Clear();
            }
            else
            {
                orig(solid, move);
            }
        }
        public void Solid_MoveVExact(On.Celeste.Solid.orig_MoveVExact orig, Solid solid, int move)
        {
            Level level = solid.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData solidData = DynamicData.For(solid);
                HashSet<Actor> riders = solidData.Get<HashSet<Actor>>("riders");
                solid.GetRiders();
                float bottom = solid.Bottom;
                float top = solid.Top;
                solid.Y += move;
                solid.MoveStaticMovers(Vector2.UnitY * move);
                if (solid.Collidable)
                {
                    foreach (Actor entity in solid.Scene.Tracker.GetEntities<Actor>())
                    {
                        if (!entity.AllowPushing)
                        {
                            continue;
                        }
                        bool collidable = entity.Collidable;
                        entity.Collidable = true;
                        if (!entity.TreatNaive && solid.CollideCheck(entity, solid.Position) && !(solid is DreamMoveBlock && entity.CollideCheck(solid)))
                        {
                            int moveV = ((move <= 0) ? (move - (int)(entity.Bottom - top)) : (move - (int)(entity.Top - bottom)));
                            solid.Collidable = false;
                            entity.MoveVExact(moveV, entity.SquishCallback, solid);
                            entity.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        }
                        else if (riders.Contains(entity) && !(solid is DreamMoveBlock && entity.CollideCheck(solid)))
                        {
                            solid.Collidable = false;
                            if (entity.TreatNaive)
                            {
                                entity.NaiveMove(Vector2.UnitY * move);
                            }
                            else
                            {
                                entity.MoveVExact(move);
                            }
                            entity.LiftSpeed = solid.LiftSpeed;
                            solid.Collidable = true;
                        }
                        entity.Collidable = collidable;
                    }
                }
                riders.Clear();
            }
            else
            {
                orig(solid, move);
            }
        }
        private void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player player, int dir)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData playerData = DynamicData.For(player);
                player.Ducking = false;
                Input.Jump.ConsumeBuffer();
                playerData.Set("superWallBounced", true);
                playerData.Set("jumpGraceTimer", 0f);
                playerData.Set("varJumpTimer", 0.25f);
                player.AutoJump = false;
                playerData.Set("dashAttackTimer", 0f);
                playerData.Set("gliderBoostTimer", 0.55f);
                playerData.Set("gliderBoostDir", -Vector2.UnitY);
                playerData.Set("wallSlideTimer", 1.2f);
                playerData.Set("wallBoostTimer", 0f);
                player.Speed.X = 170f * (float)dir;
                player.Speed.Y = -160f * DetermineWallBouncingSpeedFactor();
                player.Speed += playerData.Get<Vector2>("LiftBoost");
                playerData.Set("varJumpSpeed", player.Speed.Y);
                playerData.Set("launched", true);
                playerData.Invoke("CreateTrail");
                player.Play((dir < 0) ? "event:/char/madeline/jump_wall_right" : "event:/char/madeline/jump_wall_left");
                player.Play("event:/char/madeline/jump_superwall");
                player.Sprite.Scale = new Vector2(0.6f, 1.4f);
                int index = -1;
                List<Entity> temp = playerData.Get<List<Entity>>("temp");
                Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - Vector2.UnitX * dir * 4f, temp));
                if (platformByPriority != null)
                {
                    index = platformByPriority.GetWallSoundIndex(player, dir);
                }
                if (dir == -1)
                {
                    Dust.Burst(player.Center + Vector2.UnitX * 2f, (float)Math.PI * -3f / 4f, 4, (ParticleType)playerData.Invoke("DustParticleFromSurfaceIndex", index));
                }
                else
                {
                    Dust.Burst(player.Center + Vector2.UnitX * -2f, -(float)Math.PI / 4f, 4, (ParticleType)playerData.Invoke("DustParticleFromSurfaceIndex", index));
                }
                SaveData.Instance.TotalWallJumps++;
            }
            else
            {
                orig(player, dir);
            }
        }
        private void Player_CreateTrail(On.Celeste.Player.orig_CreateTrail orig, Player player)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData playerData = DynamicData.For(player);
                Sprite Sprite = playerData.Get<Sprite>("Sprite");
                Vector2 scale = new Vector2(Math.Abs(Sprite.Scale.X) * (float)player.Facing, Sprite.Scale.Y);
                int dashCounter = playerData.Get<int>("dashCounter");
                Color dreamDashColor = Calc.HexToColor("44B7FF");
                if (dashCounter == 1)
                {
                    dreamDashColor = Calc.HexToColor("E9F01F");
                    scale *= 1.5f;
                }
                else if (dashCounter == 2)
                {
                    dreamDashColor = Calc.HexToColor("F01F1F");
                    scale *= 1.65f;
                }
                TrailManager.Add(player, scale, dreamDashColor);
            }
            else
            {
                orig(player);
            }
        }
        
        private void Player_DreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player player)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData playerData = DynamicData.For(player);
                DreamBlock dreamBlock = playerData.Get<DreamBlock>("dreamBlock");
                orig(player);
                if (dreamBlock is UnDreamBlock)
                {
                    UnDreamBlock undreamBlock = dreamBlock as UnDreamBlock;
                    if (undreamBlock.twoDashes)
                    {
                        player.Dashes = 2;
                    }

                }
                foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
                {
                    DynamicData dreamblockData = DynamicData.For(dreamblock);
                    dreamblockData.Set("dreamDashMultiplier", 1f);
                }
                playerData.Set("starFlyDreamDash", false);
                playerData.Set("dashCounter", 0);
            }
            else
            {
                orig(player);
            }
        }
        private int Player_StarFlyUpdate(On.Celeste.Player.orig_StarFlyUpdate orig, Player player)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData playerData = DynamicData.For(player);
                playerData.Set("starFlyTimer", 10000f);
                player.Dashes = 0;
            }
            return orig(player);
        }
        private void Player_OnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player player, CollisionData data)
        {
            orig(player, data);
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                if (player.StateMachine.State == 19)
                {
                    foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
                    {
                        DynamicData dreamblockData = DynamicData.For(dreamblock);
                        bool isActivated = dreamblockData.Get<bool>("isActivated");
                        if (isNextToDreamBlock(dreamblock) && isActivated)
                        {
                            DynamicData playerData = DynamicData.For(player);
                            player.DashDir = Vector2.UnitX * (float)player.Facing;
                            player.StateMachine.State = 9;
                            playerData.Set("dashAttackTimer", 0f);
                            playerData.Set("gliderBoostTimer", 0f);
                            playerData.Set("starFlyDreamDash", true);
                            return;
                        }
                    }
                }
            }
        }
        private void Player_OnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player player, CollisionData data)
        {
            orig(player, data);
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                if (player.StateMachine.State == 19)
                {
                    foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
                    {
                        DynamicData dreamblockData = DynamicData.For(dreamblock);
                        bool isActivated = dreamblockData.Get<bool>("isActivated");
                        if (isAboveBelowDreamBlock(dreamblock) && isActivated)
                        {
                            DynamicData playerData = DynamicData.For(player);
                            player.DashDir = Vector2.UnitY * Input.GetAimVector().Y;
                            player.StateMachine.State = 9;
                            playerData.Set("dashAttackTimer", 0f);
                            playerData.Set("gliderBoostTimer", 0f);
                            playerData.Set("starFlyDreamDash", true);
                            return;
                        }
                    }
                }
            }
        }
        //private void Player_SuperWallJump(ILContext il)
        //{
        //    ILCursor cursor = new ILCursor(il);
        //    while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160f)))
        //    {
        //        cursor.EmitDelegate<Func<float>>(DetermineWallBouncingSpeedFactor);
        //        cursor.Emit(OpCodes.Mul);
        //    }
        //}
        private bool Player_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player player, Vector2 dir)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
                {
                    DynamicData dreamblockData = DynamicData.For(dreamblock);
                    bool isActivated = dreamblockData.Get<bool>("isActivated");
                    if (isAboveBelowDreamBlock(dreamblock) || isNextToDreamBlock(dreamblock))
                    {
                        if (isActivated)
                        {
                            return orig(player, dir);
                        }
                    }
                }
                return false;
            }
            else
            {
                return orig(player, dir);
            }
        }
        private int Player_DreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player player)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData playerData = DynamicData.For(player);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                Vector2 position = player.Position;
                player.NaiveMove(player.Speed * Engine.DeltaTime);
                float dreamDashCanEndTimer = playerData.Get<float>("dreamDashCanEndTimer");
                if (dreamDashCanEndTimer > 0f)
                {
                    dreamDashCanEndTimer -= Engine.DeltaTime;
                    playerData.Set("dreamDashCanEndTimer", dreamDashCanEndTimer);
                }
                DreamBlock dreamBlock = player.CollideFirst<DreamBlock>();
                if (dreamBlock == null)
                {
                    bool dreamDashedIntoSolid = (bool)playerData.Invoke("DreamDashedIntoSolid");
                    if (dreamDashedIntoSolid)
                    {
                        if (SaveData.Instance.Assists.Invincible)
                        {
                            player.Position = position;
                            player.Speed *= -1f;
                            player.Play("event:/game/general/assist_dreamblockbounce");
                        }
                        else
                        {
                            player.Die(Vector2.Zero);
                        }
                    }
                    else if (dreamDashCanEndTimer <= 0f)
                    {
                        global::Celeste.Celeste.Freeze(0.05f);
                        if (Input.Jump.Pressed && player.DashDir.X != 0f)
                        {
                            playerData.Set("dreamJump", true);
                            player.Jump();
                        }
                        else if (player.DashDir.Y >= 0f || player.DashDir.X != 0f)
                        {
                            if (player.DashDir.X > 0f && player.CollideCheck<Solid>(player.Position - Vector2.UnitX * 5f))
                            {
                                player.MoveHExact(-5);
                            }
                            else if (player.DashDir.X < 0f && player.CollideCheck<Solid>(player.Position + Vector2.UnitX * 5f))
                            {
                                player.MoveHExact(5);
                            }
                            bool flag = player.ClimbCheck(-1);
                            bool flag2 = player.ClimbCheck(1);
                            int moveX = playerData.Get<int>("moveX");
                            if (Input.Grab.Check && ((moveX == 1 && flag2) || (moveX == -1 && flag)))
                            {
                                player.Facing = (Facings)moveX;
                                if (!SaveData.Instance.Assists.NoGrabbing)
                                {
                                    return 1;
                                }
                                player.ClimbTrigger(moveX);
                                player.Speed.X = 0f;
                            }
                        }
                        return 0;
                    }
                }
                else
                {
                    DynamicData dreamBlockData = DynamicData.For(dreamBlock);
                    float dreamDashMultiplier = dreamBlockData.Get<float>("dreamDashMultiplier");
                    playerData.Set("dreamBlock", dreamBlock);
                    if (player.Scene.OnInterval(0.1f))
                    {
                        playerData.Invoke("CreateTrail");
                    }
                    if (player.Scene.OnInterval(0.04f))
                    {
                        DisplacementRenderer.Burst burst = level.Displacement.AddBurst(player.Center, 0.3f, 0f, 40f);
                        burst.WorldClipCollider = dreamBlock.Collider;
                        burst.WorldClipPadding = 2;
                    }
                    if (player.CanDash && Input.Dash.Pressed)
                    {
                        player.Dashes -= 1;
                        player.Speed *= playerData.Get<float>("dreamDashBoost");
                        dreamDashMultiplier *= playerData.Get<float>("dreamDashBoost");
                        dreamBlockData.Set("dreamDashMultiplier", dreamDashMultiplier);
                        playerData.Set("dashCounter", playerData.Get<int>("dashCounter") + 1);

                    }
                    bool starFlyDreamDash = playerData.Get<bool>("starFlyDreamDash");
                    if (starFlyDreamDash)
                    {
                        float starFlyDreamDashSpeed = playerData.Get<float>("starFlyDreamDashSpeed");
                        Vector2 input = Input.Aim.Value.SafeNormalize();
                        if (input != Vector2.Zero)
                        {
                            Vector2 vector = player.Speed.SafeNormalize();
                            if (vector != Vector2.Zero)
                            {
                                vector = ((Vector2.Dot(input, vector) != -0.8f) ? vector.RotateTowards(input.Angle(), 5f * Engine.DeltaTime) : vector);
                                player.Speed = (player.DashDir = vector) * starFlyDreamDashSpeed * dreamDashMultiplier;
                            }
                        }
                    }
                }
                return 9;
            }
            else
            {
               return orig(player);
            }
        }
        public void Player_Update(On.Celeste.Player.orig_Update orig, Player player)
        {
            Level level = player.Scene as Level;
            Session session = level.Session;
            DynamicData sessionData = DynamicData.For(session);
            bool isEnabled = sessionData.Get<bool>("isEnabled");
            if (isEnabled)
            {
                DynamicData playerData = DynamicData.For(player);
                Sprite Sprite = playerData.Get<Sprite>("Sprite");
                //Sprite dreamDashSprite = playerData.Get<Sprite>("dreamDashSprite");
                //int dashCounter = playerData.Get<int>("dashCounter");
                Vector2 scale = new Vector2(Math.Abs(Sprite.Scale.X) * (float)player.Facing, Sprite.Scale.Y);
                float timer = playerData.Get<float>("timer");
                timer -= 1f;
                playerData.Set("timer", timer);
                if (timer <= 0f)
                {
                    playerData.Set("superWallBounced", false);
                }
                if (playerData.Get<bool>("superWallBounced"))
                {
                    Color WallBounceColor = Calc.Random.Choose<Color>(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                    TrailManager.Add(player, scale, WallBounceColor);
                    playerData.Invoke("CreateTrail");
                }
                //if (dashCounter == 1)
                //{
                //    dreamDashSprite.CenterOrigin();
                //    dreamDashSprite.Play("yellowdreamDash");
                //}
                //else if (dashCounter == 2)
                //{
                //    dreamDashSprite.CenterOrigin();
                //    dreamDashSprite.Play("reddreamDash");
                //}
                //else
                //{
                //    dreamDashSprite.Play("idle");
                //}
                playerData.Set("Sprite", Sprite);
                //playerData.Set("dreamDashSprite", dreamDashSprite);
                orig(player);
                foreach (TouchSwitchFeather feather in level.Tracker.GetEntities<TouchSwitchFeather>())
                {
                    feather.playerState = player.StateMachine.State;
                }
                if (Input.Grab.Pressed)
                {
                    foreach (UnDreamBlock undreamblock in level.Tracker.GetEntities<UnDreamBlock>())
                    {
                        ToggleUnDreamBlock(undreamblock);
                    }
                    if (playerData.Get<bool>("soundEffectOn"))
                    {
                        player.Play("event:/Evermar/PP_UnDreamBlock");
                    }
                }
                foreach (UnDreamBlock undreamblock in level.Tracker.GetEntities<UnDreamBlock>())
                {
                    if (player.CollideCheck(undreamblock, player.Position) && !undreamblock.isActivated)
                    {
                        player.Die(Vector2.Zero);
                    }
                }
                foreach (DreamBlock dreamblock in level.Tracker.GetEntities<DreamBlock>())
                {
                    DynamicData dreamblockData = DynamicData.For(dreamblock);
                    if (isNextToDreamBlock(dreamblock) && dreamblockData.Get<bool>("isActivated"))
                    {
                        playerData.Set("timer", 20f);
                    }
                }
            }
            else
            {
                orig(player);
            }
        }
    }
}

