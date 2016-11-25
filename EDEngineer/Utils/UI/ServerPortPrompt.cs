using System;
using System.Drawing;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Utils.System;

namespace EDEngineer.Utils.UI
{
    public static class ServerPortPrompt
    {
        public static bool ShowDialog(ushort current, out ushort port)
        {
            var translator = Languages.Instance;

            var autoRunBox = new CheckBox()
            {
                Checked = SettingsManager.AutoRunServer,
                Height = 50,
                Text = translator.Translate("Auto run server on EDEnginner startup with this port? (if not ticked, the server will stop when you unlock/lock the window)"),
                Dock = DockStyle.Top
            };

            var textBox = new NumericUpDown()
            {
                TextAlign = HorizontalAlignment.Center,
                Minimum = 1025,
                Maximum = 65535,
                Value = current,
                Dock = DockStyle.Top
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

            var buttonsPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 50,
            };

            buttonsPanel.Controls.Add(buttonOk);
            buttonsPanel.Controls.Add(buttonCancel);

            var f = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Width = 400,
                Height = textBox.Height + autoRunBox.Height + buttonsPanel.Height + 30,
                Text = translator.Translate("Select the port for the local API server")
            };

            f.Controls.Add(autoRunBox);
            f.Controls.Add(textBox);
            f.Controls.Add(buttonsPanel);

            if (f.ShowDialog() == DialogResult.OK)
            {
                if (ushort.TryParse(textBox.Text, out port))
                {
                    SettingsManager.AutoRunServer = autoRunBox.Checked;
                    return true;
                }
            }

            port = 0;
            return false;
        }
    }
}