using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace Stalker_Studio.StalkerClass
{

    public class DDSImage : IDisposable
    {
        private bool m_isValid = false;
        private System.Drawing.Bitmap m_bitmap = null;

        public bool IsValid { get { return m_isValid; } }

        public System.Drawing.Bitmap BitmapImage
        {
            get
            {
                return m_bitmap;
            }
        }


        public DDSImage(byte[] ddsImage)
        {
            Initializer(ddsImage);

        }

        private void Initializer(byte[] ddsImage)
        {
            byte[] dds = ddsImage;
            string workDir = $"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\python\\";
            try
            {
                if (System.IO.File.Exists(workDir + "\\tmp.png"))
                    System.IO.File.Delete(workDir + "\\tmp.png");
                if (System.IO.File.Exists(workDir + "\\tmpDDS.png"))
                    System.IO.File.Delete(workDir + "\\tmpDDS.png");
                if (System.IO.File.Exists(workDir + "\\tmp.dds"))
                    System.IO.File.Delete(workDir + "\\tmp.dds");
            }
            catch (IOException g)
            {
                //  System.Threading.Thread.Sleep(150);
                // Initializer(dds);

            }

            File.WriteAllBytes(workDir + "\\tmp.dds", dds);

            System.IO.File.WriteAllText($"{workDir}\\bat.bat", $"@chcp 1251{Environment.NewLine}cd {workDir}{Environment.NewLine}DDS.exe dtx3 png tmp.dds tmpDDS.png", Encoding.Default);

            ProcessStartInfo proc = new ProcessStartInfo($"{workDir}\\bat.bat") { CreateNoWindow = false, UseShellExecute = false, RedirectStandardOutput = true };
            Console.ForegroundColor = ConsoleColor.Red;
            Process prs = Process.Start(proc);

            string txt = prs.StandardOutput.ReadToEnd();
            Console.ForegroundColor = ConsoleColor.White;
            if (txt.Contains("code: 0"))
            {
                try
                {
                    byte[] img = File.ReadAllBytes(workDir + "\\tmpDDS.png");
                    MemoryStream stream = new MemoryStream(img, 0, img.Length, true, true);
                    this.m_bitmap = new Bitmap(stream);
                    stream.Close();
                    stream.Dispose();
                    this.m_isValid = true;
                }
                catch(Exception g)
                {
                    System.Windows.Forms.MessageBox.Show($"{g.Message}\n{{" + g + "}", "ERROR [Stalker_Studio.StalkerClass.DDSImage]", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            else
            {
                this.m_isValid = false;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("_______________END PYTHON TERMINAL_______________");
                Console.ForegroundColor = ConsoleColor.White;
                System.Windows.Forms.MessageBox.Show($"\t[Python Modules]\n\n\tView in the console\n\nCheck if the data is entered correctly. (tmp.dds)", "UNKNOWN ERROR [Stalker_Studio.StalkerClass.DDSImage]", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                
            }
            //   m_bitmap.Dispose();
        }



        private static Bitmap cropImage(Image img, Rectangle cropArea)
        {
            if (img != null)
            {
                Bitmap bmpImage = new Bitmap(img);
                return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            }
            return null;
        }

        public static int S_X = 50;
        public static int S_Y = 50;
        public static int S_W = 50;
        public static int S_H = 50;



        public static Bitmap GetIcon(Bitmap bit, int X, int Y, int Width, int Height)
        {
            if (bit == null)
                return null;



            int _X = 0;
            int _Y = 0;
            int _W = 0;
            int _H = 0;
            try
            {

                for (int i = 0; i < X; i++)
                {
                    _X += S_X;
                }
                for (int i = 0; i < Y; i++)
                {
                    _Y += S_Y;
                }
                for (int i = 0; i < Width; i++)
                {
                    _W += S_W;
                }
                for (int i = 0; i < Height; i++)
                {
                    _H += S_H;
                }

                while ((_X + _W) > bit.Width)
                {
                    if (_W < 0)
                        return null;
                    _W--;
                }
                while ((_Y + _H) > bit.Height)
                {
                    if (_H < 0)
                        return null;
                    _H--;
                }


              //  Console.WriteLine($"Stalker_Studio.StalkerClass.DDSImage: X: {_X} | Y: {_Y} | W: {_W} | H: {_H}");
                return cropImage(bit, new Rectangle(_X, _Y, _W, _H));
            }
            catch (Exception g)
            {
                System.Windows.Forms.MessageBox.Show($"{g.Message}\n_X: {_X}, _Y: {_Y}, _W: {_W}, _H: {_H}\n{bit.Width}x{bit.Height}", $"Ошибка! [StalkerWeaponToolKit.DDSImage => GetIcon(bit,X,Y,W,H)]", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return null;
        }

        private DDSImage(System.Drawing.Bitmap bitmap)
        {
            this.m_bitmap = bitmap;
        }


        public void Dispose()
        {
            this.m_bitmap.Dispose();

        }

    }

}
