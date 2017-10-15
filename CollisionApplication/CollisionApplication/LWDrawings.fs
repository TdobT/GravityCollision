module LWDrawings

open VWCoordinates
open LWC
open LWPanel
open LWColorContainer
open LWFigure
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

type LWDrawings() as this =
  inherit LWPanel(Color.White)

  let DrawEnabled = true
  let timer = new Timer()

  let mutable selectedDraw : Figure = CIRCLE
  let mutable selectedColor = Color.Black
  let mutable position = PointF()
  let mutable dragStart = None
  let mutable selectedFigure : LWFigure option = None

  let toRectangle (r:RectangleF) = Rectangle(int r.X, int r.Y, int r.Width, int r.Height)
  let toPointF (p:Point) = PointF(single p.X, single p.Y)

  let mkRectangleF(p1:PointF, p2:PointF) =
    let x1, y1 = min p1.X p2.X, min p1.Y p2.Y
    let x2, y2 = max p1.X p2.X, max p1.Y p2.Y

    if selectedDraw = CIRCLE then 
      let xh, yh = x2 - x1, y2 - y1
      if p2.Y < p1.Y then
        if p2.X < p1.X then 
          if yh >= xh then RectangleF(x1, y1 + (yh - xh), xh, xh)
          else RectangleF(x1 + (xh - yh), y1, yh, yh)
        else
          if yh >= xh then RectangleF(x1, y1 + (yh - xh), xh, xh)
          else RectangleF(x1, y1, yh, yh)
      else
        if p2.X < p1.X then
          if yh >= xh then RectangleF(x1, y1, xh, xh)
          else RectangleF(x1 + (xh - yh), y1, yh, yh)
        else
          if yh >= xh then RectangleF(x1, y1, xh, xh)
          else RectangleF(x1, y1, yh, yh)
                    
    else RectangleF(x1, y1, x2 - x1, y2 - y1)


  do 
    timer.Interval <- 30
    timer.Tick.Add(fun t ->

      for i in 0 .. this.LWControls.Count - 1 do
        let c = this.LWControls.[i] :?> LWFigure
        c.ResetAcceleration()

      for i in 0 .. this.LWControls.Count - 1 do
        let c = this.LWControls.[i] :?> LWFigure
        for j in i+1 .. this.LWControls.Count - 1 do
          let c2 = this.LWControls.[j] :?> LWFigure
          let isCollide = this.Collide(c, c2)
          if c2.FigureType.Equals(CIRCLE) && (not isCollide) then
            this.Force (c, c2)

        c.UpdateSpeed
        c.UpdatePosition
        c.Invalidate()
      
    )

  member this.Force (c1:LWFigure, c2:LWFigure) =
    let m1 = c1.Size.Width * c1.Size.Width * single System.Math.PI 
    let m2 = c2.Size.Width * c2.Size.Width * single System.Math.PI 
    let center1 = PointF(c1.Location.X + c1.Size.Width / 2.f, c1.Location.Y + c1.Size.Height / 2.f)
    let center2 = PointF(c2.Location.X + c2.Size.Width / 2.f, c2.Location.Y + c2.Size.Height / 2.f)
    let distCenter = this.PointDistance(center1, center2)
    let directionX = (center2.X - center1.X) / (distCenter)
    let directionY = (center2.Y - center1.Y) / (distCenter)
    c1.UpdateAcceleration(PointF(0.01f * directionX * (m1 * m2) / (distCenter * distCenter), 0.01f * directionY * (m1 * m2) / (distCenter * distCenter)))
    c2.UpdateAcceleration(PointF(-0.01f * directionX * (m1 * m2) / (distCenter * distCenter), -0.01f * directionY * (m1 * m2) / (distCenter * distCenter)))

  
  member this.Collide (f1:LWFigure, f2:LWFigure) =

    if f1.FigureType.Equals(RECTANGLE) && f2.FigureType.Equals(RECTANGLE) then false

    else if f1.FigureType.Equals(RECTANGLE) || f2.FigureType.Equals(RECTANGLE) then false

    else 
      let f1FutureLocation, f2FutureLocation = f1.FuturePosition, f2.FuturePosition
      let v1, v2 = f1.SpeedVector, f2.SpeedVector
      let center1 = PointF(f1FutureLocation.X + f1.Size.Width / 2.f, f1FutureLocation.Y + f1.Size.Height / 2.f)
      let center2 = PointF(f2FutureLocation.X + f2.Size.Width / 2.f, f2FutureLocation.Y + f2.Size.Height / 2.f)
      let distCenter = this.PointDistance(center1, center2)
      if (f1.Size.Width / 2.f + f2.Size.Width / 2.f) >= distCenter then 
          let m1, m2 = f1.Size.Width * f1.Size.Width * single System.Math.PI, f2.Size.Width * f2.Size.Width * single System.Math.PI
          let nX = (center1.X - center2.X) / (distCenter)
          let nY = (center1.Y - center2.Y) / (distCenter)
          let Cmx = ((m1 * v1.X) + (m2 * v2.X)) / (m1 + m2)
          let Cmy = ((m1 * v1.Y) + (m2 * v2.Y)) / (m1 + m2)
          let finalV1X = v1.X - (2.f * ((v1.X - Cmx) * nX + (v1.Y - Cmy) * nY) * nX)
          let finalV1Y = v1.Y - (2.f * ((v1.X - Cmx) * nX + (v1.Y - Cmy) * nY) * nY)
          let finalV2X = v2.X - (2.f * ((v2.X - Cmx) * nX + (v2.Y - Cmy) * nY) * nX)
          let finalV2Y = v2.Y - (2.f * ((v2.X - Cmx) * nX + (v2.Y - Cmy) * nY) * nY)
          f1.SpeedVector <- PointF(finalV1X, finalV1Y)
          f2.SpeedVector <- PointF(finalV2X, finalV2Y)
          true
      else false


  member this.PointDistance (p1:PointF, p2:PointF) = single (System.Math.Sqrt((double (p1.X - p2.X)) * (double (p1.X - p2.X)) + (double (p1.Y - p2.Y)) * (double (p1.Y - p2.Y))))
  member this.Terminate () = timer.Stop()
  member this.TimerStart = timer.Start()
  member this.TimerStop = timer.Stop()
  member val currentColorContainer : LWColorContainer option = None with get, set
  member val drawingEnabled = true with get, set
  
  member this.SelectedDraw
    with get() = selectedDraw
    and set(v) = selectedDraw <- v; this.Invalidate()

  override this.OnPaint e =       
    let g = e.Graphics
    
    g.SmoothingMode <- Drawing2D.SmoothingMode.HighQuality

    base.OnPaint(e)

    if dragStart.IsSome && this.drawingEnabled then
      use brush = new SolidBrush(selectedColor)
      match selectedDraw with
        | CIRCLE -> g.FillEllipse(brush, mkRectangleF(dragStart.Value, position) |> toRectangle)
        | RECTANGLE -> g.FillRectangle(brush, mkRectangleF(dragStart.Value, position) |> toRectangle)
        | TRIANGLE -> ()

    
  override this.OnMouseDown e =
    base.OnMouseDown e
    if not(this.LWMouseEventHandled) then
      if this.currentColorContainer.IsSome then 
        selectedColor <- this.currentColorContainer.Value.SelectedColor
        dragStart <- Some(e.Location |> toPointF)
        position <- dragStart.Value

  override this.OnMouseMove e =
    base.OnMouseMove e
    if not(this.LWMouseEventHandled) && dragStart.IsSome then
      position <- e.Location |> toPointF
      this.Invalidate()

  override this.OnMouseUp e =
    if not(this.LWMouseEventHandled) then
      if selectedFigure.IsSome then selectedFigure.Value.isSelected <- false; selectedFigure <- None
      base.OnMouseUp e

      if dragStart.IsSome then
        position <- e.Location |> toPointF
        if not(dragStart.Value.Equals(position)) && this.drawingEnabled then
          let figureDimension = mkRectangleF(dragStart.Value, position)
          let figure = new LWFigure(selectedDraw, selectedColor)
          figure.LWParent <- this
          figure.Location <- this.transform.TransformPointVW(Point(int figureDimension.X, int figureDimension.Y))
          figure.Size <- SizeF(figureDimension.Width, figureDimension.Height)
          this.LWControls.Add(figure)
          selectedFigure <- Some (figure)
        else 
          let mutable found = false
          for i in { (this.LWControls.Count - 1) .. -1 .. 0 } do
            if not found then
              let c = this.LWControls.[i] :?> LWFigure
              let t = new VWCoordinates()
              t.Translate(single c.Location.X, single c.Location.Y)
              t.Multiply(c.transform)
              let p = t.TransformPointVW(e.Location)
              if c.HitTest(p) then
                found <- true
                c.isSelected <- true
                selectedFigure <- Some(c)

        dragStart <- None
        this.Invalidate()
      this.LWMouseEventHandled <- true

      
    member this.TranslateFigures (tx:single, ty:single) =
      this.LWControls |> Seq.iter (fun con ->
        let c = con :?> LWFigure
        c.transform.Translate(tx, ty)
      )

    member this.RotateFigures (angle:single) =
      this.LWControls |> Seq.iter (fun con ->
        let c = con :?> LWFigure
        c.transform.Rotate(angle)
      )
      
    member this.ScaleFigures (sx:single, sy:single) =
      this.LWControls |> Seq.iter (fun con ->
        let c = con :?> LWFigure
        c.transform.Scale(sx, sy)
      )
      

    override this.OnKeyDown e =

      if selectedFigure.IsSome then
        if not this.drawingEnabled then
          match e.KeyCode with
          | Keys.R -> this.LWControls.Remove(selectedFigure.Value) |> ignore; this.Invalidate()
          | Keys.W -> selectedFigure.Value.IncreaseSpeed
          | Keys.S -> selectedFigure.Value.DecreaseSpeed
          | Keys.A -> selectedFigure.Value.TurnLeft
          | Keys.D -> selectedFigure.Value.TurnRight
          | _ -> ()
        else 
          match e.KeyCode with
          | Keys.R -> this.LWControls.Remove(selectedFigure.Value) |> ignore; this.Invalidate()
          | Keys.W -> selectedFigure.Value.Move(PointF(0.f, -4.f))
          | Keys.S -> selectedFigure.Value.Move(PointF(0.f, 4.f))
          | Keys.A -> selectedFigure.Value.Move(PointF(-4.f, 0.f))
          | Keys.D -> selectedFigure.Value.Move(PointF(4.f, 0.f))
          | _ -> ()
