using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{

    public class DirectionLinesConfig : BasePlugin, ICustomizer
    {

        public DirectionLinesConfig()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {

            Hud.RunOnPlugin<RuneB.DirectionLinesPlugin>(plugin =>
            {
                //plugin.Debug = true; // Shows lines with animations. Go to new tristram for example.

                //--------------- GENERAL SETTINGS ------------------
                plugin.MonsterLinesEnabled = true;
                plugin.GizmoLinesEnabled = true;
                plugin.MonsterDistanceTextEnabled = true;
                plugin.AnimationsEnabled = true;
                plugin.HitRange = 55;
                plugin.CloseEnoughRange = 15;
                plugin.MonsterDistanceTextEnabled = true;
                plugin.StrokeWidth = 6;

                //--------------- ADD NEW MONSTER TYPE ------------------
                plugin.MonsterBrushes.Add(ActorRarity.Unique, new Line(Line.AnimType.WidthMod, Hud.Render.CreateBrush(100, 200, 0, 150, 0)));

                //--------------- ADD NEW GIZMO TYPE ------------------
                plugin.GizmoBrushes.Add(GizmoType.PoolOfReflection, new Line(Line.AnimType.Blink, Hud.Render.CreateBrush(20, 250, 255, 0, 0)));
                //plugin.GizmoBrushes.Add(GizmoType.Chest, new Line(Line.AnimType.Fade, Hud.Render.CreateBrush(100, 250, 250, 250, 0)));
                //plugin.GizmoBrushes.Add(GizmoType.HealingWell, new Line(Line.AnimType.WidthMod, Hud.Render.CreateBrush(100, 250, 0, 0, 0)));

                //--------------- EDIT MONSTER/GIZMO BRUSH OR ANIMATION ------------------
                plugin.MonsterBrushes[ActorRarity.Rare].brush = Hud.Render.CreateBrush(100, 255, 128, 0, 0);
                plugin.MonsterBrushes[ActorRarity.Rare].anim = RuneB.Line.AnimType.WidthMod;
                //plugin.MonsterBrushes[ActorRarity.Rare].anim = RuneB.Line.AnimType.None;

                //--------------- REMOVE DEFAULT MONSTER TYPE------------------
                //plugin.MonsterBrushes.Remove(ActorRarity.Boss);

            });
        }
    }
}
