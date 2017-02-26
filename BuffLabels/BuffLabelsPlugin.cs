namespace Turbo.Plugins.RuneB
{
    using Turbo.Plugins.Default;
    using System.Collections.Generic;

    public class BuffLabelsPlugin : BasePlugin, IInGameTopPainter
    {
        public bool ShowIgnorePain { get; set; }
        public bool ShowOculus { get; set; }
        public bool ShowInnerSanctuary { get; set; }

        public bool ChangeTextSize { get; set; }

        public float YPos { get; set; }
        public float XPos { get; set; }
        public float SizeModifier { get; set; }
        public float TextSize { get; set; }
        public float JumpDistance { get; set; }
        public int NumRows { get; set; }

        public float YPosIncrement { get; set; }

        public bool Debug { get; set; }

        public IFont TextFont { get; set; }
        public IBrush BorderBrush { get; set; }
        public IBrush BackgroundBrushIP { get; set; }
        public IBrush BackgroundBrushOC { get; set; }
        public IBrush BackgroundBrushIS { get; set; }

        public List<Label> Labels { get; set; }

        private float _yPosTemp, _previousTextSize, _labelWidthPercentage, _labelHeightPercentage, _jumpCount;
        private bool _jumped;
        private float hudWidth { get { return Hud.Window.Size.Width; } }
        private float hudHeight { get { return Hud.Window.Size.Height; } }

        private float lWidth { get { return hudWidth * _labelWidthPercentage * SizeModifier; } }
        private float lHeight { get { return hudHeight * _labelHeightPercentage * SizeModifier; } }

        public BuffLabelsPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            //Turn labels on and off
            ShowIgnorePain = true;
            ShowOculus = true;
            ShowInnerSanctuary = true;

            //Horizontal and Vertical label positions.
            YPos = 0.65f;
            XPos = 0.5f;

            SizeModifier = 1f;
            TextSize = 6;
            JumpDistance = 1.1f;
            NumRows = 5;

            //Label size is based on a percentage of screen width/height
            _labelWidthPercentage = 0.052f;
            _labelHeightPercentage = 0.016f;

            //Vertical distance between labels
            YPosIncrement = 0.02f;

            //If true labels are always shown
            Debug = false;

            ChangeTextSize = false;

            //TextFont = Hud.Render.CreateFont("tahoma", TextSize, 240, 240, 240, 240, true, false, true);
            BorderBrush = Hud.Render.CreateBrush(150, 30, 30, 30, 0);

            BackgroundBrushIP = Hud.Render.CreateBrush(100, 100, 225, 100, 0);   // Ignore Pain
            BackgroundBrushOC = Hud.Render.CreateBrush(100, 255, 255, 50, 0);    // Oculus
            BackgroundBrushIS = Hud.Render.CreateBrush(100, 185, 220, 245, 0);   // Inner Sanctuary

            Labels = new List<Label>();
            Labels.Add(new Label("Oculus", 402461, 2, BackgroundBrushOC, ShowOculus));
            Labels.Add(new Label("Inner Sanctuary", 317076, 1, BackgroundBrushIS, ShowInnerSanctuary));

            _jumpCount = 1;
            _yPosTemp = YPos;
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            //Allow changing font size from a customize method
            if (TextFont == null)
            {
                ChangeTextSize = false;
                TextFont = Hud.Render.CreateFont("tahoma", TextSize * SizeModifier, 240, 240, 240, 240, true, false, true);
            }

            foreach (Label l in Labels)
                if (l.Show && (Hud.Game.Me.Powers.BuffIsActive((uint)l.Sno, l.IconCount) || Debug))
                    DrawLabel(l.LabelBrush, l.NameText);
            
            //Avoid showing two IP labels
            if (ShowIgnorePain && (Hud.Game.Me.Powers.BuffIsActive(79528, 0) || Hud.Game.Me.Powers.BuffIsActive(79528, 1)) || Debug)
                DrawLabel(BackgroundBrushIP, "Ignore Pain");

            _yPosTemp = YPos;
            _jumped = false;
            _jumpCount = 0;

        }



        private void DrawLabel(IBrush label, string buffText)
        {
            float xJump = CalculateJump();

            BorderBrush.DrawRectangle(hudWidth * XPos - (lWidth * 1.05f * .5f) + xJump, hudHeight * _yPosTemp - lHeight * 1.1f, lWidth * 1.05f, lHeight * 1.2f);
            label.DrawRectangle(hudWidth * XPos - lWidth * .5f + xJump, hudHeight * _yPosTemp - lHeight, lWidth, lHeight);

            var layout = TextFont.GetTextLayout(buffText);
            TextFont.DrawText(layout, hudWidth * XPos - (layout.Metrics.Width * 0.5f) + xJump, hudHeight * _yPosTemp - lHeight + 2f);
            _yPosTemp += YPosIncrement * SizeModifier;
        }

        private float CalculateJump()
        {
            float xJump = lWidth * JumpDistance * _jumpCount;
            if (_yPosTemp >= (YPos + (YPosIncrement * SizeModifier * (NumRows - 1))))
            {
                _yPosTemp = YPos;
                _jumpCount+=1;
            }
            return xJump;
        }
    }

    public class Label
    {
        public string NameText { get; set; }
        public int Sno { get; set; }
        public int IconCount { get; set; }
        public IBrush LabelBrush { get; set; }
        public bool Show { get; set; }

        public Label(string NameText, int Sno, int IconCount, IBrush LabelBrush)
        {
            this.NameText = NameText;
            this.Sno = Sno;
            this.IconCount = IconCount;
            this.LabelBrush = LabelBrush;
            Show = true;
        }

        public Label(string NameText, int Sno, int IconCount, IBrush LabelBrush, bool Show)
        {
            this.NameText = NameText;
            this.Sno = Sno;
            this.IconCount = IconCount;
            this.LabelBrush = LabelBrush;
            this.Show = Show;
        }
    }
}
