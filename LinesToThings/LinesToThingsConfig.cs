using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{

    public class LinesToThingsConfig : BasePlugin, ICustomizer
    {

        public LinesToThingsConfig()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {

            Hud.RunOnPlugin<RuneB.LinesToThingsPlugin>(plugin =>
            {
                //Add a new monster + brush
                //plugin.MonsterBrushes.Add(ActorRarity.Normal, Hud.Render.CreateBrush(100, 200, 0, 150, 0));

                //Edit existing monsters brush.
                //plugin.MonsterBrushes[ActorRarity.Rare] = Hud.Render.CreateBrush(100, 255, 128, 0, 0);

                //Remove monster
                //plugin.MonsterBrushes.Remove(ActorRarity.Unique);

                plugin.HitRange = 55;
                plugin.CloseEnoughRange = 15;
                plugin.ShowText = true;
                plugin.StrokeWidth = 3;
            });
        }
    }
}

