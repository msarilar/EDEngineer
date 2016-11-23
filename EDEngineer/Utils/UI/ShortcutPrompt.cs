using System.Drawing;
using System.Windows.Forms;
using EDEngineer.Localization;

namespace EDEngineer.Utils.UI
{
    public static class ShortcutPrompt
    {
        public static bool ShowDialog(string current, out string shortcut)
        {
            var translator = Languages.Instance;

            var block = new TextBox()
            {
                Width = 385,
                TextAlign = HorizontalAlignment.Center
            };
            var converter = new KeysConverter();

            block.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Back || e.Modifiers == Keys.None)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    block.Text = "";
                }
                else
                {
                    block.Text = converter.ConvertToString(e.KeyData);
                }
            };

            shortcut = current;

            block.Text = converter.ConvertToString(shortcut);

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
                Text = translator.Translate("Input a shortcut that's going to be used to show the window")
            };

            f.Controls.Add(block);
            f.Controls.Add(buttonOk);
            f.Controls.Add(buttonCancel);

            if (f.ShowDialog() == DialogResult.OK)
            {
                Keys keys;
                if (string.IsNullOrEmpty(block.Text))
                {
                    MessageBox.Show(translator.Translate("You can't use an empty shortcut"), translator.Translate("Error"), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    keys = (Keys)converter.ConvertFromString(block.Text);
                }
                catch
                {
                    MessageBox.Show(string.Format(translator.Translate("The requested shortcut ({0}) is invalid"), block.Text), translator.Translate("Error"), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                if ((keys & ~Keys.Alt & ~Keys.Control & ~Keys.ControlKey & ~Keys.Shift & ~Keys.ShiftKey &
                     ~Keys.LShiftKey & ~Keys.RShiftKey & ~Keys.Menu) == Keys.None)
                {
                    MessageBox.Show(
                        translator.Translate("You must use a regular key to accompany the modifier key like Ctrl+F10 or Shift+R"), translator.Translate("Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                shortcut = converter.ConvertToString(keys);

                return true;
            }

            return false;
        }
    }
}