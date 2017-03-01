using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{

    public class BuffLabelsConfig : BasePlugin, ICustomizer
    {

        public BuffLabelsConfig()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }

        public void Customize()
        {
            Hud.RunOnPlugin<RuneB.BuffLabelsPlugin>(plugin =>
            {
                plugin.Debug = true; // show all labels, and add them slowly over time.


                //POSITION, MARGIN & SIZE -------------
                //plugin.YPos = 0.5f; //Vertical position (0 == top, 1 == bottom)
                //plugin.XPos = 0.2f; //Horizontal position (0 == left, 1 == right)
                //plugin.YPosIncrement = 0.01f; // Distance between labels
                plugin.SizeModifier = 1.2f; // change size of labels and text.
                plugin.NumRows = 2; //The amount of rows allowed. (if 1, all labels will align horizontally. 2)
                //-------------

                /*How to add buffs:
                plugin.Labels.Add(new RuneB.Label(<Shown buff name>, <Sno>, <Icon count>, <A brush>));
                Find sno's in /doc/sno_powers.txt
                */

                //Examples:
                //Monk:
                plugin.Labels.Add(new RuneB.Label("Flying Dragon", 246562, 1, Hud.Render.CreateBrush(100, 50, 200, 255, 0)));

                //Wizard
                plugin.Labels.Add(new RuneB.Label("Archon", 134872, 2, Hud.Render.CreateBrush(100, 0, 80, 215, 0))); 
                plugin.Labels.Add(new RuneB.Label("Magic Weapon", 76108, 0, Hud.Render.CreateBrush(100, 0, 45, 130, 0))); 
                plugin.Labels.Add(new RuneB.Label("Energy Armor", 86991, 0, Hud.Render.CreateBrush(100, 140, 1, 170, 0)));

                //Barb
                plugin.Labels.Add(new RuneB.Label("War Cry", 375483, 0, Hud.Render.CreateBrush(100, 100, 50, 40, 0)));
                plugin.Labels.Add(new RuneB.Label("Berserker", 79607, 0, Hud.Render.CreateBrush(100, 45, 100, 55, 0))); //Wrath of the Beserker

                //Crusader
                plugin.Labels.Add(new RuneB.Label("Akarat's Champion", 269032, 1, Hud.Render.CreateBrush(100, 70, 50, 40, 0)));
                plugin.Labels.Add(new RuneB.Label("Iron Skin", 291804, 0, Hud.Render.CreateBrush(100, 90, 60, 70, 0)));

                plugin.Labels.Add(new RuneB.Label("Flying Dragon", 246562, 1, Hud.Render.CreateBrush(100, 50, 200, 255, 0)));

            });
        }

    }

}