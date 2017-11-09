using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

namespace Turbo.Plugins.RuneB
{
    public class RathmaPlugin : BasePlugin, IInGameTopPainter, ICustomizer
    {
        public bool ShowSimulacrumCD { get; set; }
        public bool ShowSimulacrumRemain { get; set; }

        public bool ShowInTown { get; set; }

        public float SimulacrumCDandRemainYPos { get; set; }
        public float MonsterInBoneRangeYPos { get; set; }
        public float EssenceLabelHeight { get; set; }

        public TopLabelDecorator SimulacrumCooldownLabel { get; set; }

        public IFont SimulacrumRemainFont { get; set; }


        private float HudWidth { get { return Hud.Window.Size.Width; } }
        private float HudHeight { get { return Hud.Window.Size.Height; } }
        private float CurEssence { get { return Hud.Game.Me.Stats.ResourceCurEssence; } }
        private float MaxEssence { get { return Hud.Game.Me.Stats.ResourceMaxEssence; } }

        private static uint CommandSkeletonSkillSNO = 453801;
        public HashSet<uint> CommandSkeletonActorSNOs = new HashSet<uint>{473147, 473428, 473426, 473420, 473417, 473418 };
        private static uint SkeletonMageSkillSNO = 462089;
        public HashSet<uint> SkeletonMageActorSNOs = new HashSet<uint> {472275, 472588, 472769, 472801, 472606, 472715 };

        private float _lWidth, _lHeight, _tick;
        private bool _timerRunning;

        private IPlayerSkill _simulacrumSkill;
        private IPlayerSkill _boneArmorSkill;
        private IBrush _essenceBrush;

        public RathmaPlugin()
        {
            Enabled = true;
        }

        public void Customize()
        {

            Hud.RunOnPlugin<PlayerBottomBuffListPlugin>(plugin =>
                {
                    plugin.BuffPainter.ShowTimeLeftNumbers = true;
                    plugin.RuleCalculator.Rules.Add(new BuffRule(465350) { IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = false, ShowStacks = false, IconSizeMultiplier = 1.3f }); // Simulacrum
                });
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            ShowInTown = true;
            ShowSimulacrumCD = true;
            ShowSimulacrumRemain = true;

            SimulacrumCDandRemainYPos = 0.495f;
            MonsterInBoneRangeYPos = 0.480f;
            EssenceLabelHeight = 16;

            _essenceBrush = Hud.Render.CreateBrush(100, 250, 255, 0, 0);
            SimulacrumRemainFont = Hud.Render.CreateFont("tahoma", 12f, 255, 80, 140, 210, false, false, true);

            SimulacrumCooldownLabel = new TopLabelDecorator(Hud)
            {
                TextFont = Hud.Render.CreateFont("tahoma", 10f, 255, 140, 140, 180, false, false, 160, 0, 0, 0, true),
                TextFunc = SimulacrumCooldown,
            };

            _lWidth = 80;
            _lHeight = 15;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;

            if (Hud.Game.IsInGame && Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Necromancer && !(Hud.Game.Me.IsInTown && !ShowInTown))
            {
                var me = Hud.Game.Me;
                UpdateSkills(me);

                //Simulacrum cooldown
                if (_simulacrumSkill != null && !me.Powers.BuffIsActive(465350, 1) && ShowSimulacrumCD)
                    SimulacrumCooldownLabel.Paint(HudWidth * 0.5f - _lWidth / 2, HudHeight * (SimulacrumCDandRemainYPos + 0.015f), _lWidth, _lHeight, HorizontalAlign.Center);

                //Simulacrum time remaining
                if (_simulacrumSkill != null && ShowSimulacrumRemain)
                    SimulacrumRemaining(me);

                //Dynamic essence bar
                    ShowEssence();

                //Mages and skeletons

                //Bone Armor 
                //if (_boneArmorSkill != null) ShowNearbyEnemies();
            }
        }

        private void ShowEssence()
        {
            //var monsterCount = Hud.Game.AliveMonsters.Where(monster => monster.NormalizedXyDistanceToMe < 30 && monster.SummonerAcdDynamicId == 0).Count();

            //var layout = SimulacrumRemainFont.GetTextLayout(string.Format("{0:N0}", Hud.Game.Me.Stats.ResourceCurEssence));
            //SimulacrumRemainFont.DrawText(layout, HudWidth * 0.5f - (layout.Metrics.Width * 0.5f), HudHeight * (MonsterInBoneRangeYPos + 0.015f));
            //_boneArmorSkill.
            //Hud.Game.Me.Stats.ResourceCurEssence
            var labelWidth = map(CurEssence,0, MaxEssence, 1, 100);
            var color = (int) map(CurEssence, 0, MaxEssence, 0, 255);
            _essenceBrush = Hud.Render.CreateBrush(245, 255-(int)(color*0.8f), color, 0, 0);
            _essenceBrush.DrawRectangle(HudWidth*0.5f-labelWidth*0.5f, HudHeight*0.485f,labelWidth, EssenceLabelHeight);
            _essenceBrush.DrawEllipse(HudWidth * 0.5f - labelWidth * 0.5f, HudHeight * 0.485f + EssenceLabelHeight/2, EssenceLabelHeight / 2, EssenceLabelHeight / 2);
            _essenceBrush.DrawEllipse(HudWidth * 0.5f + labelWidth * 0.5f, HudHeight * 0.485f + EssenceLabelHeight / 2, EssenceLabelHeight / 2, EssenceLabelHeight / 2);

        }
    

        public string SimulacrumCooldown()
        {
            string s = "";
            if (_simulacrumSkill.CooldownFinishTick > Hud.Game.CurrentGameTick)
            {
                var c = (_simulacrumSkill.CooldownFinishTick - Hud.Game.CurrentGameTick) / 60.0d;
                s = string.Format("\u267F {0:N1} \u267F", c);
            }
            return s;
        }

        private void SimulacrumRemaining(IPlayer me)
        {
            if (me.Powers.BuffIsActive(465350, 1))
            {
                if (!_timerRunning)
                    _tick = Hud.Game.CurrentGameTick;
                _timerRunning = true;

                var r = 30f - ((Hud.Game.CurrentGameTick - _tick) / 60.0d);
                if (r > 3f)
                {
                    var layout = SimulacrumRemainFont.GetTextLayout(string.Format("{0:N1}", r));
                    SimulacrumRemainFont.DrawText(layout, HudWidth * 0.5f - (layout.Metrics.Width * 0.5f), HudHeight * (SimulacrumCDandRemainYPos + 0.015f));
                }
                else
                {
                    string str = string.Format("{0:N1}", r);
                    if (r <= 3 && r > 2) str = string.Format("\u231A {0:N1} \u231A", r);
                    if (r <= 2 && r > 1) str = string.Format("\u231B {0:N1} \u231B", r);
                    if (r <= 1) str = string.Format("\u25B6 {0:N1} \u25C0", r);
                    var layout = SimulacrumRemainFont.GetTextLayout(str);
                    SimulacrumRemainFont.DrawText(layout, HudWidth * 0.5f - (layout.Metrics.Width * 0.5f), HudHeight * (SimulacrumCDandRemainYPos + 0.005f));
                }
            }
            else _timerRunning = false;
        }

        private void UpdateSkills(IPlayer me)
        {
            _simulacrumSkill = null;
            _boneArmorSkill = null;
            //_energyArmorSkill = null;
            me.Powers.UsedSkills.ForEach(skill =>
            {

                if (skill.SnoPower.Sno == 465350) _simulacrumSkill = skill;
                if (skill.SnoPower.Sno == 466857) _boneArmorSkill = skill;
                //if (skill.SnoPower.Sno == 86991) _energyArmorSkill = skill;
            });
        }

        public float map(float value, float istart, float istop, float ostart, float ostop)
        {
            return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
        }

    }
}


