module LWRoundButton

open VWCoordinates
open LWC
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

type butState =
    | Pressed
    | Released
    | MouseOver

type LWRoundButton(p:LWC) as this =
    inherit LWC()
    
    let pressedColor1, pressedColor2 = Color.FromArgb(255, 100, 100, 100), Color.FromArgb(255, 160, 150, 140)
    let releasedColor1, releasedColor2 = Color.FromArgb(255, 180, 180, 180), Color.FromArgb(255, 100, 90, 80)
    let mouseOverColor1, mouseOverColor2 = Color.FromArgb(255, 200, 200, 200), Color.FromArgb(255, 100, 90, 80)
    let mutable state = Released
    
    do 
      this.controlType <- BUTTON
      this.LWParent <- p

    member val Text : string = "" with get, set
    
    override this.OnPaint(e) =
      let g = e.Graphics
      g.SmoothingMode <- SmoothingMode.HighQuality
      
      let specialBrush =
        match state with
        | Pressed -> new LinearGradientBrush(Point(0, 0), Point(0, int this.Size.Height), pressedColor1, pressedColor2)
        | Released -> new LinearGradientBrush(Point(0, 0), Point(0, int this.Size.Height), releasedColor1, releasedColor2)
        | MouseOver -> new LinearGradientBrush(Point(0, 0), Point(0, int this.Size.Height), mouseOverColor1, mouseOverColor2)
      
      let borderPen = new Pen(Color.Black, Width = 1.f)

      let minDim = min this.Size.Width this.Size.Height

      g.FillEllipse(specialBrush, 0.f, 0.f, minDim / 4.f, minDim / 4.f)
      g.DrawEllipse(borderPen   , 0.f, 0.f, minDim / 4.f, minDim / 4.f)
      g.FillEllipse(specialBrush, this.Size.Width - minDim / 4.f, 0.f, minDim / 4.f, minDim / 4.f)
      g.DrawEllipse(borderPen   , this.Size.Width - minDim / 4.f, 0.f, minDim / 4.f, minDim / 4.f)
      g.FillEllipse(specialBrush, 0.f, this.Size.Height - minDim / 4.f, minDim / 4.f, minDim / 4.f)
      g.DrawEllipse(borderPen   , 0.f, this.Size.Height - minDim / 4.f, minDim / 4.f, minDim / 4.f)
      g.FillEllipse(specialBrush, this.Size.Width - minDim / 4.f, this.Size.Height - minDim / 4.f, minDim / 4.f, minDim / 4.f)
      g.DrawEllipse(borderPen   , this.Size.Width - minDim / 4.f, this.Size.Height - minDim / 4.f, minDim / 4.f, minDim / 4.f)
      
      g.FillRectangle(specialBrush, minDim / 8.f, 0.f, this.Size.Width - minDim / 4.f, this.Size.Height)
      g.FillRectangle(specialBrush, 0.f, minDim / 8.f, this.Size.Width, this.Size.Height - minDim / 4.f)
      
      g.DrawLine(borderPen, Point(int minDim / 8, 0), Point(int this.Size.Width - int minDim / 8, 0))
      g.DrawLine(borderPen, Point(int minDim / 8, int this.Size.Height), Point(int this.Size.Width - int minDim / 8, int this.Size.Height))
      g.DrawLine(borderPen, Point(0, int minDim / 8), Point(0, int this.Size.Height - int minDim / 8))
      g.DrawLine(borderPen, Point(int this.Size.Width, int minDim / 8), Point(int this.Size.Width, int this.Size.Height - int minDim / 8))
    
      let r = g.MeasureString(this.Text, this.GetFont)
      let x, y = (this.Size.Width - r.Width) / 2.f, (this.Size.Height - r.Height) / 2.f
      g.DrawString(this.Text, this.GetFont, Brushes.Black, PointF(x, y))
      

    override this.HitTest p =
      (RectangleF(PointF(), this.Size)).Contains(p)
    
    override this.OnMouseEnter(e) = state <- MouseOver; this.Invalidate() 

    override this.OnMouseLeave(e) = state <- Released; this.Invalidate()

    override this.OnMouseDown(e) = state <- Pressed; this.Invalidate(); base.OnMouseDown(e)
    override this.OnMouseUp(e)   = state <- Released; this.Invalidate(); this.isMouseHover <- false; base.OnMouseUp(e)



