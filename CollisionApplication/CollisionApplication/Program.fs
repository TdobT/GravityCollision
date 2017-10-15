//Altre informazioni su F# all'indirizzo http://fsharp.org
// Per ulteriori informazioni, vedere il progetto 'Esercitazione su F#'.

open CollisionForm
open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D

[<EntryPoint>]

let main argv = 
    
  let f = new Form(Dock = DockStyle.Fill, Text = "Simulatore Collisioni", BackColor = Color.Gray, Size = Size(697, 530), TopMost = true)
  f.Show()
  let c = new collisionForm(f, Dock = DockStyle.Fill)
  f.Controls.Add(c)
  f.Closing.Add(fun _ -> c.Close)
  f.ClientSizeChanged.Add(fun e -> c.Readapt(f.Size))
  c.Focus() |> ignore
  Application.Run(f)
  0
