using Turbo.Plugins.Default;
using System.Linq;

namespace Turbo.Plugins.RuneB
{

    public class BaseCustomizationPlugin : BasePlugin, ICustomizer
    {

        public BaseCustomizationPlugin()
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
                plugin.ShowRashaElements = true;
                plugin.ShowZeiCircle = true;
                plugin.ShowArchonCD = true;
                plugin.ShowArchonRemain = true;
            });

            Hud.RunOnPlugin<Jack.Alerts.PlayerTopAlertListPlugin>(plugin =>
            {
                var alerts = plugin.AlertList.Alerts.Where(a => a.TextSnoId == 76108/*MagicWeapon*/ || a.TextSnoId == 135663/*SlowTime*/ || a.TextSnoId == 86991/*EnergyArmor*/);
                foreach (var a in alerts) a.Enabled = false;                                
            });


            Hud.GetPlugin<InventoryAndStashPlugin>().NotGoodDisplayEnabled = false;
            Hud.GetPlugin<Jack.MouseLocatorPlugin>().Enabled = false;

        }

    }

}