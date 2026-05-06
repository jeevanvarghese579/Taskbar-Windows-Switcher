using System;  
using System.Drawing;  
class Program { static void Main() { using (var bmp = new Bitmap(32, 32)) { using (var g = Graphics.FromImage(bmp)) { g.Clear(Color.FromArgb(255, 0, 120, 212)); using (var brush = new SolidBrush(Color.White)) using (var font = new Font("Arial", 14, FontStyle.Bold)) { g.DrawString("T", font, brush, 6, 4); } } bmp.Save("d:\\Veet projects\\MDS2\\TaskbarDesktopSwitcher\\icon.ico"); } } } 
