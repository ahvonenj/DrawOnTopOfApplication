using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawOnTopOfApplication
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private Process proc = new Process();
        private IntPtr procHandle;
        private RECT procRect;

        private int initialStyle;


        int GameX = 1;
        int GameY = 1;
        float ScreenRatioX = 1;
        float ScreenRatioY = 1;

        Pen myPen = new Pen(Color.Red, 2);
        Pen curvePen = new Pen(Color.Black, 2);

        bool Startup = true;

        int showAngle;
        float angle = 60;
        float radians;
        float Velocity = 45;
        float y = 1;
        int x;
        float a;
        float c = 1;

        bool underGroundShot = false;

        private const int WM_HOTKEY = 0x312;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            try
            {
                proc = Process.GetProcessesByName("Notepad").First();
                procHandle = proc.MainWindowHandle;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Process not running");
                Console.WriteLine(ex.Message);
            }

            SetForegroundWindow(procHandle);
            initialStyle = GetWindowLong(procHandle, -20);
            //SetWindowLong(procHandle, -20, initialStyle != null || unchecked((int)0x80000) || unchecked((int)0x20));
            SetWindowLong(procHandle, -20, initialStyle);

            BornNext(this);

            /*RegisterHotKey(Handle, 101, 0, (uint)Keys.Tab);
            RegisterHotKey(Handle, 102, 0, (uint)Keys.NumPad2);

            RegisterHotKey(Handle, 104, 0, (uint)Keys.NumPad4);
            RegisterHotKey(Handle, 105, 0, (uint)Keys.NumPad5);
            RegisterHotKey(Handle, 106, 0, (uint)Keys.NumPad6);

            RegisterHotKey(Handle, 108, 0, (uint)Keys.NumPad8);*/

            timer1.Start();
        }

        public void BornNext(Form form)
        {
            GameX = Math.Abs(procRect.Right - procRect.Left);
            GameY = Math.Abs(procRect.Top - procRect.Bottom);
            form.Size = new Size(GameX - 16, GameY - 30);
            form.Location = new Point(procRect.Left + 8, procRect.Top + 30);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetWindowRect(procHandle, out procRect);
            BornNext(this);
            ScreenRatioX = GameX / 1280;
            ScreenRatioY = GameY / 759;


            StringBuilder sb = new StringBuilder(0, 100);

            GetWindowText(GetForegroundWindow(), sb, 100);

            if(sb.ToString().Contains("Untitled - Notepad"))
            {
                TopMost = true;

                if(Startup)
                {
                    WindowState = FormWindowState.Normal;
                    Startup = false;
                }
            }
            else
            {
                TopMost = false;
            }

            if (GetAsyncKeyState(Keys.RButton) == 1)
            {
                MyPlayer.X = Cursor.Position.X - Left;
                MyPlayer.Y = -(Cursor.Position.Y - Top) + Height;
                ArrowBot();
            }
        }

        private void ArrowBot()
        {
            try
            {
                if (angle == 90)
                    return;

                Graphics myGraphics = CreateGraphics();
                myGraphics.Clear(Color.FromArgb(255, 1, 1, 1));
                myGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                myGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

                if (angle > 90)
                {
                    angle = 180 - angle;
                }
                else if (angle == 0)
                {
                    angle = 180;
                }

                radians = (float)(angle * Math.PI / 180);

                MaxHeightPoint.Y = (float)(Velocity * Velocity * Math.Sin(radians) * Math.Sin(radians) / 20 * 1.67 * ScreenRatioX); //1.68
                a = (float)(4 * MaxHeightPoint.Y / Math.Tan(radians));
                c = (float)(-Math.Tan(radians) / a * 1.02);

                MaxHeightPoint.X = MyPlayer.X + a / 2;

                if (underGroundShot)
                    c = -c;

                x = 0;

                Point[] Points = new Point[Width];

                while (x < Width)
                {
                    y = c * ((x - MyPlayer.X) - a) * (x - MyPlayer.X);   //curve, where the magic happens 

                    try
                    {
                        Points[x] = new Point(x, (int)(-y + MyPlayer.YtoScreen(this)));
                    }
                    catch (Exception ex)
                    {
                        x = Width;
                    }

                    x++;
                }

                // point edit
                int MinPoint;
                int MaxPoint;

                if (MaxHeightPoint.X > MyPlayer.X)
                {
                    MinPoint = (int)MyPlayer.X; // 4000- MyPlayer.X
                    MaxPoint = Width;
                }
                else
                {
                    MinPoint = 0; // MyPlayer.X+4000
                    MaxPoint = (int)MyPlayer.X;
                }

                Point[] PointsNot = new Point[MaxPoint - MinPoint];

                try
                {
                    x = 0;

                    if (MaxHeightPoint.X > MyPlayer.X)
                    {
                        while (x < MaxPoint - MinPoint)
                        {
                            PointsNot[x] = Points[Width - x];
                            x = x + 1;
                        }
                    }
                    else
                    {

                        while (x < MaxPoint - MinPoint)
                        {
                            PointsNot[x] = Points[x];
                            x = x + 1;
                        }
                    }
                }
                catch (Exception ex2)
                {

                }

                // point edit
                try
                {
                    myGraphics.DrawCurve(curvePen, PointsNot);
                }
                catch (Exception ex3)
                {

                }

                lblPlayer.Size = new Size((int)(45 * ScreenRatioX), 20);
                lblPlayer.Location = new Point((int)(MyPlayer.X - lblPlayer.Size.Width / 2), (int)(MyPlayer.YtoScreen(this) + 21 * ScreenRatioX));

                //Form2.lblSolution.Text = Velocity.ToString + ", " + showAngle.ToString
                lblSolution.Text = Velocity.ToString() + ", " + showAngle.ToString();
                lblSolution.Location = new Point((int)(MyPlayer.X - 18), (int)(MyPlayer.YtoScreen(this) - 90 * ScreenRatioX));

                lblSolution.Text = GetAsyncKeyState(Keys.RButton).ToString();

                if (GetAsyncKeyState(Keys.RButton) == 1)
                {
                    
                    myGraphics.DrawLine(curvePen, MyPlayer.X - 80, MyPlayer.YtoScreen(this) - 80, MyPlayer.X + 80, MyPlayer.YtoScreen(this) + 80);
                    myGraphics.DrawLine(curvePen, MyPlayer.X - 80, MyPlayer.YtoScreen(this) + 80, MyPlayer.X + 80, MyPlayer.YtoScreen(this) - 80);
                    myGraphics.DrawLine(curvePen, MyPlayer.X, MyPlayer.YtoScreen(this) - 113, MyPlayer.X, MyPlayer.YtoScreen(this) + 113);
                    myGraphics.DrawLine(curvePen, MyPlayer.X - 113, MyPlayer.YtoScreen(this), MyPlayer.X + 113, MyPlayer.YtoScreen(this));
                }
                else
                {
                    myGraphics.DrawLine(curvePen, MyPlayer.X, MyPlayer.YtoScreen(this) - 30, MyPlayer.X, MyPlayer.YtoScreen(this) + 30);
                    myGraphics.DrawLine(curvePen, MyPlayer.X - 30, MyPlayer.YtoScreen(this), MyPlayer.X + 30, MyPlayer.YtoScreen(this));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //UnregisterHotKey(Handle, (int)Keys.Tab);
            //UnregisterHotKey(Handle, (int)Keys.NumPad2);
            //UnregisterHotKey(Handle, (int)Keys.NumPad4);
            //UnregisterHotKey(Handle, (int)Keys.NumPad5);
            //UnregisterHotKey(Handle, (int)Keys.NumPad6);
            //UnregisterHotKey(Handle, (int)Keys.NumPad8);

            /*UnregisterHotKey(Handle, 101);
            UnregisterHotKey(Handle, 102);
            UnregisterHotKey(Handle, 104);
            UnregisterHotKey(Handle, 105);
            UnregisterHotKey(Handle, 106);
            UnregisterHotKey(Handle, 108);*/
        }
    } 
}
