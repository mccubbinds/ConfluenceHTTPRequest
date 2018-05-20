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

        string GetJsonFromHttpAuth(string url, string username, string password, ref JObject jObject)
        {
            string usernamePassword = username + ":" + password;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(usernamePassword));
            request.PreAuthenticate = true;
            try
            {
                WebResponse response = request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    string jsonObjectString = reader.ReadToEnd();

                    jObject = JObject.Parse(jsonObjectString);
                    return "Json successfully retreived.";
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();

                    return "Http request Failed.\r\n" + errorText + "\r\n";
                }
            }
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            JObject jsonObject = null;
            string responseString = GetJsonFromHttpAuth(
                "https://syn-confluence.tms-orbcomm.com:8800/rest/api/content?spaceKey=GT1020FW&title=Device+Specification&expand=body.view",
                usernameTextbox.Text,
                passwordTextbox.Text,
                ref jsonObject);

            ParseJsonForDeviceComponentParameterNumbers(jsonObject);
            logTextbox.AppendText(responseString);
        }

        private void ParseJsonForDeviceComponentParameterNumbers(JObject jsonObject)
        {
            string deviceSpecificationTablesHtml = "<body>" + (string)jsonObject["results"][0]["body"]["view"]["value"] + "</body>";

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(deviceSpecificationTablesHtml);

            List<List<string>> table = doc.DocumentNode.SelectSingleNode("//body")
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();

            foreach (List<string> list in table)
            {
                foreach (string item in list)
                {
                    logTextbox.AppendText(item + "\r\n");
                }
            }
        }
    }
}
