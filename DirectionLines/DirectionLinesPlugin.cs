using System;
using System.Linq;
using Turbo.Plugins.Default;
using System.Collections.Generic;

namespace Turbo.Plugins.RuneB
{
    public class DirectionLinesPlugin : BasePlugin, IInGameTopPainter
    {
        public int TextDistanceAway { get; set; }
        public IFont TextFont { get; set; }
        public IBrush GreyBrush { get; set; }

        public float StrokeWidth { get; set; }
        public float HitRange { get; set; }
        public float CloseEnoughRange { get; set; }
        public bool Debug { get; set; }
        public bool ShowText { get; set; }
        public bool ShowMonsterLines { get; set; }
        public bool ShowGizmos { get; set; }
        public Dictionary<ActorRarity, IBrush> MonsterBrushes { get; set; }
        public Dictionary<GizmoType, GizmoLine> GizmoBrushes { get; set; }

        private IScreenCoordinate center { get { return Hud.Game.Me.ScreenCoordinate; } }

        private float _opacityMod = 0.02f;
        private float _opacity = 0.01f;

        private float _lineWidthMod = 0.03f;
        private float _lineWidth = 0.01f;
        private bool _started;

        public DirectionLinesPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ShowGizmos = true;
            ShowText = true;
            ShowMonsterLines = true;
            HitRange = 55;
            CloseEnoughRange = 15;
            TextDistanceAway = 160;
            StrokeWidth = 3;


            TextFont = Hud.Render.CreateFont("tahoma", 8, 120, 255, 255, 255, true, false, true);
            GreyBrush = Hud.Render.CreateBrush(100, 80, 80, 80, 0);

            MonsterBrushes = new Dictionary<ActorRarity, IBrush>();
            MonsterBrushes.Add(ActorRarity.Rare, Hud.Render.CreateBrush(100, 255, 128, 0, 0));
            MonsterBrushes.Add(ActorRarity.Champion, Hud.Render.CreateBrush(100, 0, 128, 255, 0));
            MonsterBrushes.Add(ActorRarity.Boss, Hud.Render.CreateBrush(100, 255, 208, 0, 0));
            MonsterBrushes.Add(ActorRarity.Unique, Hud.Render.CreateBrush(100, 200, 0, 150, 0));

            GizmoBrushes = new Dictionary<GizmoType, GizmoLine>();


        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            if (Debug && !_started)
            {
                _started = true;   
                GizmoBrushes.Add(GizmoType.Portal, new GizmoLine(GizmoLine.AnimType.Fade, Hud.Render.CreateBrush(100, 250, 255, 0, 0)));
                GizmoBrushes.Add(GizmoType.IdentifyAll, new GizmoLine(GizmoLine.AnimType.WidthMod, Hud.Render.CreateBrush(100, 0, 0, 255, 0)));
                GizmoBrushes.Add(GizmoType.SharedStash, new GizmoLine(GizmoLine.AnimType.Blink, Hud.Render.CreateBrush(100, 0, 255, 0, 0)));
            }

            //Monster lines
            if (ShowMonsterLines)
            {
                var textDistanceAway = TextDistanceAway;
                var monsters = Hud.Game.AliveMonsters.Where(monster => MonsterBrushes.ContainsKey(monster.Rarity) && monster.NormalizedXyDistanceToMe > CloseEnoughRange);
                foreach (var monster in monsters)
                {
                    //Find coordinates
                    var monsterScreenCoordinate = monster.FloorCoordinate.ToScreenCoordinate();
                    var start = PointOnLine(center.X, center.Y, monsterScreenCoordinate.X, monsterScreenCoordinate.Y, 60);
                    var end = PointOnLine(monsterScreenCoordinate.X, monsterScreenCoordinate.Y - 30, center.X, center.Y, 20);

                    //Draw line to monster
                    if (monster.NormalizedXyDistanceToMe < HitRange) { MonsterBrushes[monster.Rarity].DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth); }
                    else GreyBrush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.5f);

                    if (ShowText) //Draw text
                    {
                        var layout = TextFont.GetTextLayout(string.Format("{0:N0}", monster.NormalizedXyDistanceToMe));
                        var p = PointOnLine(center.X, center.Y, monsterScreenCoordinate.X, monsterScreenCoordinate.Y, textDistanceAway);
                        TextFont.DrawText(layout, p.X, p.Y);
                        textDistanceAway += 30; // avoid text overlap
                    }
                }
            }

            //Gizmo lines
            if (ShowGizmos)
            {
                var Gizmos = Hud.Game.Actors.Where(actor => GizmoBrushes.ContainsKey(actor.GizmoType));
                foreach (var gizmo in Gizmos)
                {
                    var gizmoPos = gizmo.FloorCoordinate.ToScreenCoordinate();
                    var start = PointOnLine(center.X, center.Y, gizmoPos.X, gizmoPos.Y, 60);
                    var end = PointOnLine(gizmoPos.X, gizmoPos.Y - 30, center.X, center.Y, 20);

                    var gizmoLine = GizmoBrushes[gizmo.GizmoType];
                    switch (gizmoLine.anim)
                    {
                        case GizmoLine.AnimType.None:
                            gizmoLine.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                            break;
                        case GizmoLine.AnimType.Blink:
                            //TextFont.DrawText(""+(Hud.Game.CurrentRealTimeMilliseconds/1000) % 2, end);
                            if ((Hud.Game.CurrentRealTimeMilliseconds / 700) % 2 == 0)
                                gizmoLine.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                            break;
                        case GizmoLine.AnimType.Fade:
                            if (_opacity < 0 || _opacity > 1) { _opacityMod = -_opacityMod; }
                            _opacity += _opacityMod;
                            gizmoLine.brush.Opacity = _opacity;
                            gizmoLine.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                            break;
                        case GizmoLine.AnimType.WidthMod:
                            if (_lineWidth < 0 || _lineWidth > 1.5f) { _lineWidthMod = -_lineWidthMod; }
                            _lineWidth += _lineWidthMod;
                            gizmoLine.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * _lineWidth);
                            break;
                    }
                }
            }
        }

        public IScreenCoordinate PointOnLine(float x1, float y1, float x2, float y2, float offset)
        {
            //Returns a coordinate at offset distance away from x1,y1 towards x2,y2
            var distance = (float)Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            var ratio = offset / distance;

            var x3 = ratio * x2 + (1 - ratio) * x1;
            var y3 = ratio * y2 + (1 - ratio) * y1;
            return Hud.Window.CreateScreenCoordinate(x3, y3);
        }
    }

    public class GizmoLine
    {
        public enum AnimType { Blink, Fade, WidthMod, None }
        public AnimType anim { get; set; }
        public IBrush brush { get; set; }
        private AnimType blink { get; set; }

        public GizmoLine(AnimType anim, IBrush brush)
        {
            this.anim = anim;
            this.brush = brush;
        }

    }
}