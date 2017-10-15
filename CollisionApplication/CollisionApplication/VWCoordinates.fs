module VWCoordinates

open System.Drawing.Drawing2D
open System.Drawing

type VWCoordinates() =
  let mutable wv = new Drawing2D.Matrix()
  let mutable vw = new Drawing2D.Matrix()

  member this.WV with get() = wv and set(v) = wv <- v
  member this.VW with get() = vw and set(v) = vw <- v

  member this.Clone() =
    let ret = VWCoordinates() 
    ret.WV <- wv.Clone()
    ret.VW <- vw.Clone()
    ret
    
  member this.Multiply(m:VWCoordinates, ?order:Drawing2D.MatrixOrder) =
    let wo = match order with Some(Drawing2D.MatrixOrder.Append) -> Drawing2D.MatrixOrder.Append | _ -> Drawing2D.MatrixOrder.Prepend
    let vo = match order with Some(Drawing2D.MatrixOrder.Prepend) -> Drawing2D.MatrixOrder.Prepend | _ -> Drawing2D.MatrixOrder.Append
    wv.Multiply(m.WV, wo)
    vw.Multiply(m.VW, vo)

  member this.Rotate(a:single) =
    wv.Rotate(a)
    vw.Rotate(-a, Drawing2D.MatrixOrder.Append)

  member this.RotateAt(a:single, p:PointF) =
    wv.RotateAt(a, p)
    vw.RotateAt(-a, p, Drawing2D.MatrixOrder.Append)

  member this.Translate(tx:single, ty:single) =
    wv.Translate(tx, ty)
    vw.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)

  member this.Scale(sx:single, sy:single) =
    wv.Scale(sx, sy)
    vw.Scale(1.f / sx, 1.f / sy, Drawing2D.MatrixOrder.Append)
    
  member this.TransformPointVW (p:Point) =
    let toPointF (p:Point) = PointF(single p.X, single p.Y)
    let a = [| p |> toPointF |]
    this.VW.TransformPoints(a)
    a.[0]

  member this.TransformPointVWF (p:PointF) =
    let a = [| p |]
    this.VW.TransformPoints(a)
    a.[0]

  member this.TransformPointWV (p:Point) =
    let toPointF (p:Point) = PointF(single p.X, single p.Y)
    let a = [| p |> toPointF |]
    this.WV.TransformPoints(a)
    a.[0]

  member this.ScaleAtV(sx:single, sy: single, cv:Point) =
      let cw = cv |> this.TransformPointVW
      this.Scale(sx, sy)
      let cwp = cv |> this.TransformPointVW
      this.Translate(cwp.X - cw.X, cwp.Y - cw.Y)


