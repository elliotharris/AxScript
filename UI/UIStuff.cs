using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using axScript3;
using System.Windows.Forms;
using System.Drawing;

namespace UI
{
    public class UIStuff
    {
        [ExportAsAxFunction("form")]
        public static Form NewForm()
        {
            return new Form();
        }

        [ExportAsAxFunction("form_add")]
        public static void AddControl(Form f, Control b)
        {
            f.Controls.Add(b);
        }

        [ExportAsAxFunction("textbox")]
        public static TextBox NewTextBox(String Text, double X, double Y, double W, double H)
        {
            return new TextBox() { Text = Text, Location = new Point((int)X, (int)Y), Size = new Size((int)W, (int)H) };
        }

        [ExportAsAxFunction("button")]
        public static Button NewButton(String Text, double X, double Y, double W, double H)
        {
            return new Button() { Text = Text, Location = new Point((int)X, (int)Y), Size = new Size((int)W, (int)H) };
        }
    }
}
