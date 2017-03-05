using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{
    public class ArchonWizPlugin : BasePlugin, IInGameTopPainter, ICustomizer
    {
        public bool ShowWarnings { get; set; }
        public bool ShowInTown { get; set; }
        public bool ShowZeiCircle { get; set; }
        public bool ShowRashaElements { get; set; }
        public bool ShowArchonCD { get; set; }
        public bool ShowArchonRemain { get; set; }
        public bool AlwaysShowElements { get; set; }

        public GroundCircleDecorator ZeiRanceIndicator { get; set; }
        public TopLabelDecorator ArchonCDLabel { get; set; }
        public TopLabelDecorator ArchonCooldownLabel { get; set; }
        public TopLabelDecorator ArchonRemainingLabel { get; set; }

        public IFont WarningFont { get; set; }
        public IFont ArchonCDFont { get; set; }
        public IFont ArchonRemainFont { get; set; }
        public IFont ArchonRemainSoonFont { get; set; }

        public IBrush RashaBackgroundBrush { get; set; }
        public IBrush FireBrush { get; set; }
        public IBrush ArcaneBrush { get; set; }
        public IBrush LightningBrush { get; set; }
        public IBrush ColdBrush { get; set; }
        public IBrush GreyBrush { get; set; }

        private IPlayerSkill archonSkill;
        private IPlayerSkill magicWeaponSkill;
        private IPlayerSkill energyArmorSkill;

        private float hudWidth { get { return Hud.Window.Size.Width; } }
        private float hudHeight { get { return Hud.Window.Size.Height; } }

        private float _lWidth, _lHeight, _lRashaSize, _lRashaYpos, _lRashaSizeMod, _arcCDRemain, _tick;
        private bool _timerRunning = false;

        public ArchonWizPlugin()
        {
            Enabled = true;
        }

        public void Customize()
        {
            Hud.RunOnPlugin<PlayerBottomBuffListPlugin>(plugin =>
            {
                plugin.BuffPainter.ShowTimeLeftNumbers = true;
                plugin.RuleCalculator.Rules.Add(new BuffRule(134872) { IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = true, IconSizeMultiplier = 1.3f }); // Archon 
                plugin.RuleCalculator.Rules.Add(new BuffRule(429855) { IconIndex = 5, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, IconSizeMultiplier = 1.3f }); // Tal Rasha
            });
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);

            // Public vars
            ShowWarnings = true;
            ShowInTown = true;
            ShowZeiCircle = true;
            ShowRashaElements = true;
            ShowArchonCD = true;
            ShowArchonRemain = true;
            AlwaysShowElements = false;

            WarningFont = Hud.Render.CreateFont("tahoma", 10f, 200, 255, 0, 0, false, false, true);
            ArchonCDFont = Hud.Render.CreateFont("tahoma", 10f, 255, 140, 140, 180, false, false, true);
            ArchonRemainFont = Hud.Render.CreateFont("tahoma", 10f, 255, 80, 140, 210, false, false, true);
            ArchonRemainSoonFont = Hud.Render.CreateFont("tahoma", 14.5f, 255, 255, 0, 0, false, false, true);

            RashaBackgroundBrush = Hud.Render.CreateBrush(100, 30, 30, 30, 0);
            GreyBrush = Hud.Render.CreateBrush(255, 50, 50, 50, 0);
            FireBrush = Hud.Render.CreateBrush(255, 200, 130, 30, 0);
            ArcaneBrush = Hud.Render.CreateBrush(255, 180, 80, 180, 0);
            LightningBrush = Hud.Render.CreateBrush(255, 0, 65, 145, 0);
            ColdBrush = Hud.Render.CreateBrush(255, 80, 130, 180, 0);
            ZeiRanceIndicator = new GroundCircleDecorator(Hud)
            {
                Brush = Hud.Render.CreateBrush(50, 14, 200, 245, 1.5f),
                Radius = 50f
            };
            ArchonCooldownLabel = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 10f, 255, 140, 140, 180, false, false, 160, 0, 0, 0, true),
                TextFunc = () => ArchonCooldown(),
            };

            // Private vars
            _lWidth = 80;
            _lHeight = 15;
            _lRashaSize = 24f;
            _lRashaYpos = 0.585f;
            _lRashaSizeMod = 0.9f;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            if (Hud.Game.IsInGame && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Wizard && !(Hud.Game.Me.IsInTown && !ShowInTown))
            {
                var me = Hud.Game.Me;
                UpdateSkills(me);

                //If Disentegration Wave is channelled, draw zei circle
                if (me.Powers.BuffIsActive(392891, 4) && ShowZeiCircle)
                    ZeiRanceIndicator.Paint(me, me.FloorCoordinate, null);

                //Draw missing buff warnings
                if (ShowWarnings && !me.IsDead)
                    DrawWarnings(me);

                //Draw indicators for each tal rasha element
                if ((me.Powers.BuffIsActive(429855, 5) || AlwaysShowElements) && ShowRashaElements)
                    TalRashaElements(me);

                //Draw Archon cooldown
                if (archonSkill != null && ShowArchonCD)
                    ArchonCooldownLabel.Paint(hudWidth * 0.5f - _lWidth / 2, hudHeight * 0.515f, _lWidth, _lHeight, HorizontalAlign.Center);

                //Draw Archon time remaining
                if (archonSkill != null && ShowArchonRemain)
                    ArchonRemaining(me);
            }
        }

        public string ArchonCooldown()
        {
            string s = "";
            if (archonSkill.CooldownFinishTick > Hud.Game.CurrentGameTick)
            {
                var c = (archonSkill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0d;
                s = string.Format("\u267F {0:N1} \u267F", c);
            }
            return s;
        }

        private void ArchonRemaining(IPlayer me)
        {
            if (me.Powers.BuffIsActive(134872, 2))
            {
                if (!_timerRunning)
                    _tick = Hud.Game.CurrentGameTick;
                _timerRunning = true;

                var r = 20f - ((Hud.Game.CurrentGameTick - _tick) / 60.0d);
                if (r > 3f)
                {
                    var layout = ArchonRemainFont.GetTextLayout(string.Format("{0:N1}", r));
                    ArchonRemainFont.DrawText(layout, hudWidth * 0.5f - (layout.Metrics.Width * 0.5f), hudHeight * 0.515f);
                }
                else {
                    string str = string.Format("{0:N1}", r);
                    if (r <= 3 && r > 2) str = string.Format("\u231A {0:N1} \u231A", r);
                    if (r <= 2 && r > 1) str = string.Format("\u231B {0:N1} \u231B", r);
                    if (r <= 1) str = string.Format("\u25B6 {0:N1} \u25C0", r);
                    var layout = ArchonRemainSoonFont.GetTextLayout(str);
                    ArchonRemainSoonFont.DrawText(layout, hudWidth * 0.5f - (layout.Metrics.Width * 0.5f), hudHeight * 0.505f);
                }
            }
            else _timerRunning = false;
        }

        private void TalRashaElements(IPlayer me)
        {
            RashaBackgroundBrush.DrawRectangle((hudWidth * 0.5f - _lRashaSize * .5f) - _lRashaSize * 1.6f, hudHeight * _lRashaYpos - _lRashaSize * 0.1f, _lRashaSize * 4.1f, _lRashaSize * 1.1f);

            if (me.Powers.BuffIsActive(429855, 1)) ArcaneBrush.DrawRectangle((hudWidth * 0.5f - _lRashaSize * .5f) - _lRashaSize * 1.5f, hudHeight * _lRashaYpos, _lRashaSize * _lRashaSizeMod, _lRashaSize * _lRashaSizeMod);
            else DrawGreyBrush(-_lRashaSize * 1.5f);

            if (me.Powers.BuffIsActive(429855, 2)) ColdBrush.DrawRectangle((hudWidth * 0.5f - _lRashaSize * .5f) - _lRashaSize / 2, hudHeight * _lRashaYpos, _lRashaSize * _lRashaSizeMod, _lRashaSize * _lRashaSizeMod);
            else DrawGreyBrush(-_lRashaSize / 2);

            if (me.Powers.BuffIsActive(429855, 3)) FireBrush.DrawRectangle((hudWidth * 0.5f - _lRashaSize * .5f) + _lRashaSize / 2, hudHeight * _lRashaYpos, _lRashaSize * _lRashaSizeMod, _lRashaSize * _lRashaSizeMod);
            else DrawGreyBrush(_lRashaSize / 2);

            if (me.Powers.BuffIsActive(429855, 4)) LightningBrush.DrawRectangle((hudWidth * 0.5f - _lRashaSize * .5f) + _lRashaSize * 1.5f, hudHeight * _lRashaYpos, _lRashaSize * _lRashaSizeMod, _lRashaSize * _lRashaSizeMod);
            else DrawGreyBrush(_lRashaSize * 1.5f);
        }

        private void DrawGreyBrush(float xPos)
        {
            GreyBrush.DrawRectangle((hudWidth * 0.5f - _lRashaSize * .5f) + xPos, hudHeight * _lRashaYpos, _lRashaSize * _lRashaSizeMod, _lRashaSize * _lRashaSizeMod);
        }

        private void DrawWarnings(IPlayer me)
        {
            //IN ARCHON
            if (me.Powers.BuffIsActive(134872, 2)) //Archon
            {
                if (!me.Powers.BuffIsActive(135663, 0)) //Slow Time
                {
                    var layout = WarningFont.GetTextLayout("\u22EF Bubble Up \u22EF");
                    WarningFont.DrawText(layout, hudWidth * 0.5f - (layout.Metrics.Width * 0.5f), hudHeight * 0.47f);
                }
            }else
            {//NOT IN ARCHON
                if (magicWeaponSkill != null)
                {
                    var layout = WarningFont.GetTextLayout("\u22EF Missing Magic Weapon \u22EF");
                    if (!me.Powers.BuffIsActive(76108, 0))
                        WarningFont.DrawText(layout, hudWidth * 0.5f - (layout.Metrics.Width * 0.5f), hudHeight * 0.47f);
                }

                if (energyArmorSkill != null)
                {
                    var layout = WarningFont.GetTextLayout("\u22EF Missing Energy Armor \u22EF");
                    if (!me.Powers.BuffIsActive(86991, 0))
                        WarningFont.DrawText(layout, hudWidth * 0.5f - (layout.Metrics.Width * 0.5f), hudHeight * 0.485f);
                }
            }
        }

        private void UpdateSkills(IPlayer me)
        {
            me.Powers.UsedSkills.ForEach(skill =>
            {
                if (skill.SnoPower.Sno == 134872) archonSkill = skill;
                if (skill.SnoPower.Sno == 76108) magicWeaponSkill = skill;
                if (skill.SnoPower.Sno == 86991) energyArmorSkill = skill;
            });
        }
    }
}