using System;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{

    public class SuppBarbPlugin : BasePlugin, IInGameWorldPainter, ICustomizer, IInGameTopPainter
    {
        public bool OnlyShowInGreaters { get; set; }
        public bool ShowHealthGlobes { get; set; }
        public bool ShowHealthGlobesOnNonBarb { get; set; }

        public float XOffset { get; set; }
        public float YOffset { get; set; }

        public IFont WarningFont { get; set; }
        public WorldDecoratorCollection HealthGlobeDecorator { get; set; }

        private float HudWidth { get { return Hud.Window.Size.Width; } }
        private float HudHeight { get { return Hud.Window.Size.Height; } }
        private bool missingIP, tooFar;
        private string warningStr = "";

        public SuppBarbPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ShowHealthGlobes = true;
            ShowHealthGlobesOnNonBarb = true;
            OnlyShowInGreaters = false;

            XOffset = 0.048f;
            YOffset = 0;

            WarningFont = Hud.Render.CreateFont("tahoma", 23f, 200, 255, 0, 0, false, false, true);
            HealthGlobeDecorator = new WorldDecoratorCollection(
                 new MapShapeDecorator(Hud)
                 {
                     Brush = Hud.Render.CreateBrush(255, 66, 194, 244, 0),
                     ShadowBrush = Hud.Render.CreateBrush(200, 0, 0, 0, 1),
                     Radius = 4.5f,
                     ShapePainter = new CircleShapePainter(Hud),
                 },
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = Hud.Render.CreateBrush(255, 240, 0, 0, 0),
                    TextFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 0, 0, false, false, false),
                }
            );
        }

        public void Customize()
        {
            Hud.RunOnPlugin<PlayerBottomBuffListPlugin>(plugin =>
            {
                plugin.BuffPainter.ShowTimeLeftNumbers = true;
                plugin.RuleCalculator.Rules.Add(new BuffRule(79528) { IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, IconSizeMultiplier = 1.3f }); // Ignore pain 
            });
        }

        public void PaintWorld(WorldLayer layer)
        {
            if (!ShowHealthGlobes || !ShowHealthGlobesOnNonBarb && Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Barbarian || Hud.Game.Me.IsInTown) return;
            var actors = Hud.Game.Actors.Where(x => x.SnoActor.Kind == ActorKind.HealthGlobe);
            foreach (var actor in actors)
            {
                HealthGlobeDecorator.ToggleDecorators<GroundLabelDecorator>(!actor.IsOnScreen); // do not display ground labels when the actor is on the screen
                HealthGlobeDecorator.Paint(layer, actor, actor.FloorCoordinate, "health globe");
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip || Hud.Game.Me.HeroClassDefinition.HeroClass != HeroClass.Barbarian) return;

            foreach (var player in Hud.Game.Players.Where(p => p.SnoArea.Sno == Hud.Game.Me.SnoArea.Sno))
                DrawPlayerWarnings(player);

            if (missingIP)
                warningStr += "\u2620";
            if (tooFar)
                warningStr += "\u2757";

            var textlayout = WarningFont.GetTextLayout(warningStr);
            WarningFont.DrawText(textlayout, HudWidth * 0.96f, HudHeight * 0.26f);
            missingIP = false;
            tooFar = false;
            warningStr = "";
        }

        private void DrawPlayerWarnings(IPlayer player)
        {
            var warning = "";
            var portraitRect = player.PortraitUiElement.Rectangle;
            var yPos = portraitRect.Y + YOffset * HudHeight;
            var xPos = portraitRect.X + XOffset * HudWidth;

            if (!player.Powers.BuffIsActive(79528, 0) && !player.Powers.BuffIsActive(79528, 1)) //no ip
            {
                missingIP = true;
                warning += "\u2620";
            }
            if (player.NormalizedXyDistanceToMe > 50f) //to far away
            {
                tooFar = true;
                warning += "\u2757";
            }

            var textlayout = WarningFont.GetTextLayout(warning);
            WarningFont.DrawText(textlayout, xPos, yPos);
        }
    }
}
