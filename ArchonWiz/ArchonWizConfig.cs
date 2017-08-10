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
                //General settings
                //plugin.ShowWarnings = true; // Disable if using jack's alertlistplugin
                //plugin.ShowInTown = true;
                //plugin.ShowZeiCircle = true;
                //plugin.ShowRashaElements = true;
                //plugin.ShowArchonCD = true;
                //plugin.ShowArchonRemain = true;
                //plugin.AlwaysShowElements = false; 
                //plugin.WarningYPosIncr = 0.022f; // Distance between warnings

                //The following values (and the PositionOffset in PlayerBottomBuffListPlugin below) puts everything in the middle of the screen above the character.
                //plugin.WarningYPos = 0.35f;
                //plugin.ArchonCDandRemainYPos = 0.480f; // Just below tal rasha icons = 0.605f;
                //plugin.RashaIndicatorsYpos = 0.415f;
            });

            Hud.RunOnPlugin<Default.PlayerBottomBuffListPlugin>(plugin =>
            {
                //plugin.PositionOffset = -0.05f; //On top of the character
            });
        }
    }
}

