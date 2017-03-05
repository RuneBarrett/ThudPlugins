using System;
using System.Linq;
using Turbo.Plugins.Default;
using System.Collections.Generic;

namespace Turbo.Plugins.RuneB
{
    public class EliteDirectionPlugin : BasePlugin, IInGameTopPainter
    {
        public IFont TextFont { get; set; }
        public IBrush GreyBrush { get; set; }
        public IBrush RareBrush { get; set; }
        public IBrush ChampionBrush { get; set; }
        public IBrush BossBrush { get; set; }
        public IBrush UniqueBrush { get; set; }
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
            UniqueBrush = Hud.Render.CreateBrush(100, 200, 0, 150, 0);
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            IEnumerable<IMonster> monsters = Hud.Game.AliveMonsters.Where(monster => monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare || monster.Rarity == ActorRarity.Boss || monster.Rarity == ActorRarity.Unique);
            int textDistanceAway = 180;

            foreach (IMonster monster in monsters)
            {
                double mobDistance = monster.NormalizedXyDistanceToMe;
                if (mobDistance < CloseEnoughRange) continue;

                float x = monster.ScreenCoordinate.X;
                float y = monster.ScreenCoordinate.Y;

                IBrush brush;
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
                    case ActorRarity.Unique:
                        brush = UniqueBrush;
                        break;

                    default:
                        continue;
                }

                //Draw line to monster
                IScreenCoordinate start = PointOnLine(center.X, center.Y, x, y, 60);
                IScreenCoordinate end = PointOnLine(x, y, center.X, center.Y, 80);
                if (mobDistance < HitRange && brush != null) { brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth); }
                else GreyBrush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.5f);

                if (ShowText)
                {// Draw text
                    var layout = TextFont.GetTextLayout(string.Format("{0:N0}", mobDistance));
                    IScreenCoordinate p = PointOnLine(center.X, center.Y, x, y, textDistanceAway);
                    TextFont.DrawText(layout, p.X, p.Y);
                    textDistanceAway += 30; // avoid text overlap
                }
            }
        }

        public IScreenCoordinate PointOnLine(float x1, float y1, float x2, float y2, float offset)
        {
            float distance = (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            float ratio = offset / distance;

            float x3 = ratio * x2 + (1 - ratio) * x1;
            float y3 = ratio * y2 + (1 - ratio) * y1;
            return Hud.Window.CreateScreenCoordinate(x3, y3);
        }
    }
}