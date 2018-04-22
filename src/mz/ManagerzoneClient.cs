using Newtonsoft.Json.Linq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ManagerzoneConsole
{
    public class ManagerzoneClient
    {
        private readonly string username;
        private readonly string password;

        public ManagerzoneClient(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public async Task<IEnumerable<Player>> YourTeam(string teamId)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.managerzone.com/ajax.php?p=players&sub=team_players&tid={teamId}&sport=soccer");

            request.Headers.Add("Cookie", GetAuthCookies());

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            var allPlayersHtml = json["players"].ToString();

            var splitedHtml = allPlayersHtml.Split("player_name");

            var result = new List<Player>();

            for (int i = 1; i < splitedHtml.Length; i++)
            {
                result.Add(ExtractPlayerFrom(splitedHtml[i]));
            }

            return result;
        }

        public async Task<Player> SearchPlayerBy(string id)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.managerzone.com/?p=players&pid={id}");

            request.Headers.Add("Cookie", GetAuthCookies());

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ExtractPlayerFrom(content);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private Player ExtractPlayerFrom(string content)
        {
            var player = new Player()
            {
                Id = ExtractId(content),
                Name = ExtractName(content),
                Nationality = ExtractNationality(content),
                BirthSeason = ExtractBirthSeason(content),
                Height = ExtractHeight(content),
                Weight = ExtractWeight(content),
                PreferredFoot = ExtractPreferredFoot(content),

                Value = ExtractPlayerValue(content),
                Salary = ExtractSalary(content),

                TrainingHistory = GetTrainingHistory(content),

                Speed = ExtractSkill("Speed", content),
                Stamina = ExtractSkill("Stamina", content),
                PlayIntelligence = ExtractSkill("Play Intelligence", content),
                Passing = ExtractSkill("Passing", content),
                Shooting = ExtractSkill("Shooting", content),
                Heading = ExtractSkill("Heading", content),
                Keeping = ExtractSkill("Keeping", content),
                BallControl = ExtractSkill("Ball Control", content),
                Tackling = ExtractSkill("Tackling", content),
                AerialPassing = ExtractSkill("Aerial Passing", content),
                SetPlays = ExtractSkill("Set Plays", content),
                Experience = ExtractSkill("Experience", content),
                Form = ExtractSkill("Form", content),
            };
            return player;
        }

        private IEnumerable<TrainingResult> GetTrainingHistory(string content)
        {
            var todaysTraining = new TrainingResult()
            {
                ImprovementRate = ExtractImprovementRate(content),
                Date = new LocalDate(1999,12,31),
                HasGainedNextLevel = ExtractLevelUp(content),
                TrainedSkill = ExtractTrainedSkill(content),
                Type = ExtractTrainingType(content)
            };

            return new TrainingResult[] { todaysTraining };
        }

        private TrainingResult.TrainingType ExtractTrainingType(string _content)
        {
            var content = _content;
            if (!content.Contains("extraTrainingIcon"))
            {
                return TrainingResult.TrainingType.InRegularCamp;
            }
            
            if (content.Contains("coach"))
            {
                return TrainingResult.TrainingType.OnFieldWithCoach;
            }
            else if (content.Contains("camp"))
            {
                return TrainingResult.TrainingType.InRegularCamp;
            }
            else
            {
                throw new InvalidOperationException($"Cannot determine training type from content - {content}.");
            }
        }

        private string ExtractTrainedSkill(string content)
        {
            if (content.Contains("resting"))
            {
                return "resting";
            }

            var start = content.IndexOf("improvementLabel");
            content = content.Substring(start);
            var cutTagStart = content.IndexOf("clippable");
            content = content.Substring(cutTagStart);

            return ExtractValueBetweenFirstTag(content);
        }

        private bool ExtractLevelUp(string content)
        {
            var searchTag = "bar_";
            var startRate = content.IndexOf(searchTag);
            content = content.Substring(startRate + searchTag.Length);
            var endRate = content.IndexOf(".");
            var rateHtml = content.Substring(0, endRate);

            return rateHtml.Contains("_");
        }

        private int ExtractImprovementRate(string content)
        {
            var startRate = content.IndexOf("bar_");
            content = content.Substring(startRate);
            var endRate = content.IndexOf(".");
            var rateHtml = content.Substring(0, endRate);


            var rateString = "";

            foreach (var character in rateHtml)
            {
                if (character >= '0' && character <= '9')
                {
                    rateString += character;
                }
            }
            if(rateString == "")
            {
                return 0;
            }
            return int.Parse(rateString);
        }

        private int ExtractSalary(string content)
        {
            var start = content.IndexOf("Salary:");
            if(start == -1)
            {
                return 0;
            }
            content = content.Substring(start);
            var salaryAsHtml = ExtractValueBetweenFirstTag(content);

            var salaryAsString = "";

            foreach (var character in salaryAsHtml)
            {
                if (character >= '0' && character <= '9')
                {
                    salaryAsString += character;
                }
            }

            return int.Parse(salaryAsString);
        }

        private int ExtractPlayerValue(string content)
        {
            var start = content.IndexOf("Value:");
            content = content.Substring(start);
            var valueAsHtml = ExtractValueBetweenFirstTag(content);

            var valueAsString = "";

            foreach (var character in valueAsHtml)
            {
                if (character >= '0' && character <= '9')
                {
                    valueAsString += character;
                }
            }

            return int.Parse(valueAsString);
        }

        private Player.Foot ExtractPreferredFoot(string content)
        {
            var start = content.IndexOf("Foot:");
            content = content.Substring(start);

            var foot = ExtractValueBetweenFirstTag(content).ToLower();

            switch (foot)
            {
                case "left": return Player.Foot.Left;
                case "right": return Player.Foot.Right;
                case "both": return Player.Foot.Both;
                default:
                    throw new InvalidOperationException($"Uknown \"foot\" - {foot}.");
            }
        }

        private int ExtractWeight(string content)
        {
            var start = content.IndexOf("Weight:");
            content = content.Substring(start);

            var weightHtml = ExtractValueBetweenFirstTag(content);
            var weightString = "";

            foreach (var character in weightHtml)
            {
                if (character >= '0' && character <= '9')
                {
                    weightString += character;
                }
            }

            return int.Parse(weightString);
        }

        private int ExtractHeight(string content)
        {
            var start = content.IndexOf("Height:");
            content = content.Substring(start);

            var heightHtml = ExtractValueBetweenFirstTag(content);
            var heightString = "";

            foreach (var character in heightHtml)
            {
                if(character >= '0' && character <= '9')
                {
                    heightString += character;
                }
            }
            
            return int.Parse(heightString);
        }

        private int ExtractBirthSeason(string content)
        {
            var start = content.IndexOf("Birth Season:");
            content = content.Substring(start);

            var birthSeasonString = ExtractValueBetweenFirstTag(content);
            var birthSeason = int.Parse(birthSeasonString);

            return birthSeason;
        }

        private string ExtractNationality(string content)
        {
            var start = content.IndexOf("Nationality:");
            content = content.Substring(start);
            var nextSpanStart = content.IndexOf("<span");
            content = content.Substring(nextSpanStart);

            return ExtractValueBetweenFirstTag(content);
        }

        private string ExtractName(string content)
        {
            return ExtractValueBetweenFirstTag(content);
        }

        private string ExtractId(string content)
        {
            var start = content.IndexOf("player_id_span");
            content = content.Substring(start);

            return ExtractValueBetweenFirstTag(content);
        }

        private string ExtractValueBetweenFirstTag(string content)
        {
            var startIndex = content.IndexOf(">");
            content = content.Substring(startIndex);
            var endIndex = content.IndexOf("<");
            var value = content.Substring(1, endIndex - 1);

            return value;
        }

        private int ExtractSkill(string skill, string content)
        {
            var startIndex = content.IndexOf(skill + ": ");
            var initialSubString = content.Substring(startIndex);

            var start = initialSubString.IndexOf(": ");

            var secondSubString = initialSubString.Substring(start + 2);
            var end = secondSubString.IndexOf("\"");

            var skillString = secondSubString.Substring(0, end);

            return int.Parse(skillString);
        }

        private string GetAuthCookies()
        {
            var rememberMe = 0;
            var sport = "soccer";
            var hash = Encrypt(password.ToLower());
            var randomGen = new Random();
            var randomNr = randomGen.NextDouble();

            var url = $"https://www.managerzone.com/ajax.php?p=start&sub=Login&logindata_md5={hash}&logindata_username={username}&logindata_remember_me={rememberMe}&sport={sport}&sid={randomNr}";
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = client.SendAsync(request).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            var cookies = response.Headers.GetValues("Set-Cookie").ToArray();
            var cookie = cookies[0];
            cookie = cookie.Substring(0, cookie.IndexOf(";"));

            var request2 = new HttpRequestMessage(HttpMethod.Post, "https://www.managerzone.com/?p=login");
            request2.Headers.Add("Cookie", cookie);

            var dic = new Dictionary<string, string>();
            dic.Add("logindata[md5]", hash);
            dic.Add("logindata[username]", username);
            dic.Add("logindata[sport]", sport);
            dic.Add("logindata[remember_me]", "");
            dic.Add("logindata[markasdefault]", "");

            var content = new FormUrlEncodedContent(dic);

            request2.Content = content;

            var response2 = client.SendAsync(request2).Result;
            var responseContent2 = response2.Content.ReadAsStringAsync().Result;

            return cookie;
        }

        private string Encrypt(string password)
        {
            var key = Encoding.UTF8.GetBytes("g372=RVx6!yn");
            var pass = Encoding.UTF8.GetBytes(password);
            var md5 = new HMACMD5(key);
            var hex = md5.ComputeHash(pass);

            string hash = BitConverter.ToString(hex).Replace("-", string.Empty).ToLower();
            return hash;
        }
    }
}
