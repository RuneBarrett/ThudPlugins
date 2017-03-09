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
                //plugin.Debug = true; // Shows lines with animations. Go to new tristram.

                //--------------- GENERAL SETTINGS ------------------
                plugin.ShowGizmos = true;
                plugin.ShowText = true;
                plugin.ShowMonsterLines = true;
                plugin.HitRange = 55;
                plugin.CloseEnoughRange = 15;
                plugin.ShowText = true;
                plugin.StrokeWidth = 3;

                //--------------- ADD NEW MONSTER TYPE ------------------
                //plugin.MonsterBrushes.Add(ActorRarity.RareMinion, Hud.Render.CreateBrush(100, 200, 0, 150, 0));

                //--------------- ADD NEW GIZMO TYPE ------------------
                plugin.GizmoBrushes.Add(GizmoType.PoolOfReflection, new GizmoLine(GizmoLine.AnimType.Fade, Hud.Render.CreateBrush(100, 250, 255, 0, 0)));

                //--------------- EDIT MONSTER/GIZMO BRUSH ------------------
                //plugin.MonsterBrushes[ActorRarity.Rare] = Hud.Render.CreateBrush(100, 255, 128, 0, 0);

                //--------------- REMOVE DEFAULT MONSTER TYPE------------------
                //plugin.MonsterBrushes.Remove(ActorRarity.Unique);

            });
        }
    }
}

