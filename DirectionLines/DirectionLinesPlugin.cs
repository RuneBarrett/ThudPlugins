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

        private IScreenCoordinate Center { get { return Hud.Game.Me.ScreenCoordinate; } }

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

            MonsterBrushes = new Dictionary<ActorRarity, Line>
            {
                {ActorRarity.Rare, new Line(Line.AnimType.Fade, Hud.Render.CreateBrush(100, 255, 128, 0, 0))},
                {ActorRarity.Champion, new Line(Line.AnimType.WidthMod, Hud.Render.CreateBrush(100, 0, 128, 255, 0))},
                {ActorRarity.Boss, new Line(Line.AnimType.None, Hud.Render.CreateBrush(100, 255, 208, 0, 0))}
            };

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
                var monsters = Hud.Game.AliveMonsters.Where(monster => MonsterBrushes.ContainsKey(monster.Rarity) && monster.NormalizedXyDistanceToMe > CloseEnoughRange && monster.SummonerAcdDynamicId == 0);
                foreach (var monster in monsters)
                {
                    var monsterScreenCoordinate = monster.FloorCoordinate.ToScreenCoordinate();
                    DrawLine(monsterScreenCoordinate, MonsterBrushes[monster.Rarity], !(monster.NormalizedXyDistanceToMe < HitRange));

                    if (!MonsterDistanceTextEnabled) continue;
                    var layout = TextFont.GetTextLayout(string.Format("{0:N0}", monster.NormalizedXyDistanceToMe));
                    var p = PointOnLine(Center.X, Center.Y, monsterScreenCoordinate.X, monsterScreenCoordinate.Y, textDistanceAway);
                    TextFont.DrawText(layout, p.X, p.Y);
                    textDistanceAway += 30; // avoid text overlap
                }
            }

            //Gizmo lines
            if ((!GizmoLinesEnabled || Hud.Game.IsInTown) && !Debug) return;
            var gizmos = Hud.Game.Actors.Where(actor => GizmoBrushes.ContainsKey(actor.GizmoType));
            foreach (var gizmo in gizmos)
            {
                if (gizmo.IsOperated) continue;
                var gizmoPos = gizmo.FloorCoordinate.ToScreenCoordinate();
                var gizmoLine = GizmoBrushes[gizmo.GizmoType];
                DrawLine(gizmoPos, gizmoLine, false);
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
            var start = PointOnLine(Center.X, Center.Y, objectPosition.X, objectPosition.Y, 60);
            var end = PointOnLine(objectPosition.X, objectPosition.Y - 30, Center.X, Center.Y, 20);
            if (grey)
            {
                GreyBrush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.5f);
                return;
            }

            switch (line.Anim)
            {
                case Line.AnimType.None:
                    line.Brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                    break;
                case Line.AnimType.Blink:
                    if ((Hud.Game.CurrentRealTimeMilliseconds / 700) % 2 == 0)
                        line.Brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.6f);
                    break;
                case Line.AnimType.Fade:
                    line.Brush.Opacity = _opacity;
                    line.Brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * 0.8f);
                    break;
                case Line.AnimType.WidthMod:
                    line.Brush.DrawLine(start.X, start.Y, end.X, end.Y, StrokeWidth * _lineWidth);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
        public AnimType Anim { get; set; }
        public IBrush Brush { get; set; }

        public Line(AnimType anim, IBrush brush)
        {
            this.Anim = anim;
            this.Brush = brush;
        }
    }
}