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
        public bool MonsterDistanceTextEnabled { get; set; }
        public bool MonsterLinesEnabled { get; set; }
        public bool GizmoLinesEnabled { get; set; }
        public bool AnimationsEnabled { get; set; }
        public Dictionary<ActorRarity, Line> MonsterBrushes { get; set; }
        public Dictionary<GizmoType, Line> GizmoBrushes { get; set; }

        private IScreenCoordinate center { get { return Hud.Game.Me.ScreenCoordinate; } }

        private float _opacityMod = 0.04f;
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
            TextFont = Hud.Render.CreateFont("tahoma", 8, 120, 255, 255, 255, true, false, true);
            GreyBrush = Hud.Render.CreateBrush(100, 80, 80, 80, 0);

            GizmoLinesEnabled = true;
            MonsterDistanceTextEnabled = true;
            MonsterLinesEnabled = true;
            AnimationsEnabled = true;
            HitRange = 55;
            CloseEnoughRange = 15;
            TextDistanceAway = 160;
            StrokeWidth = 3;

            MonsterBrushes = new Dictionary<ActorRarity, Line>();
            MonsterBrushes.Add(ActorRarity.Rare, new Line(Line.AnimType.Fade, Hud.Render.CreateBrush(100, 255, 128, 0, 0)));
            MonsterBrushes.Add(ActorRarity.Champion, new Line(Line.AnimType.WidthMod, Hud.Render.CreateBrush(100, 0, 128, 255, 0)));
            MonsterBrushes.Add(ActorRarity.Boss, new Line(Line.AnimType.None, Hud.Render.CreateBrush(100, 255, 208, 0, 0)));

            GizmoBrushes = new Dictionary<GizmoType, Line>();
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            if (Debug && !_started)             
                DebugAnimations();
            
            if (AnimationsEnabled)
                AnimationUpdate();

            //Monster lines
            if (MonsterLinesEnabled)
            {
                var textDistanceAway = TextDistanceAway;
                var monsters = Hud.Game.AliveMonsters.Where(monster => MonsterBrushes.ContainsKey(monster.Rarity) && monster.NormalizedXyDistanceToMe > CloseEnoughRange);
                foreach (var monster in monsters)
                {
                    var monsterScreenCoordinate = monster.FloorCoordinate.ToScreenCoordinate();
                    if (monster.NormalizedXyDistanceToMe < HitRange) { DrawLine(monsterScreenCoordinate, MonsterBrushes[monster.Rarity], false); }
                    else DrawLine(monsterScreenCoordinate, MonsterBrushes[monster.Rarity], true);

                    if (MonsterDistanceTextEnabled) //Draw text
                    {
                        var layout = TextFont.GetTextLayout(string.Format("{0:N0}", monster.NormalizedXyDistanceToMe));
                        var p = PointOnLine(center.X, center.Y, monsterScreenCoordinate.X, monsterScreenCoordinate.Y, textDistanceAway);
                        TextFont.DrawText(layout, p.X, p.Y);
                        textDistanceAway += 30; // avoid text overlap
                    }
                }
            }

            //Gizmo lines
            if ((GizmoLinesEnabled && !Hud.Game.IsInTown) || Debug)
            {
                var Gizmos = Hud.Game.Actors.Where(actor => GizmoBrushes.ContainsKey(actor.GizmoType));
                foreach (var gizmo in Gizmos)
                {
                    if (gizmo.IsOperated) continue;
                    var gizmoPos = gizmo.FloorCoordinate.ToScreenCoordinate();
                    var gizmoLine = GizmoBrushes[gizmo.GizmoType];
                    DrawLine(gizmoPos, gizmoLine, false);
                }
            }
        }

        private void DebugAnimations()
        {
            _started = true;
            GizmoBrushes.Add(GizmoType.Portal, new Line(Line.AnimType.Fade, Hud.Render.CreateBrush(100, 250, 255, 0, 0)));
            GizmoBrushes.Add(GizmoType.IdentifyAll, new Line(Line.AnimType.WidthMod, Hud.Render.CreateBrush(100, 0, 0, 255, 0)));
            GizmoBrushes.Add(GizmoType.SharedStash, new Line(Line.AnimType.Blink, Hud.Render.CreateBrush(100, 0, 255, 0, 0)));
        }

        private void AnimationUpdate()
        {
            if (_opacity < 0 || _opacity > 1) { _opacityMod = -_opacityMod; }
            _opacity += _opacityMod;
            if (_lineWidth < 0 || _lineWidth > 1f) { _lineWidthMod = -_lineWidthMod; }
            _lineWidth += _lineWidthMod;
        }

        private void DrawLine(IScreenCoordinate objectPosition, Line line, bool grey)
        {
            var start = PointOnLine(center.X, center.Y, objectPosition.X, objectPosition.Y, 60);
            var end = PointOnLine(objectPosition.X, objectPosition.Y - 30, center.X, center.Y, 20);
            if (grey)
            {
                GreyBrush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.5f);
                return;
            }

            switch (line.anim)
            {
                case Line.AnimType.None:
                    line.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                    break;
                case Line.AnimType.Blink:
                    if ((Hud.Game.CurrentRealTimeMilliseconds / 700) % 2 == 0)
                        line.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.6f);
                    break;
                case Line.AnimType.Fade:
                    line.brush.Opacity = _opacity;
                    line.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                    break;
                case Line.AnimType.WidthMod:
                    line.brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * _lineWidth);
                    break;
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

    public class Line
    {
        public enum AnimType { Blink, Fade, WidthMod, None }
        public AnimType anim { get; set; }
        public IBrush brush { get; set; }
        private AnimType blink { get; set; }

        public Line(AnimType anim, IBrush brush)
        {
            this.anim = anim;
            this.brush = brush;
        }
    }
}