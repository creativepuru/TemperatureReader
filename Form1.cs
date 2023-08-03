using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System.Threading;
using System.IO;
using Action = System.Action;

namespace TemperatureReader
{
    public partial class TemperatureReader : Form
    {
        private SheetsService sheetsService;

        public TemperatureReader()
        {
            InitializeComponent();

            serialPort1.PortName = "COM4";
            serialPort1.BaudRate = 9600;
            serialPort1.Open();

            // Calling the authorization method to get the authorized SheetsService object
            sheetsService = GoogleSheetsAuthentication.GetService();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedData = serialPort1.ReadLine();
            string[] values = receivedData.Split(',');

            // Checking if the data contains both temperature and humidity values
            if (values.Length == 3)
            {
                this.Invoke(new Action(async () =>
                {
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");

                    // Prepare data in separate variables for each column
                    var rowData = new List<object>
                    {
                        currentTime,        // Column A: Time
                        values[0],          // Column B: Temperature
                        values[1],          // Column C: Humidity
                        values[2]           // Column D: LDR
                    };

                    textBox1.Text = currentTime.ToString(); // Time
                    textBox2.Text = values[0].ToString();   // Temperature
                    textBox3.Text = values[1].ToString();   // Humidity
                    textBox4.Text = values[2].ToString();   // LDR

                    // Write data to Google Sheet
                    var spreadsheetId = "1Ahiq3saNaXlYVoFkS7b99rgxHLNibUs3mFD91R-jtLU";
                    var range = "Sheet1!A2:D2";
                    var valueRange = new ValueRange();
                    valueRange.Values = new List<IList<object>> { rowData };

                    var appendRequest = sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
                    appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                    var appendResponse = await appendRequest.ExecuteAsync();
                }));
            }
        }
    }

    public class GoogleSheetsAuthentication
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Temperature Reader";
        static string ClientSecretFilePath = "D:\\COURSES\\TISHITU\\CSharpPrograms\\TemperatureReader\\bin\\Debug\\client_secret_947361990979-qtarip1envsd8d2ldjhr80pki342evco.apps.googleusercontent.com.json"; 

        public static SheetsService GetService()
        {
            UserCredential credential;

            using (var stream = new FileStream(ClientSecretFilePath, FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,Scopes,"user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets service using the authorized credential
            return new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
    }
}
