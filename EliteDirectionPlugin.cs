using System;
using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{
    public class EliteDirectionPlugin : BasePlugin, IInGameWorldPainter
    {
        public IFont TextFont { get; set; }
        public IBrush GreyBrush { get; set; }
        public IBrush RareBrush { get; set; }
        public IBrush ChampionBrush { get; set; }
        public float HitRange { get; set; }
        public float StrokeWidth { get; set; }
        public float TooCloseRange { get; set; }
        public bool ShowText { get; set; }

        private float hudWidth { get { return Hud.Window.Size.Width; } }
        private float hudHeight { get { return Hud.Window.Size.Height; } }
        private Point center { get { return new Point(Hud.Window.Size.Width / 2, Hud.Window.Size.Height / 2 - 100); } }

        public EliteDirectionPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            HitRange = 55;
            TooCloseRange = 15;
            ShowText = true;
            StrokeWidth = 3;
            TextFont = Hud.Render.CreateFont("tahoma", 8, 120, 255, 255, 255, true, false, true);
            GreyBrush = Hud.Render.CreateBrush(100, 80, 80, 80, 0);
            RareBrush = Hud.Render.CreateBrush(100, 255, 128, 0, 0);
            ChampionBrush = Hud.Render.CreateBrush(100, 0, 128, 255, 0);
        }

        public void PaintWorld(WorldLayer layer)
        {
            var monsters = Hud.Game.AliveMonsters;
            float x, y;
            double mobDistance;
            int textDistanceAway = 150;

            foreach (var monster in monsters)
            {
                if (monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare)
                {
                    mobDistance = monster.NormalizedXyDistanceToMe;
                    if (mobDistance < TooCloseRange) continue;

                    x = monster.ScreenCoordinate.X;
                    y = monster.ScreenCoordinate.Y;

                    if (ShowText)
                    {
                        var layout = TextFont.GetTextLayout(string.Format("{0:N0}", mobDistance));
                        Point p = PointOnLine(center.x, center.y, x, y, textDistanceAway);
                        TextFont.DrawText(layout, p.x, p.y);
                        textDistanceAway += 30;
                    }

                    if (monster.Rarity == ActorRarity.Rare)                   
                        DrawLineToMonster(RareBrush, x, y, mobDistance);
                    
                    if (monster.Rarity == ActorRarity.Champion)                    
                        DrawLineToMonster(ChampionBrush, x, y, mobDistance);
                    
                }
            }
        }

        private void DrawLineToMonster(IBrush brush, float x, float y, double distance)
        {
            Point start = PointOnLine(center.x, center.y, x, y, 60);
            Point end = PointOnLine(x, y, center.x, center.y, 40);
            if (distance < HitRange) { brush.DrawLine(start.x, start.y, end.x, end.y, StrokeWidth); }
            else GreyBrush.DrawLine(start.x, start.y, end.x, end.y, StrokeWidth-StrokeWidth/2);
        }

        public Point MidPoint(float x1, float y1, float x2, float y2)
        {
            return new Point((x1 + x2) / 2, (y1 + y2) / 2);
        }

        public Point PointOnLine(float x1, float y1, float x2, float y2, float offset)
        {
            float distance = (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2)); //distance 
            float ratio = offset / distance; //segment ratio

            float x3 = ratio * x2 + (1 - ratio) * x1; // find point that divides the segment
            float y3 = ratio * y2 + (1 - ratio) * y1; // into the ratio (1-r):r
            return new Point(x3, y3);
        }

    }

    public class Point
    {
        public float x { get; set; }
        public float y { get; set; }

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}