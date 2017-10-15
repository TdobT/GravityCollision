module LWC


open VWCoordinates
open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D

type Figure =
    | RECTANGLE
    | CIRCLE
    | TRIANGLE

type ControlType =
    | DRAWING of Figure
    | BUTTON
    | PANEL
    | NONE

type LWC() =

  let mutable parent : Control = null
  let mutable LWparent : LWC option = None
  let mutable location = PointF()
  let mutable size = SizeF()
  
  let mouseDown = Event<MouseEventArgs>()
  let mouseMove = Event<MouseEventArgs>()
  let mouseUp = Event<MouseEventArgs>()
  
  member val isMouseHover = false with get, set
  member val controlType = NONE with get, set
  member val transform = new VWCoordinates() with get, set
  member val MouseEventHandled = false with get, set
  
  member this.MouseDown with get() = mouseDown.Publish
  member this.MouseMove with get() = mouseMove.Publish
  member this.MouseUp with get() = mouseUp.Publish

  abstract OnMouseDown : MouseEventArgs -> unit
  default this.OnMouseDown e = mouseDown.Trigger(e)

  abstract OnMouseMove : MouseEventArgs -> unit
  default this.OnMouseMove e = mouseMove.Trigger(e)

  abstract OnMouseUp : MouseEventArgs -> unit
  default this.OnMouseUp e = mouseUp.Trigger(e)
  
  abstract OnKeyDown : KeyEventArgs -> unit
  default this.OnKeyDown _ = ()
  
  abstract OnMouseEnter : System.EventArgs -> unit
  default this.OnMouseEnter _ = ()
  
  abstract OnMouseLeave : System.EventArgs -> unit
  default this.OnMouseLeave _ = ()

  abstract OnMouseHover : System.EventArgs -> unit
  default this.OnMouseHover _ = ()

  abstract OnPaint : PaintEventArgs -> unit
  default this.OnPaint _ = ()

  abstract HitTest : PointF -> bool
  default this.HitTest p =
    (RectangleF(PointF(), this.Size)).Contains(p)

  member this.Invalidate() =
    if parent <> null then parent.Invalidate()
    else if LWparent <> None then LWparent.Value.Invalidate()

  member this.Location
    with get() = location
    and set(v) = location <- v; this.Invalidate()

  member this.Size
    with get() = size
    and set(v) = size <- v; this.Invalidate()
    
  member this.Parent
    with get() = parent
    and set(v) = 
      parent <- v
      this.transform <- new VWCoordinates()
    
  member this.LWParent
    with get() = LWparent.Value
    and set(v) = 
      LWparent <- Some(v)
      this.transform <- new VWCoordinates()
      this.transform.Multiply(v.transform.Clone())

  member this.GetFont =
    if parent <> null then parent.Font
    else LWparent.Value.GetFont


type LWContainer() =
  inherit UserControl()

  let controls = ResizeArray<LWC>()

  let cloneMouseEvent (c:LWC) (e:MouseEventArgs) =
    let t = new VWCoordinates()
    t.Translate(single c.Location.X, single c.Location.Y)
    t.Multiply(c.transform)
    let p = t.TransformPointVW(e.Location)
    new MouseEventArgs(e.Button, e.Clicks, int p.X, int p.Y, e.Delta)
 
         
  let correlate (e:MouseEventArgs) (f:LWC->MouseEventArgs->unit) =
    let mutable found = false

    for i in { (controls.Count - 1) .. -1 .. 0 } do
      if not found then
        let c = controls.[i]
        let relativeEvent = cloneMouseEvent c e
        if c.HitTest(PointF(single(relativeEvent.Location.X), single(relativeEvent.Location.Y))) then
          if not c.isMouseHover then 
            c.isMouseHover <- true
            c.OnMouseEnter(System.EventArgs.Empty)
          f c relativeEvent
          found <- true
        else 
          if c.isMouseHover then c.isMouseHover <- false; c.OnMouseLeave(System.EventArgs.Empty)
    

  let mutable captured : LWC option = None
  
  do 
    base.SetStyle(ControlStyles.AllPaintingInWmPaint ||| ControlStyles.OptimizedDoubleBuffer, true)

  member this.LWControls = controls
  member val LWMouseEventHandled = false with get, set
  
  override this.OnMouseDown e =
    this.LWMouseEventHandled <- false
    correlate e (fun c ev -> captured <- Some(c); c.OnMouseDown(ev); this.LWMouseEventHandled <- c.MouseEventHandled; c.MouseEventHandled <- false)
    base.OnMouseDown e

  override this.OnMouseUp e =
    this.LWMouseEventHandled <- false
    correlate e (fun c ev -> c.OnMouseUp(ev); this.LWMouseEventHandled <- c.MouseEventHandled; c.MouseEventHandled <- false)
    base.OnMouseUp e

  override this.OnMouseMove e =
    this.LWMouseEventHandled <- false
    correlate e (fun c ev -> c.OnMouseMove(ev); this.LWMouseEventHandled <- c.MouseEventHandled; c.MouseEventHandled <- false)
    base.OnMouseMove e

  override this.OnKeyDown e =
    for i in { (controls.Count - 1) .. -1 .. 0 } do
        let c = controls.[i]
        c.OnKeyDown(e)

  override this.OnPaint e =
    controls |> Seq.iter (fun c ->
      let g = e.Graphics

      let s = g.Save()
      
      let t = g.Transform.Clone()
      t.Multiply(c.transform.WV)
      g.Transform <- t
      g.TranslateTransform(c.Location.X, c.Location.Y)
      
      let clip = g.Clip.Clone()
      clip.Intersect(RectangleF(0.f, 0.f, c.Size.Width + 1.f, c.Size.Height + 1.f))
      g.Clip <- clip
      let r = g.ClipBounds

      let evt = new PaintEventArgs(g, new Rectangle(int(r.Left), int(r.Top), int(r.Width), int(r.Height)))
      c.OnPaint evt
      g.Restore(s)
    )
    base.OnPaint(e)