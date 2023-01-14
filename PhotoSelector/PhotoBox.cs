using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSelector
{
    public partial class PhotoBox : UserControl
    {
        private CheckBox CheckBox { get; set; }
        private Button PreviousButton { get; set; }
        private Button NextButton { get; set; }
        private PictureBox pictureBox1;

        public bool Checked { get { return _Checked; } set { _Checked = value; CheckBox.Checked = value; } }
        private bool _Checked { get; set; }
        public PhotoBox()
        {
            InitializeComponent();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(470, 409);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;


            CheckBox = new CheckBox();
            CheckBox.Checked = false;
            CheckBox.Size = new Size(15, 15);
            CheckBox.Location = new Point(3, 9);
            CheckBox.CheckedChanged += CheckBox_CheckedChanged;
            CheckBox.TabStop = false;

            PreviousButton = new Button();
            PreviousButton.Font = new Font(PreviousButton.Font.FontFamily, 12);
            PreviousButton.Text = "<";
            PreviousButton.Size = new Size(25, 25);
            PreviousButton.FlatStyle = FlatStyle.Popup;
            PreviousButton.Location = new Point(21, 3);
            PreviousButton.Click += PreviousButton_Click;
            PreviousButton.TabStop = false;

            NextButton = new Button();
            NextButton.Font = new Font(NextButton.Font.FontFamily, 12);
            NextButton.Text = ">";
            NextButton.Size = new Size(25, 25);
            NextButton.FlatStyle = FlatStyle.Popup;
            NextButton.Location = new Point(49, 3);
            NextButton.Click += NextButton_Click;
            NextButton.TabStop = false;

            this.Controls.Add(CheckBox);
            this.Controls.Add(PreviousButton);
            this.Controls.Add(NextButton);
            this.Controls.Add(pictureBox1);

        }


        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckChangedEventArgs args = new CheckChangedEventArgs();
            args.Checked = CheckBox.Checked;
            OnCheckBoxCheckChanged(args);
        }


        public event EventHandler<CheckChangedEventArgs> CheckBox_CheckChanged;
        protected virtual void OnCheckBoxCheckChanged(CheckChangedEventArgs e)
        {
            EventHandler<CheckChangedEventArgs> handler = CheckBox_CheckChanged;
            handler?.Invoke(this, e);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            OnNextButtonClick(e);
        }


        public event EventHandler NextButton_Clicked;

        protected virtual void OnNextButtonClick(EventArgs e)
        {
            EventHandler handler = NextButton_Clicked;
            handler?.Invoke(this, e);
        }


        private void PreviousButton_Click(object sender, EventArgs e)
        {
            OnPreviousButtonClick(e);
        }

        public event EventHandler PreviousButton_Clicked;
        private void OnPreviousButtonClick(EventArgs e)
        {
            EventHandler handler = PreviousButton_Clicked;
            handler?.Invoke(this, e);
        }
        //protected override void OnKeyUp(KeyEventArgs e)
        //{
        //    if (e.KeyData == Keys.Right || e.KeyData == Keys.Down)
        //    {
        //        OnNextButtonClick(e);
        //    }
        //    if (e.KeyData == Keys.Left || e.KeyData == Keys.Up)
        //    {
        //        OnPreviousButtonClick(e);
        //    }
        //    if (e.KeyData == Keys.Space)
        //    {
        //        CheckChangedEventArgs args = new CheckChangedEventArgs();
        //        args.Checked = CheckBox.Checked;
        //        OnCheckBoxCheckChanged(args);

        //    }
        //}

        private Image _Image { get; set; }
        public Image Image { get { return _Image; } set { _Image = value; UpdateImage(); } }

        public void UpdateImage()
        {
            this.pictureBox1.Image = _Image;
        }
    }

    public class CheckChangedEventArgs : EventArgs
    {
        public bool Checked { get; set; }
    }

}
