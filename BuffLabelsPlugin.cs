namespace Turbo.Plugins.RuneB
{
    using Turbo.Plugins.Default;

    public class BuffLabelsPlugin : BasePlugin, IInGameWorldPainter
    {
        public bool IgnorePain { get; set; }
        public bool Oculus { get; set; }
        public bool InnerSanctuary { get; set; }
        public bool FlyingDragon { get; set; }

        public float YPos { get; set; }
        public float XPos { get; set; }

        public float YPosIncrement { get; set; }

        public float LabelWidthPercentage { get; set; }
        public float LabelHeightPercentage { get; set; }

        public bool Debug { get; set; }

        public IFont TextFont { get; set; }
        public IBrush BorderBrush { get; set; }
        public IBrush BackgroundBrushIP { get; set; }
        public IBrush BackgroundBrushOC { get; set; }
        public IBrush BackgroundBrushIS { get; set; }
        public IBrush BackgroundBrushFD { get; set; }

        private float _lWidth, LH, Width, Height, YPosTemp;

        public BuffLabelsPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            //Turn labels on and off
            IgnorePain = true;
            Oculus = true;
            InnerSanctuary = true;
            FlyingDragon = true;
            
            //Horizontal and Vertical label positions.
            YPos = 0.65f;
            XPos = 0.5f;

            //Label size is based on a percentage of screen width/height
            LabelWidthPercentage = 0.052f;
            LabelHeightPercentage = 0.016f;
            
            //Vertical distance between labels
            YPosIncrement = 0.02f;

            //If true labels are always shown
            Debug = false;

            TextFont = Hud.Render.CreateFont("tahoma", 6, 240, 240, 240, 240, true, false, true);
            BorderBrush = Hud.Render.CreateBrush(150, 30, 30, 30, 0);

            BackgroundBrushIP = Hud.Render.CreateBrush(100, 100, 225, 100, 0);   // Ignore Pain
            BackgroundBrushOC = Hud.Render.CreateBrush(100, 255, 255, 50, 0);    // Oculus
            BackgroundBrushIS = Hud.Render.CreateBrush(100, 185, 220, 245, 0);   // Inner Sanctuary
            BackgroundBrushFD = Hud.Render.CreateBrush(100, 50, 200, 255, 0);    // Flying Dragon

            Width = Hud.Window.Size.Width;
            Height = Hud.Window.Size.Height;
            _lWidth = Width * LabelWidthPercentage;     // label width
            LH = Height * LabelHeightPercentage;   // label height

            YPosTemp = YPos;           
        }

        public void PaintWorld(WorldLayer layer)
        {
            if (IgnorePain && (Hud.Game.Me.Powers.BuffIsActive(79528, 0) || Hud.Game.Me.Powers.BuffIsActive(79528, 1)) || Debug)
                DrawLabel(BackgroundBrushIP,"Ignore Pain");

            if (Oculus && Hud.Game.Me.Powers.BuffIsActive(402461, 2) || Debug)
                DrawLabel(BackgroundBrushOC, "Oculus");

            if (InnerSanctuary && Hud.Game.Me.Powers.BuffIsActive(317076, 1) || Debug)
                DrawLabel(BackgroundBrushIS, "Inner Sanctuary");

            if (FlyingDragon && Hud.Game.Me.Powers.BuffIsActive(246562, 1) || Debug)
                DrawLabel(BackgroundBrushFD, "Flying Dragon");

            YPosTemp = YPos;
        }



        private void DrawLabel(IBrush label, string buffText) {
            YPosTemp += YPosIncrement;
            BorderBrush.DrawRectangle(Width * XPos - _lWidth * 1.05f * .5f, Height * YPosTemp - LH * 1.1f, _lWidth * 1.05f, LH * 1.2f);
            label.DrawRectangle(Width * XPos - _lWidth * .5f, Height * YPosTemp - LH, _lWidth, LH);

            var layout = TextFont.GetTextLayout(buffText);
            TextFont.DrawText(layout, Width * XPos - (layout.Metrics.Width*0.5f), Height * YPosTemp - LH + 2f);
        }
    }
}

/*- Added Flying Dragon
- Made it easier to change the size and position of the labels
- Added Inner Sanctuary 
- Refactored code, made labels and text more visible and added borders.
- Fixed naming convention and accessors. Added Ignore Pain. Renamed to BuffLabelsPlugin.cs
- Fixed bug: IP was only working for barbs.
- There are now only one X and one Y variable controlling the whole "box" of labels. The potential gab between labels, when for example Ignore Pain and Inner Sanctum was active but Oculus was not, has been removed. Instead the vertical positions are generated based on the amount of active buffs. 
-Added textcentering based on text width*/
