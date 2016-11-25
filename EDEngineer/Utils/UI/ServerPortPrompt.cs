using System.Drawing;
using System.Windows.Forms;
using EDEngineer.Localization;

namespace EDEngineer.Utils.UI
{
    public static class ServerPortPrompt
    {
        public static bool ShowDialog(ushort current, out ushort port)
        {
            var translator = Languages.Instance;

            var textBox = new NumericUpDown()
            {
                Width = 385,
                TextAlign = HorizontalAlignment.Center,
                Minimum = 1025,
                Maximum = 65535,
                Value = current
            };

            textBox.Validating += (o, e) =>
                                  {
                                      ushort p;
                                      if (!ushort.TryParse(textBox.Text, out p))
                                      {
                                          e.Cancel = true;
                                      }
                                  };

            var buttonOk = new Button()
            {
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Green,
                Text = translator.Translate("OK"),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Left
            };


            var buttonCancel = new Button()
            {
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.OrangeRed,
                Text = translator.Translate("Cancel"),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Right
            };

            var f = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Width = 400,
                Height = 100,
                Text = translator.Translate("Select the port for the local API server")
            };

            f.Controls.Add(textBox);
            f.Controls.Add(buttonOk);
            f.Controls.Add(buttonCancel);

            if (f.ShowDialog() == DialogResult.OK)
            {
                if (ushort.TryParse(textBox.Text, out port))
                {
                    return true;
                }
            }

            port = 0;
            return false;
        }
    }
}