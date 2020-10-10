using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gecko;
using Gecko.DOM;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeckoUI
{
    /// <summary>
    /// Gecko Audio Player
    /// </summary>
    public class GeckoAudio
    {
        private string audioID;
        private GeckoForm gecko;
        private string path;

        /// <summary>
        /// Create audio player in GeckoFX form
        /// </summary>
        /// <param name="form">Gecko Form</param>
        /// <param name="pat">Audio file path</param>
        public GeckoAudio(GeckoForm form, Uri pat)
        {
            gecko = form;
            audioID = "geckoAudio" + new Random().Next(0, 2400);
            if (pat.IsFile) path = "file://" + pat.ToString().Replace(@"\", "/");
            else path = pat.ToString();
            string js = "var " + audioID + " = new Audio(\""+path+"\");";

            gecko.RunJS(js);
        }
        /// <summary>
        /// Play audio player
        /// </summary>
        public void Play()
        {
            gecko.RunJS(audioID + ".play()");
        }
        /// <summary>
        /// Pause audio player
        /// </summary>

        public void Pause()
        {
            gecko.RunJS(audioID + ".pause();");
        }
    }
}
