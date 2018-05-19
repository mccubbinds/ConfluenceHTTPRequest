using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfluenceJsonRequest
{
    public partial class ConfluenceJsonRequest : Form
    {
        public ConfluenceJsonRequest()
        {
            InitializeComponent();
        }

        JObject Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string usernamePassword = usernameTextbox.Text + ":" + passwordTextbox.Text;

            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(usernamePassword));
            request.PreAuthenticate = true;
            try
            {
                WebResponse response = request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    string jsonObjectString = reader.ReadToEnd();

                    return JObject.Parse(jsonObjectString);
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();

                    MessageBox.Show(errorText);
                }
                throw;
            }
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            JObject jsonObject = Get("https://syn-confluence.tms-orbcomm.com:8800/rest/api/content?spaceKey=GT1020FW&title=Device+Specification&expand=body.view");
            
            string tablesHtml = (string)jsonObject["results"][0]["body"]["view"]["value"];
            logTextbox.AppendText(tablesHtml);
        }
    }
}
