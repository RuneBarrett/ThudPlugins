using System;
using System.Linq;
using SharpDX.DirectInput;
using SharpDX;
using Turbo.Plugins.Default;
using System.Collections.Generic;

namespace Turbo.Plugins.RuneB
{
    public class PartyCooldownsPlugin : BasePlugin, IInGameTopPainter
    {
        public SkillPainter SkillPainter { get; set; }
        public TopLabelDecorator Label { get; set; }
        public IFont ClassFont { get; set; }
        public List<uint> WatchedSnos;

        public float SizeRatio { get; set; }
        public float StartXPos { get; set; }
        public float StartYPos { get; set; }
        public float IconSize { get; set; }
        public bool ShowSelf { get; set; }
        public bool ShowInTown { get; set; }
        public bool OnlyInGR { get; set; }

        private float size = 0;
        private float hudWidth { get { return Hud.Window.Size.Width; } }
        private float hudHeight { get { return Hud.Window.Size.Height; } }
        private Dictionary<HeroClass, string> classShorts;
        private int[] skillOrder = new int[] { 2, 3, 4, 5, 0, 1 };


        public PartyCooldownsPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            ShowSelf = true;
            ShowInTown = false;
            OnlyInGR = false;
            base.Load(hud);
            SizeRatio = 0.02f;
            StartYPos = 0.002f;
            StartXPos = 0.555f;
            IconSize = 0.05f;


            WatchedSnos = new List<uint>();
            //Add skills to the watch list below
            //Necromancer
            WatchedSnos.Add(465350); //Simulacrum  
            WatchedSnos.Add(465839); //Land of the Dead

            //Barb
            WatchedSnos.Add(79528); //Ignore Pain
            //WatchedSnos.Add(79607); //Wrath of the Berserker
            //WatchedSnos.Add(375483); //Warcry

            //Monk
            //WatchedSnos.Add(317076); //Inner Sanctuary

            //Witch Doctor
            //WatchedSnos.Add(106237); //Spirit Walk

            //Demon Hunter 
            //WatchedSnos.Add(365311); //Companion

            //Wizard
            //WatchedSnos.Add(134872); //Archon - PROBABLY BUGGING IN GREATER RIFT GROUPS (looking in to it)
            //---------------------- 

            ClassFont = Hud.Render.CreateFont("tahoma", 7, 230, 255, 255, 255, true, false, 255, 0, 0, 0, true);

            classShorts = new Dictionary<HeroClass, string>();
            classShorts.Add(HeroClass.Barbarian, "Barb");
            classShorts.Add(HeroClass.Monk, "Monk");
            classShorts.Add(HeroClass.Necromancer, "Necro");
            classShorts.Add(HeroClass.Wizard, "Wiz");
            classShorts.Add(HeroClass.WitchDoctor, "WD");
            classShorts.Add(HeroClass.Crusader, "Sader");
            classShorts.Add(HeroClass.DemonHunter, "DH");

            SkillPainter = new SkillPainter(Hud, true)
            {
                TextureOpacity = 1.0f,
                EnableSkillDpsBar = true,
                EnableDetailedDpsHint = true,
                CooldownFont = Hud.Render.CreateFont("arial", 7, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true),
                SkillDpsBackgroundBrushesByElementalType = new IBrush[]
                {
                    Hud.Render.CreateBrush(222, 255, 6, 0, 0),
                    Hud.Render.CreateBrush(222, 183, 57, 7, 0),
                    Hud.Render.CreateBrush(222, 0, 38, 119, 0),
                    Hud.Render.CreateBrush(222, 77, 102, 155, 0),
                    Hud.Render.CreateBrush(222, 50, 106, 21, 0),
                    Hud.Render.CreateBrush(222, 138, 0, 94, 0),
                    Hud.Render.CreateBrush(222, 190, 117, 0, 0),
                },
                SkillDpsFont = Hud.Render.CreateFont("tahoma", 7, 222, 255, 255, 255, false, false, 0, 0, 0, 0, false),
            };
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip || !ShowInTown && Hud.Game.Me.IsInTown || OnlyInGR && Hud.Game.SpecialArea != SpecialArea.GreaterRift) return;
            if (size <= 0)
                size = hudWidth * SizeRatio;

            float xPos = hudWidth * StartXPos;

            foreach (IPlayer player in Hud.Game.Players)
            {
                if (player.IsMe && !ShowSelf)
                    continue;
                bool found = false; 
                bool firstIter = true;
                foreach (int i in skillOrder)
                {

                    var skill = player.Powers.SkillSlots[i];
                    if (skill != null && WatchedSnos.Contains(skill.SnoPower.Sno))
                    {
                        found = true;
                        if (firstIter)
                        {
                            var layout = ClassFont.GetTextLayout(player.BattleTagAbovePortrait + "\n(" + classShorts[player.HeroClassDefinition.HeroClass] + ")"); 
                            ClassFont.DrawText(layout, xPos - (layout.Metrics.Width * 0.1f), hudHeight * StartYPos);
                            firstIter = false;
                        }

                        var rect = new RectangleF(xPos, hudHeight * (StartYPos + 0.03f), size, size);
                        SkillPainter.Paint(skill, rect);
                        xPos += size * 1.1f;
                    }
                }
                if (found)
                    xPos += size * 1.1f;
            }
        }
    }
}