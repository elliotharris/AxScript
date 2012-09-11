using System;
using System.Drawing;
using System.Windows.Forms;
using axScript3;

namespace UI
{
    public class UIStuff
    {
        [ExportAx("form", "Creates and returns a new form.")]
        public static Form NewForm()
        {
            return new Form();
        }

        [ExportAx("textbox", "Creates and returns a new textbox")]
        public static TextBox NewTextBox(String Text, double X, double Y, double W, double H)
        {
            return new TextBox {Text = Text, Location = new Point((int) X, (int) Y), Size = new Size((int) W, (int) H)};
        }

        [ExportAx("button", "Creates and returns a new button")]
        public static Button NewButton(String Text, double X, double Y, double W, double H)
        {
            return new Button {Text = Text, Location = new Point((int) X, (int) Y), Size = new Size((int) W, (int) H)};
        }
    }
}