using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{

    public class ArchonWizConfig : BasePlugin, ICustomizer
    {

        public ArchonWizConfig()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.RunOnPlugin<RuneB.ArchonWizPlugin>(plugin =>
            {
                plugin.ShowWarnings = true;
                plugin.ShowInTown = true;
                plugin.ShowZeiCircle = true;
                plugin.ShowRashaElements = true;
                plugin.ShowArchonCD = true;
                plugin.ShowArchonRemain = true;
                plugin.AlwaysShowElements = false;
                plugin.WarningYPos = 0.27f;
                plugin.ArchonCDandRemainYPos = 0.5f; // Just below tal rasha icons = 0.605f;
                plugin.WarningYPosIncr = 0.022f; // Distance between warnings

            });
        }
    }
}
