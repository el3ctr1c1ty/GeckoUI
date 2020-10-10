using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Gecko;
using Gecko.DOM;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace GeckoUI
{
    public class GeckoForm : Form
    {
        /// <summary>
        /// Toggle form border
        /// </summary>
        public bool EnableBorder { get => formBorder; set { formBorder = value; UpdateBorder(); } }
        private bool formBorder = true;
        /// <summary>
        /// Toggle from shadow (only for forms without border)
        /// </summary>
        public bool EnableShadow { get => formShadow; set => formShadow = value; }
        private bool formShadow = false;
        /// <summary>
        /// Redirect all links to default system browser
        /// </summary>
        public bool RedirectOutLinks { get => redirect; set => redirect = value; }
        private bool redirect = true;
        /// <summary>
        /// Form border round radius (only for forms without border)
        /// </summary>
        public int RoundRadius { get => round; set => round = value; }
        private int round = 0;
        /// <summary>
        /// Button click identifier (you get it in ElementClick callback)
        /// </summary>
        public string ClickIndeficator { get => btnId; set => btnId = value; }
        private string btnId = "app:btn";

        /// <summary>
        /// Enable form showing animation
        /// </summary>
        public bool EnableEffects { get { if (CheckAeroEnabled()) return plain; else return false; } set => plain = value; }
        private bool plain = true;
        /// <summary>
        /// Limit of form height allowed to move form
        /// </summary>
        public int MoveYLimit { get => moveMax; set => moveMax = value; }
        int moveMax = 0;


        public delegate void ClickHandler(string id, GeckoElement target);
        /// <summary>
        /// Fires on any element is clicked
        /// </summary>
        public event ClickHandler ElementClick;

        public delegate void LoadHandler();
        /// <summary>
        /// Fires when page is fully loaded
        /// </summary>
        public event LoadHandler PageLoaded;
        /// <summary>
        /// GeckoWebBrowser element
        /// </summary>
        public GeckoWebBrowser gecko;

        public GeckoForm()
        {
            
            
            this.SuspendLayout();

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "GeckoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GeckoUI";
            
            this.AllowTransparency = true;
            this.ResumeLayout(false);
            try
            {
                Xpcom.Initialize("Firefox");
            }
            catch(Exception e) {
                if (e.Message.Contains("mozglue"))
                {
                    
                    MessageBox.Show("You must install Microsoft C++ Redistributable 2015 x86!\nIf it's already installed, contact application developer to fix this issue.\n\n\n" + e.Message);
                    System.Diagnostics.Process.Start("https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads");

                }
                else { MessageBox.Show(e.Message); }
                Environment.Exit(-1);
            }
           
            if (plain)
            {
                Opacity = 0;
                Hide();
            }
            Width = 800;
            Height = 500;

            gecko = new GeckoWebBrowser() { Dock = DockStyle.Fill, BackColor = Color.Black };
            Controls.Add(gecko);

            gecko.DomClick += Gecko_DomClick;
            gecko.DomMouseDown += Gecko_DomMouseDown;
            gecko.DomMouseMove += Gecko_DomMouseMove;
            gecko.DomMouseUp += Gecko_DomMouseUp;
            gecko.DocumentCompleted += Gecko_DocumentCompleted;
            gecko.Navigating += Gecko_Navigating;
            this.Resize += GeckoForm_Resize;
            

            

            UpdateBorder();

        }
        /// <summary>
        /// Clear Gecko browser cache
        /// </summary>
        public void ClearCache()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            

            nsIBrowserHistory historyMan = Xpcom.GetService<nsIBrowserHistory>(Gecko.Contracts.NavHistoryService);
            historyMan = Xpcom.QueryInterface<nsIBrowserHistory>(historyMan);
            historyMan.RemovePagesByTimeframe(0, (long)unixTimestamp);
        }
        /// <summary>
        /// Clear Gecko browser cookies
        /// </summary>
        public void ClearCookies()
        {
            nsICookieManager CookieMan;
            CookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
            CookieMan = Xpcom.QueryInterface<nsICookieManager>(CookieMan);
            CookieMan.RemoveAll();
        }

        private string path = string.Empty;
        private void Gecko_Navigating(object sender, Gecko.Events.GeckoNavigatingEventArgs e)
        {
            if (RedirectOutLinks && e.Uri.ToString().Contains("http") && e.Uri.ToString() != path)
            {
                e.Cancel = true;
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
        }

        private void Gecko_DocumentCompleted(object sender, Gecko.Events.GeckoDocumentCompletedEventArgs e)
        {
            Show();
            if (plain)
            {
                ((Control)sender).Refresh();
                for (Opacity = 0; Opacity < 1; Opacity += .1, System.Threading.Thread.Sleep(10)) ;
            }
            else Opacity = 1;
            PageLoaded?.Invoke();
        }

        private void GeckoForm_Resize(object sender, EventArgs e)
        {
            if (round == 0) return;
            this.Region = new Region(
                RoundedRect(
                    new Rectangle(0, 0, this.Width, this.Height)
                    , round
                )
            );
        }
        /// <summary>
        /// Evaluate JavaScript
        /// </summary>
        /// <param name="script">JavaScript string to execute</param>
        /// <returns>JS Evaluation result</returns>
        public string RunJS(string script)
        {
            using (AutoJSContext context = new AutoJSContext(gecko.Window))
            {
                string result;
                context.EvaluateScript(script, out result);
                return result;
            }
        }
        private void Close_Effect()
        {
            if (plain)
            {
                while(Opacity != 0)
                {
                    System.Threading.Thread.Sleep(1); Opacity -= .1;
                }

            }
        }



        private void Gecko_DomClick(object sender, DomMouseEventArgs e)
        {
            var elem = e.Target.CastToGeckoElement();
            string act = elem.GetAttribute(btnId);

            if (elem != null) ElementClick?.Invoke(act, elem);

            string href = elem.GetAttribute("href");
            if (href != null && href.Contains("http") && !href.Contains(path) && RedirectOutLinks) System.Diagnostics.Process.Start(href);  
            
            if (act == "closeApp") { Close_Effect(); Close(); }
            if (act == "muggsy_bogues") MessageBox.Show("Imma running this game like Muggsy Bogues");
            if (act == "minimizeApp") { Close_Effect(); WindowState = FormWindowState.Minimized; Opacity = 1; }
        }

        // ---------------------------------------------------------
        /// <summary>
        /// Load page and show form
        /// </summary>
        /// <param name="page">Page URL</param>
        public void LoadPage(string page)
        {
            
            path = page;
            LoadPage();
        }
        /// <summary>
        /// Load page, set form size and show it
        /// </summary>
        /// <param name="page">Page URL</param>
        /// <param name="width">Form Width</param>
        /// <param name="height">Form Height</param>
        public void LoadPage(string page, int width, int height)
        {
            Width = width;
            Height = height;
            LoadPage(page);
        }
        /// <summary>
        /// Load default page, but set form size
        /// </summary>
        /// <param name="width">Form Width</param>
        /// <param name="height">Form Height</param>
        public void LoadPage(int width, int height)
        {
            Width = width;
            Height = height;
            LoadPage();
        }
        /// <summary>
        /// Load default page, to use in example
        /// </summary>
        public void LoadPage()
        {

            if (path == "")
            {
                if (CultureInfo.CurrentUICulture.Name.Contains("ru")) path = "http://zd3v.tech/gUI/index_ruru.html";
                else path = "http://zd3v.tech/gUI";
                Width = 800;
                Height = 510;
            }

            gecko.Navigate(path);
        }
        // -------------------------------------------------------------------
        private bool mouseDown;
        private Point lastLocation;

        private void Gecko_DomMouseUp(object sender, DomMouseEventArgs e) => mouseDown = false;

        private void Gecko_DomMouseMove(object sender, DomMouseEventArgs e)
        {
            if (mouseDown)
            {
                if (e.ClientY > moveMax && moveMax != 0) return;
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.ClientX, (this.Location.Y - lastLocation.Y) + e.ClientY);

                this.Update();
            }
        }

        private void Gecko_DomMouseDown(object sender, DomMouseEventArgs e)
        {
            if (formBorder) return;
            mouseDown = true;
            lastLocation = new Point(e.ClientX, e.ClientY);
        }

        private void UpdateBorder()
        {
            if (formBorder) FormBorderStyle = FormBorderStyle.Sizable;
            else FormBorderStyle = FormBorderStyle.None;
        }

        // --------------------------------------------------------------------------



        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        public static GraphicsPath RoundedRect(Rectangle baseRect, int radius)
        {
            var diameter = radius * 2;
            var sz = new Size(diameter, diameter);
            var arc = new Rectangle(baseRect.Location, sz);
            var path = new GraphicsPath();

            // Верхний левый угол
            path.AddArc(arc, 180, 90);

            // Верхний правый угол
            arc.X = baseRect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Нижний правый угол
            arc.Y = baseRect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Нижний левый угол
            arc.X = baseRect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            if (!formBorder && formShadow)
            {
                switch (m.Msg)
                {
                    case WM_NCPAINT:
                        if (m_aeroEnabled)
                        {
                            var v = 2;
                            DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                            MARGINS margins = new MARGINS()
                            {
                                bottomHeight = 1,
                                leftWidth = 0,
                                rightWidth = 0,
                                topHeight = 0
                            }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                        }
                        break;
                    default: break;
                }
                base.WndProc(ref m);
                if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
            }
            else { base.WndProc(ref m); return; }
        }
        

    }

    // -----------------------------------------------------------------------------------------------------------


}
