using Microsoft.AspNetCore.Mvc;
using System.Net;
using TimeProduction.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ServiceReference1;
using Newtonsoft.Json;

namespace TimeProduction.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyProductionController : ControllerBase
    {
        List<string> files = GetDirectoryFiles();

        //GET TOTAL PRODUCED IN AN HOUR. INPUT FORMAT yyMMddHH

        [HttpGet]

        public Time GetTotalinHour(string date)
        {

            Time result = new();

            var match = files.FirstOrDefault(stringToCheck => stringToCheck.Contains(date));

            List<string> energyValues = GetEnergyProduction(match.ToString());

            result.producedEnergy = SumarizeProduction(energyValues);

            return result;


        }

        // SHOW PRODUCTION PER MINUTE - INPUT FORMAT yyMMddHH

        [HttpGet("{date}")]

        public Time GetTotalPerMinute(string date)
        {

            Time result = new();

            var match = files.FirstOrDefault(stringToCheck => stringToCheck.Contains(date));

            result.AllValues = GetEnergyProduction(match.ToString());
       

            return result;

        }

        //GET ENERGY PRODUCTION IN A PERIOD OF TIME - INPUT FORMAT yyyy-MM-dd HH (HH is optional)

        [HttpGet("{start}/getperiod")]

        public Time GetPeriodProduction(DateTime start, DateTime end)
        {
            if (start == end)
            throw new ArgumentException("Start date and time must be different"); 


            Time result = new();
            List <string> interval = GetDateRange(start, end).ConvertAll(date => date.ToString("yyMMddHH"));
            List<string> matches = new();

            
            for (int i = 0; i < interval.Count; i++)
            {
                var match =files.Where(file => file.Contains(interval[i]));

                foreach (var x in match)
                { 
                    var production = GetEnergyProduction(x.ToString());
                    
                    foreach (var y in production)
                    {
                        matches.Add(y);
                        
                    }
                }
               
            }

            result.producedEnergy = SumarizeProduction(matches);

            return result;
        }

        //GET FORECAST
        [HttpPost("{location}")]
        public string Post(string location)
        {
            ForecastServiceClient client = new();
            var result = client.GetForecastAsync(location, "Jeger1studerende").Result.Body.GetForecastResult;

            var hourlyForecast = new List<ForecastValues>();
            var dayforecast = new List<string>();

            for (int x = 0; x < 24; x++)
            {
                hourlyForecast.Add(new ForecastValues
                {
                    temp = result.location.values[x].temp.ToString(),
                    precip = result.location.values[x].precip.ToString(),
                    conditions = result.location.values[x].conditions.ToString(),
                    cloudcover = result.location.values[x].cloudcover.ToString()
                });


            }

            var convert = JsonConvert.SerializeObject(hourlyForecast);


            return convert;
        }


        // METHODS //
        public static List<string> GetDirectoryFiles()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://inverter.westeurope.cloudapp.azure.com/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential("studerende", "kmdp4gslmg46jhs");
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();

                reader.Close();
                response.Close();
                List<string> files = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                return files;
            }
            catch (Exception)
            {
                throw;
            }
         
        }


        public static List<string> GetEnergyProduction(string file)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://inverter.westeurope.cloudapp.azure.com/" + file);
                request.Credentials = new NetworkCredential("studerende", "kmdp4gslmg46jhs");

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);

                var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    HasHeaderRecord = false,
                    Comment = '#',
                    AllowComments = true,
                    Delimiter = ";",
                    MissingFieldFound = null

                };


                using var CsvReader = new CsvReader(reader, csvConfig);
                List<string> energy = new List<string>();


                while (CsvReader.Read())
                {
                    var producedEnergy = CsvReader.GetField(37);
                    if (producedEnergy != null)
                    {
                        energy.Add(producedEnergy.ToString());
                    }

                }
                return energy;

            }

            catch (Exception)
            {
                throw;
            }

        }

        public static List<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> range = new List<DateTime>();
            if (startDate > endDate)
            throw new ArgumentException("End Date must be greater than Start Date");

            for (var date = startDate; date <= endDate; date = date.AddHours(1))
            {
                range.Add(date);
            }
            return range;
        }

        public static int SumarizeProduction(List<string> list)
        {
            int result = int.Parse(list[list.Count - 1]) - int.Parse(list[1]);

            return result;
        }

    }

   
}





