module CollisionForm

open LWC
open LWPanel
open LWLabel
open LWDrawings
open LWRoundButton
open LWColorContainer
open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D


type collisionForm(f:Form) as this =
    inherit LWContainer()

    let topPanel = new LWPanel(Color.DarkCyan, Location = PointF(0.f, 0.f), Size = SizeF(680.f, 120.f), Parent = this)
    let bottomPanel = new LWPanel(Color.Gray, Location = PointF(5.f, 11.f), Size = SizeF(275.f, 100.f), LWParent = topPanel)
    let drawingPanel = new LWPanel(Color.White, Parent = this, Location = PointF(0.f, 120.f), Size = SizeF(680.f, 370.f))
    
    let lab = new LWLabel(LWParent = drawingPanel, Location = PointF(260.f, 3.f), Size = SizeF(140.f, 100.f), Text = "disegna!")
    let lab2 = new LWLabel(LWParent = bottomPanel, Location = PointF(3.f, 3.f), Size = SizeF(269.f, 100.f), Text = "Color [A=255, R=0, G=0, B=0]")
    let lab3 = new LWLabel(LWParent = bottomPanel, Location = PointF(3.f, 16.f), Size = SizeF(269.f, 100.f), Text = "Forma Corrente: Cerchio!")

    let draw = new LWDrawings(Location = PointF(3.f, 3.f), Size = SizeF(674.f, 364.f), LWParent = drawingPanel)
    
    let butUP = new LWRoundButton(topPanel, Location = PointF(306.f, 8.f), Size = SizeF(42.f, 13.f), Text = "˄")
    let butDW = new LWRoundButton(topPanel, Location = PointF(306.f, 36.f), Size = SizeF(40.f, 13.f), Text = "˅")
    let butDX = new LWRoundButton(topPanel, Location = PointF(327.f, 22.f), Size = SizeF(40.f, 13.f), Text = "˃")
    let butSX = new LWRoundButton(topPanel, Location = PointF(284.f, 22.f), Size = SizeF(42.f, 13.f), Text = "˂")
    let butRuoO = new LWRoundButton(topPanel, Location = PointF(284.f, 60.f), Size = SizeF(40.f, 20.f), Text = "Rot")
    let butRuoA = new LWRoundButton(topPanel, Location = PointF(326.f, 60.f), Size = SizeF(40.f, 20.f), Text = "Anti Rot")
    let butScalaP = new LWRoundButton(topPanel, Location = PointF(284.f, 90.f), Size = SizeF(40.f, 20.f), Text = "+")
    let butScalaM = new LWRoundButton(topPanel, Location = PointF(326.f, 90.f), Size = SizeF(40.f, 20.f), Text = "-")
    
    let butRect = new LWRoundButton(bottomPanel, Location = PointF(10.f, 33.f), Size = SizeF(125.f, 30.f), Text = "Disegna Rettangolo")
    let butCerc = new LWRoundButton(bottomPanel, Location = PointF(136.f, 33.f), Size = SizeF(125.f, 30.f), Text = "Disegna Cerchio")
    let butDraw = new LWRoundButton(bottomPanel, Location = PointF(10.f, 64.f), Size = SizeF(125.f, 30.f), Text = "Disegna")
    let butRun = new LWRoundButton(bottomPanel, Location = PointF(136.f, 64.f), Size = SizeF(125.f, 30.f), Text = "Avvia!")
    let colors = new LWColorContainer(20, 8, Location = PointF(370.f, 10.f), Size = SizeF(300.f, 100.f), LWParent = topPanel)
    
    // list for easy resizing
    let ControlList : array<LWC> = [| topPanel; bottomPanel; drawingPanel; lab; lab2; lab3; draw; butUP; butDW; butDX;
                                     butSX; butRuoO; butRuoA; butScalaP; butScalaM; butRect; butCerc; butDraw; butRun; colors |]
    let mutable oldSize : Size = f.Size
    do 
      
      // WASD for moving, delete with R
      this.LWControls.AddRange([| topPanel ; drawingPanel |])
      drawingPanel.LWControls.AddRange([| draw ; lab |])
      topPanel.LWControls.AddRange([| bottomPanel ; colors ; butUP ; butDW; butDX ; butSX ; butRuoO ; butRuoA ; butScalaP ; butScalaM |])
      bottomPanel.LWControls.AddRange([| lab2 ; lab3 ; butRect ; butCerc ; butDraw ; butRun|])
      draw.currentColorContainer <- Some(colors)

      butRect.MouseUp.Add(fun e -> draw.SelectedDraw <- RECTANGLE; lab3.Text <- "Forma Corrente: Rettangolo!")
      butCerc.MouseUp.Add(fun e -> draw.SelectedDraw <- CIRCLE; lab3.Text <- "Forma Corrente: Cerchio!")
      colors.MouseDown.Add(fun e -> lab2.Text <- colors.SelectedColor.ToString())
      butDraw.MouseUp.Add(fun e -> lab.Text <- "Disegna!"; draw.TimerStop; draw.drawingEnabled <- true)
      butRun.MouseUp.Add(fun e -> lab.Text <- "Animazione Avviata!"; draw.TimerStart; draw.drawingEnabled <- false)
      
      butUP.MouseUp.Add(fun e -> draw.TranslateFigures(0.f, -5.f))
      butDW.MouseUp.Add(fun e -> draw.TranslateFigures(0.f, 5.f))
      butDX.MouseUp.Add(fun e -> draw.TranslateFigures(5.f, 0.f))
      butSX.MouseUp.Add(fun e -> draw.TranslateFigures(-5.f, 0.f))
      butRuoA.MouseUp.Add(fun e -> draw.RotateFigures(-5.f))
      butRuoO.MouseUp.Add(fun e -> draw.RotateFigures(5.f))
      butScalaM.MouseUp.Add(fun e -> draw.ScaleFigures(1.f/1.1f, 1.f/1.1f))
      butScalaP.MouseUp.Add(fun e -> draw.ScaleFigures(1.1f, 1.1f))

    member this.Close =
      draw.Terminate()

    member this.Readapt (newSize:Size)=
      ControlList |> Seq.iter (fun c -> 
        let (proportion:SizeF) = SizeF((single newSize.Width) / (single oldSize.Width), (single newSize.Height) / (single oldSize.Height))
        
        c.Size <- SizeF(proportion.Width * c.Size.Width, proportion.Height * c.Size.Height)
        c.Location <- PointF(proportion.Width * c.Location.X, proportion.Height * c.Location.Y)       
      )
      oldSize <- newSize

