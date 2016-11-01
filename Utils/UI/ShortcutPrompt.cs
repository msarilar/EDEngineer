using System.Drawing;
using System.Windows.Forms;

namespace EDEngineer.Utils.UI
{
    public static class ShortcutPrompt
    {
        public static bool ShowDialog(string current, out string shortcut)
        {
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
                Text = "OK",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Left
            };


            var buttonCancel = new Button()
            {
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.OrangeRed,
                Text = "Cancel",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Right
            };

            var f = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Width = 400,
                Height = 100,
                Text = "Input a shortcut that's going to be used to show the window"
            };

            f.Controls.Add(block);
            f.Controls.Add(buttonOk);
            f.Controls.Add(buttonCancel);

            if (f.ShowDialog() == DialogResult.OK)
            {
                Keys keys;
                if (string.IsNullOrEmpty(block.Text))
                {
                    MessageBox.Show("You can't use an empty shortcut", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    keys = (Keys)converter.ConvertFromString(block.Text);
                }
                catch
                {
                    MessageBox.Show($"The requested shortcut ({block.Text}) is invalid", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                if ((keys & ~Keys.Alt & ~Keys.Control & ~Keys.ControlKey & ~Keys.Shift & ~Keys.ShiftKey &
                     ~Keys.LShiftKey & ~Keys.RShiftKey & ~Keys.Menu) == Keys.None)
                {
                    MessageBox.Show(
                        "You must use a regular key to accompany the modifier key like (Ctrl+F10 or Shift+R)", "Error",
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