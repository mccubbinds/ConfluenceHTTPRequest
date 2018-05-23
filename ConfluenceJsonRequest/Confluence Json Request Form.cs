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
        const string DeviceSpecificationPageUrl = "https://syn-confluence.tms-orbcomm.com:8800/rest/api/content?spaceKey=GT1020FW&title=Device+Specification&expand=body.view";

        List<Device> DeviceList;

        public ConfluenceJsonRequest()
        {
            InitializeComponent();
            DeviceList = new List<Device>();
        }

        ResponseCode.Response GetJsonFromHttpAuth(string url, string username, string password, ref JObject jObject)
        {
            ResponseCode.Response responseCode;
            string usernamePassword = username + ":" + password;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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

                    responseCode = ResponseCode.Response.Success;
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                if (errorResponse != null)
                {
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                        String errorText = reader.ReadToEnd();
                    }
                }

                responseCode = ResponseCode.Response.HttpRequestFailed;
            }

            return responseCode;
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            const int deviceTablePosition = 0;
            JObject jsonObject = null;

            var responseCode = GetJsonFromHttpAuth(
                DeviceSpecificationPageUrl,
                usernameTextbox.Text,
                passwordTextbox.Text,
                ref jsonObject);

            if(responseCode == ResponseCode.Response.Success)
            {
                string deviceSpecificationTablesHtml = string.Empty;
                responseCode = ParsedeviceSpecificationJsonForTablesHtml(jsonObject, ref deviceSpecificationTablesHtml);

                if (responseCode == ResponseCode.Response.Success)
                {
                    List<List<string>> tableToReturn = new List<List<string>>();
                    responseCode = ParseTablesHtmlForTable(deviceSpecificationTablesHtml, deviceTablePosition, ref tableToReturn);

                    foreach (List<string> row in tableToReturn)
                    {
                        DeviceList.Add(new Device(Convert.ToByte(row[0]), row[1]));
                    }

                    UpdateDataSources(0xff);
                }
            }

            logTextbox.AppendText(responseCode.ToString());
        }

        private void DevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            JObject jsonObject = null;

            var responseCode = GetJsonFromHttpAuth(
                DeviceSpecificationPageUrl,
                usernameTextbox.Text,
                passwordTextbox.Text,
                ref jsonObject);

            if (responseCode == ResponseCode.Response.Success)
            {
                string deviceSpecificationTablesHtml = string.Empty;
                responseCode = ParsedeviceSpecificationJsonForTablesHtml(jsonObject, ref deviceSpecificationTablesHtml);

                if (responseCode == ResponseCode.Response.Success)
                {
                    if (DevicesComboBox.SelectedValue != null)
                    {
                        Device device = (Device)DevicesComboBox.SelectedItem;
                        var deviceNumber = device.Index;

                        List<List<string>> tableToReturn = new List<List<string>>();
                        responseCode = ParseTablesHtmlForTable(
                            deviceSpecificationTablesHtml,
                            deviceNumber,
                            ref tableToReturn);

                        foreach (List<string> row in tableToReturn)
                        {
                            DeviceList[deviceNumber].AddComponentToList(Convert.ToByte(row[0]), row[1]);
                        }

                        UpdateDataSources(deviceNumber);
                    }
                }
            }
            
        }

        private ResponseCode.Response ParsedeviceSpecificationJsonForTablesHtml(JObject jsonObject,  ref string deviceSpecificationTablesHtml)
        {
            ResponseCode.Response responseCode;
            try
            {
                deviceSpecificationTablesHtml = "<body>" + (string)jsonObject["results"][0]["body"]["view"]["value"] + "</body>";

                responseCode = ResponseCode.Response.Success;
            }

            catch(Exception all)
            {
                responseCode = ResponseCode.Response.UnableToParseJson;
            }

            return responseCode;
        }


        private ResponseCode.Response ParseTablesHtmlForTable(string tablesHtml, int tablePosition, ref List<List<string>> tableToReturn)
        {
            ResponseCode.Response responseCode;

            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(tablesHtml);

                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//body")
                            .Descendants("tr")
                            .Skip(1)
                            .Where(tr => tr.Elements("td").Count() > 1)
                            .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                            .ToList();

                bool parseTable = false;
                foreach (List<string> row in table)
                {
                    if (Convert.ToByte(row[0]) == 0)   //if we are at the beginning of a table
                    {
                        if (tablePosition == 0)         //if this is the table we are trying to parse
                        {
                            parseTable = true;
                        }

                        else
                        {
                            parseTable = false;
                        }

                        tablePosition--;
                    }

                    if (parseTable == true)
                    {
                        tableToReturn.Add(row);
                    }
                }

                responseCode = ResponseCode.Response.Success;
            }

            catch(Exception all)
            {
                responseCode = ResponseCode.Response.ErrorWhileParsingTableData;
            }

            return responseCode;
        }

        private void UpdateDataSources(int deviceNumber)
        {
            DevicesComboBox.DataSource = DeviceList;

            DevicesComboBox.DisplayMember = "Name";
            DevicesComboBox.ValueMember = "Index";
            
            if(DeviceList.Count >= deviceNumber)
            {
                ComponentsComboBox.DataSource = DeviceList[deviceNumber].ComponentList;

                ComponentsComboBox.DisplayMember = "Name";
                ComponentsComboBox.ValueMember = "Index";
            }
        }
    }
}
