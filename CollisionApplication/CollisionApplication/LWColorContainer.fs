module LWColorContainer

open VWCoordinates
open LWC
open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

type LWColor() = 
    inherit LWC()
    
    let outBorderPen = new Pen(Color.Black)
    let pressedInBorderPen = new Pen(Color.White)
    let unpressedInBorderPen = new Pen(Color.Gray)
    let mutable colorBrush = new SolidBrush(Color.Black)
    let mutable isPressed = false

    member this.IsPressed
      with get() = isPressed
      and set(v) = isPressed <- v; this.Invalidate()

    member this.ColorBrush
      with get() = colorBrush.Color
      and set(v) = colorBrush <- new SolidBrush(v)

    override this.OnPaint(e) =
      let g = e.Graphics
      
      g.DrawRectangle(outBorderPen, 0.f, 0.f, this.Size.Width, this.Size.Height)
      if isPressed then g.DrawRectangle(pressedInBorderPen, 1.f, 1.f, this.Size.Width - 2.f, this.Size.Height - 2.f)
      else g.DrawRectangle(unpressedInBorderPen, 1.f, 1.f, this.Size.Width - 2.f, this.Size.Height - 2.f)
      g.FillRectangle(colorBrush, 1.5f, 1.5f, this.Size.Width - 3.f, this.Size.Height - 3.f)
   

type LWColorContainer(RawElement, RawNumber) as this =
    inherit LWC()

    let colors = new ResizeArray<LWColor>()
    let rawElement = RawElement
    let rawNumber = RawNumber
    let mutable oldSize = this.Size
    let mutable selectedColor : LWColor option = None
    

    let correlate (e:MouseEventArgs) =
      let mutable found = false

      for i in { (colors.Count - 1) .. -1 .. 0 } do
        if not found then
          let c = colors.[i]

          if c.HitTest(PointF(single e.Location.X - c.Location.X, single e.Location.Y - c.Location.Y)) then
            found <- true
            c.IsPressed <- true
            selectedColor <- Some(c)


    do
      for i in 0 .. rawNumber - 1 do
        for j in 0 .. rawElement - 1 do
          let magicNumber = (single (j + rawElement * i)) / single (rawNumber * rawElement)
          
          // Sfumatura alternativa
          //let r = if magicNumber < 0.5f then (1.f - magicNumber) * 255.f else 0.f
          //let g = if magicNumber < 0.75f && magicNumber > 0.25f then abs (0.5f - magicNumber) * 255.f else 0.f
          //let b = if magicNumber > 0.5f then (1.f - magicNumber) * 255.f else 0.f
          let r, g, b = (1.f - magicNumber) * 255.f, abs (0.5f - magicNumber) * 255.f, magicNumber * 255.f
          
          let grayScale = int (255.f * (single i) / single (rawNumber - 1))
          if j = 0 then colors.Add(new LWColor(ColorBrush = Color.FromArgb(255, grayScale, grayScale, grayScale), LWParent = this))
          else colors.Add(new LWColor(ColorBrush = Color.FromArgb(255, int r, int g, int b), LWParent = this))

          
    member val LWMouseEventHandled = false with get, set

    override this.OnPaint(e) =
      let g = e.Graphics

      let blockWidth, blockHeight = this.Size.Width / single rawElement, this.Size.Height / single rawNumber

      for i in 0 .. rawNumber - 1 do
        for j in 0 .. rawElement - 1 do
          let col = colors.[j + rawElement * i]

          let s = g.Save()
        
          let t = g.Transform.Clone()
          t.Multiply(col.transform.WV)
          g.Transform <- t
          
          g.TranslateTransform(col.Location.X, col.Location.Y)

          if oldSize <> this.Size then 
            col.Location <- PointF(blockWidth * single j, blockHeight * single i)
            col.Size <- SizeF(blockWidth, blockHeight)

          let clip = g.Clip.Clone()
          clip.Intersect(RectangleF(0.f, 0.f, col.Size.Width + 1.f, col.Size.Height + 1.f))
          g.Clip <- clip
          let r = g.ClipBounds

          let evt = new PaintEventArgs(g, new Rectangle(int(r.Left), int(r.Top), int(r.Width), int(r.Height)))
          col.OnPaint evt
          g.Restore(s)

      oldSize <- this.Size

    member this.SelectedColor 
      with get() = if selectedColor.IsSome then selectedColor.Value.ColorBrush
                   else Color.Black

    override this.OnMouseDown(e) = 
      if selectedColor.IsSome then
        selectedColor.Value.IsPressed <- false
        selectedColor <- None
      
      correlate e
      base.OnMouseDown(e)

