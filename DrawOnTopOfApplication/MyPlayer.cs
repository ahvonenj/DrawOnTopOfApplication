using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawOnTopOfApplication
{
    class MyPlayer
    {
        public static float X;
        public static float Y;

        public static float YtoScreen(Form1 form)
        {
            return -Y + form.Height;
        }
    }
}
