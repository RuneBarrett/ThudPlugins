using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{
    public class TemplatePlugin : BasePlugin//, IInGameWorldPainter, IInGameTopPainter, ICustomizer
    {
        public TemplatePlugin()
        {
            Enabled = true;
        }

        /*public void Customize()
        {
            //Hud.RunOnPlugin<SOMEPLUGIN>(plugin => { });
        }*/

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        /*public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
        }*/

        /*public void PaintWorld(WorldLayer layer) {

        }*/
    }
}