using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FirstAidQuiz.Controllers;
using FirstAidQuiz.Helpers;
using FirstAidQuiz.Properties;

namespace FirstAidQuiz {

    public partial class MainScreen : Form {
        private CancellationTokenSource mouseEnterCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource mouseLeaveCancellationTokenSource = new CancellationTokenSource();
        private bool shouldDisposeTokens = false;

        private readonly PictureBox title = new PictureBox { Name = "Header" }; // create a PictureBox
        private readonly Label minimise = new Label { Name = "Header" }; // this doesn't even have to be a label!
        private readonly Label maximise = new Label { Name = "Header" }; // this will simulate our this.maximise box
        private readonly Label close = new Label { Name = "Header" }; // simulates the this.close box
        private static readonly Color BannerColor = Color.FromArgb(255, 0, 102, 204);

        private bool drag = false; // determine if we should be moving the form
        private Point startPoint = new Point(0, 0); // also for the moving

        public Rectangle ScreenRectangle() {
            return Screen.FromControl(this).Bounds;
        }

        private ListViewController viewController = new ListViewController();

        public MainScreen() {
            this.InitializeComponent();
            this.BackColor = Color.FromArgb(60, 60, 60);
            this.FormBorderStyle = FormBorderStyle.None; // get rid of the standard title bar

            this.title.Location = this.Location; // assign the location to the form location
            this.title.Width = this.Width; // make it the same width as the form
            this.title.Height = 50; // give it a default height (you may want it taller/shorter)
            this.title.BackColor = BannerColor; // give it a default colour (or load an image)
            this.Controls.Add(this.title); // add it to the form's controls, so it gets displayed

            this.minimise.Text = Resources.Minimize; // Doesn't have to be
            this.minimise.Location = new Point(this.Location.X + this.Width / 3 * 0, this.title.Height - 37);
            this.minimise.ForeColor = Color.White; // Give it a colour that will make it stand out
            this.minimise.TextAlign = ContentAlignment.MiddleCenter;
            this.minimise.Font = new Font(FontFamily.GenericSansSerif, 10);
            this.minimise.Width = this.Width / 3;
            this.minimise.BackColor = BannerColor; // make it the same as the PictureBox
            this.Controls.Add(this.minimise); // add it to the form's controls
            this.minimise.BringToFront(); // bring it to the front, to display it above the picture box

            this.maximise.Text = Resources.Maximize;
            // remember to make sure it's far enough away so as not to overlap our minimise option
            this.maximise.Location = new Point(this.Location.X + this.Width / 3 * 1, this.title.Height - 37);
            this.maximise.Font = new Font(FontFamily.GenericSansSerif, 10);
            this.maximise.ForeColor = Color.White;
            this.maximise.TextAlign = ContentAlignment.MiddleCenter;
            this.maximise.BackColor = BannerColor; // remember, we want it to match the background
            this.maximise.Width = this.Width / 3;
            this.Controls.Add(this.maximise); // add it to the form
            this.maximise.BringToFront();

            this.close.Text = Resources.Close;
            this.close.TextAlign = ContentAlignment.MiddleCenter;
            this.close.Font = new Font(FontFamily.GenericSansSerif, 10);

            this.close.Location = new Point(this.Location.X + this.Width / 3 * 2, this.title.Height - 37);
            this.close.ForeColor = Color.White;
            this.close.BackColor = BannerColor;
            this.close.Width = this.Width / 3; // this is just to make it fit nicely
            this.Controls.Add(this.close);
            this.close.BringToFront();

            this.minimise.MouseEnter += this.Control_MouseEnter;
            this.maximise.MouseEnter += this.Control_MouseEnter;
            this.close.MouseEnter += this.Control_MouseEnter;

            // and we need to do the same for MouseLeave events, to change it back
            this.minimise.MouseLeave += this.Control_MouseLeave;
            this.maximise.MouseLeave += this.Control_MouseLeave;
            this.close.MouseLeave += this.Control_MouseLeave;

            // and lastly, for these controls, we need to add some functionality
            this.minimise.MouseClick += this.Control_MouseClick;
            this.maximise.MouseClick += this.Control_MouseClick;
            this.close.MouseClick += this.Control_MouseClick;

            // finally, wouldn't it be nice to get some moveability on this control?
            this.title.MouseDown += this.Title_MouseDown;
            this.title.MouseUp += this.Title_MouseUp;
            this.title.MouseMove += this.Title_MouseMove;
        }

        private void Control_MouseEnter(object sender, EventArgs e) {
            if (this.shouldDisposeTokens) { // exited, controls have been disposed
                return;
            }

            this.mouseLeaveCancellationTokenSource.Cancel();

            this.mouseEnterCancellationTokenSource.Dispose();
            this.mouseEnterCancellationTokenSource = new CancellationTokenSource();
            var ct = this.mouseEnterCancellationTokenSource.Token;
            FadeAnimHelper.FadeLabel(sender, Color.DarkCyan, ct);
        }

        private void Control_MouseLeave(object sender, EventArgs e) { // return them to their default colours
            if (this.shouldDisposeTokens) { // exited, controls have been disposed
                return;
            }

            this.mouseEnterCancellationTokenSource.Cancel();

            this.mouseLeaveCancellationTokenSource.Dispose();
            this.mouseLeaveCancellationTokenSource = new CancellationTokenSource();
            var ct = this.mouseLeaveCancellationTokenSource.Token;
            FadeAnimHelper.FadeLabel(sender, Color.White, ct);
        }

        private void Control_MouseClick(object sender, MouseEventArgs e) {
            if (sender.Equals(this.close)) {
                this.Close(); // close the form
            } else if (sender.Equals(this.maximise)) { // maximise is more interesting. We need to give it different functionality,
                                                       // depending on the window state (Maximise/Restore)
                if (this.maximise.Text == Resources.Maximize) {
                    this.WindowState = FormWindowState.Maximized; // maximise the form
                    this.maximise.Text = Resources.Restore; // change the text
                    this.title.Width = this.Width; // stretch the title bar
                } else { // we need to restore
                    this.WindowState = FormWindowState.Normal;
                    this.maximise.Text = Resources.Maximize;
                }
            } else { // it's the minimise label
                this.WindowState = FormWindowState.Minimized; // minimise the form
            }
        }

        private void Title_MouseUp(object sender, MouseEventArgs e) {
            this.drag = false;
        }

        private void Title_MouseDown(object sender, MouseEventArgs e) {
            this.startPoint = e.Location;
            this.drag = true;
        }

        private void Title_MouseMove(object sender, MouseEventArgs e) {
            if (this.drag) { // if we should be dragging it, we need to figure out some movement
                var p1 = new Point(e.X, e.Y);
                var p2 = this.PointToScreen(p1);
                var p3 = new Point(p2.X - this.startPoint.X,
                                     p2.Y - this.startPoint.Y);
                this.Location = p3;
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            var form = (Form)sender;
            this.viewController.RenderInitialView(form);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (this.components != null) {
                    this.components.Dispose();
                }

                this.shouldDisposeTokens = true;
                this.mouseEnterCancellationTokenSource.Dispose();
                this.mouseLeaveCancellationTokenSource.Dispose();
                this.title.Dispose();
                this.minimise.Dispose();
                this.maximise.Dispose();
                this.close.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}