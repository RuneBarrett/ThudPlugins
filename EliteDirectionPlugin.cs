using System;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{
    public class EliteDirectionPlugin : BasePlugin, IInGameTopPainter
    {
        public IFont TextFont { get; set; }
        public IBrush GreyBrush { get; set; }
        public IBrush RareBrush { get; set; }
        public IBrush ChampionBrush { get; set; }
        public IBrush BossBrush { get; set; }
        public float StrokeWidth { get; set; }
        public float HitRange { get; set; }
        public float CloseEnoughRange { get; set; }
        public bool ShowText { get; set; }

        private IScreenCoordinate center { get { return Hud.Game.Me.ScreenCoordinate; } }

        public EliteDirectionPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            HitRange = 55;
            CloseEnoughRange = 15;
            ShowText = true;
            StrokeWidth = 3;
            TextFont = Hud.Render.CreateFont("tahoma", 8, 120, 255, 255, 255, true, false, true);
            GreyBrush = Hud.Render.CreateBrush(100, 80, 80, 80, 0);
            RareBrush = Hud.Render.CreateBrush(100, 255, 128, 0, 0);
            ChampionBrush = Hud.Render.CreateBrush(100, 0, 128, 255, 0);
            BossBrush = Hud.Render.CreateBrush(100, 255, 208, 0, 0);
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            var monsters = Hud.Game.AliveMonsters.Where(monster => monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare || monster.Rarity == ActorRarity.Boss);
            var textDistanceAway = 150;

            foreach (var monster in monsters)
            {
                var mobDistance = monster.NormalizedXyDistanceToMe;
                if (mobDistance < CloseEnoughRange) continue;

                var x = monster.ScreenCoordinate.X;
                var y = monster.ScreenCoordinate.Y;

                var brush = GreyBrush;
                if (mobDistance <= HitRange)
                {
                    // TODO : replace this with a Dictionary
                    switch (monster.Rarity)
                    {
                        case ActorRarity.Champion:
                            brush = ChampionBrush;
                            break;
                        case ActorRarity.Rare:
                            brush = RareBrush;
                            break;
                        case ActorRarity.Boss:
                            brush = BossBrush;
                            break;
                        //case ActorRarity.Normal:
                        //case ActorRarity.RareMinion:
                        //case ActorRarity.Hireling:
                        //case ActorRarity.Unique:
                        default:
                            continue;
                    }
                }

                brush.DrawLine(center.X, center.Y, x, y, StrokeWidth);

                if (!ShowText)
                    continue;

                var layout = TextFont.GetTextLayout(string.Format("{0:N0}", mobDistance));
                var p = PointOnLine(center.X, center.Y, x, y, textDistanceAway);
                TextFont.DrawText(layout, p.X, p.Y);

                // trying to avoid label overlap
                textDistanceAway += 30;
            }
        }

        public IScreenCoordinate PointOnLine(float x1, float y1, float x2, float y2, float offset)
        {
            var distance = (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            var ratio = offset / distance;

            var x3 = ratio * x2 + (1 - ratio) * x1;
            var y3 = ratio * y2 + (1 - ratio) * y1;
            return Hud.Window.CreateScreenCoordinate(x3, y3);
        }
    }
}