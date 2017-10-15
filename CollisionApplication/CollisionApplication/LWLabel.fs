module LWLabel

open VWCoordinates
open LWC
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

type LWLabel() = 
    inherit LWC()

    let mutable maxHeight = 14.f

    member val Text : string = "" with get, set
    
    member this.MaxHeight
      with get() = maxHeight
      and set(v) = maxHeight <- v

    override this.OnPaint e =
      let g = e.Graphics

      let r = g.MeasureString(this.Text, this.GetFont)
      let x, y = (this.Size.Width - r.Width + 6.f) / 2.f, (maxHeight - r.Height) / 2.f

      g.FillRectangle(Brushes.LightGray, 0.f, 0.f, this.Size.Width, maxHeight)
      g.DrawRectangle(Pens.Black, 0.f, 0.f, this.Size.Width, maxHeight)
      g.DrawString(this.Text, this.GetFont, Brushes.Black, PointF(x, y))

    override this.HitTest p =
      if p.X >= 0.f && p.X < this.Size.Width && p.Y >= 0.f && p.Y < maxHeight then true
      else false


