module LWFigure

open VWCoordinates
open LWC
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

type LWFigure(f:Figure, c:Color) as this =
    inherit LWC()

    let figureType = f
    let figureColor = c
    let rnd = System.Random()
    let figureBrush = new SolidBrush(c)
    let borderPen = new Pen(Color.Red, DashStyle = DashStyle.Dash, Width = 2.f)
    let mutable speedVector = PointF(0.f, 0.f)
    let mutable accelerationVector = PointF(0.f, 0.f)

    do this.controlType <- DRAWING(f)
     
    member val TextV = "" with get, set
    
    member val TextPos = "" with get, set
    
    member this.UpdatePosition = this.Location <- PointF(this.Location.X + speedVector.X, this.Location.Y + speedVector.Y)
                                 this.TextPos <- "Pos: (" + this.Location.X.ToString("0.0") + "; " + this.Location.Y.ToString("0.0") + ")"
                                 this.TextV <-   "Vel: (" + speedVector.X.ToString("0.0") + "; " + speedVector.Y.ToString("0.0") + ")"
    member this.UpdateSpeed = speedVector <- PointF(speedVector.X + accelerationVector.X, speedVector.Y + accelerationVector.Y)

    member this.UpdateAcceleration (f:PointF) = 
      let m = this.Size.Width * this.Size.Width * single System.Math.PI
      let addAcc = PointF(f.X / m, f.Y / m)
      accelerationVector <- PointF(accelerationVector.X + addAcc.X, accelerationVector.Y + addAcc.Y)

    member this.ResetAcceleration () = accelerationVector <- PointF(0.f, 0.f)

    member this.IncreaseSpeed = speedVector <- PointF(speedVector.X * 1.1f, speedVector.Y * 1.1f)
    
    member this.DecreaseSpeed = speedVector <- PointF(speedVector.X / 1.1f, speedVector.Y / 1.1f)
    
    member this.TurnLeft = let mat = VWCoordinates() in mat.Rotate(3.f); speedVector <- mat.TransformPointVWF(speedVector)
    
    member this.TurnRight = let mat = VWCoordinates() in mat.Rotate(-3.f); speedVector <- mat.TransformPointVWF(speedVector)
    
    member this.Move (p:PointF) = this.Location <- PointF(this.Location.X + p.X, this.Location.Y + p.Y)
    
    member this.FigureType = figureType
    
    member this.SpeedVector
      with get() = speedVector
      and set(v) = speedVector <- v; this.TextV <- "Pos: (" + speedVector.X.ToString("0.0") + "; " + speedVector.Y.ToString("0.0") + ")"

    member this.FuturePosition = PointF(this.Location.X + speedVector.X, this.Location.Y + speedVector.Y)

    member val isSelected = true with get, set

    override this.OnPaint e =
      let g = e.Graphics

      if figureType.Equals(RECTANGLE) then
        g.FillRectangle(figureBrush, 0.f, 0.f, this.Size.Width, this.Size.Height)
        g.DrawRectangle(Pens.Black, 0.f, 0.f, this.Size.Width, this.Size.Height)
        g.DrawRectangle(Pens.White, 1.f, 1.f, this.Size.Width - 2.f, this.Size.Height - 2.f)
        if this.isSelected then g.DrawRectangle(borderPen, 0.f, 0.f, this.Size.Width, this.Size.Height)
      else if figureType.Equals(CIRCLE) then 
        g.FillEllipse(figureBrush, 0.f, 0.f, this.Size.Width, this.Size.Height)
        g.DrawEllipse(Pens.Black, 0.f, 0.f, this.Size.Width, this.Size.Height)
        g.DrawEllipse(Pens.White, 1.f, 1.f, this.Size.Width - 2.f, this.Size.Height - 2.f)
        if this.isSelected then g.DrawEllipse(borderPen, 0.f, 0.f, this.Size.Width, this.Size.Height)
        
      
      let r1 = g.MeasureString(this.TextV, this.GetFont)
      let r2 = g.MeasureString(this.TextPos, this.GetFont)
      let x1, y1 = (this.Size.Width - r1.Width) / 2.f, -5.f + (this.Size.Height - r1.Height) / 2.f
      let x2, y2 = (this.Size.Width - r2.Width) / 2.f, 5.f + (this.Size.Height - r2.Height) / 2.f
      
      g.DrawString(this.TextV, this.GetFont, Brushes.White, PointF(x1, y1))
      g.DrawString(this.TextPos, this.GetFont, Brushes.White, PointF(x2, y2))



    override this.HitTest p =
      match figureType with
      | RECTANGLE -> (RectangleF(PointF(), this.Size)).Contains(p)
      | CIRCLE ->
            let r = this.Size.Width / 2.f
            let dist = single (System.Math.Sqrt(float ((p.X - r)*(p.X - r) + (p.Y - r)*(p.Y - r))))
            if dist <= r then true
            else false
      | _ -> false


