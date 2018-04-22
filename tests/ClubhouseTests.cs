using ManagerzoneConsole;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace ManagerzoneTests
{
    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PrivateTeamInformation
    {
        public string OwnPlayerId { get; set; }
        public string TeamId { get; set; }
    }

    public class ClubhouseTests
    {
        private string username;
        private string password;
        private string playerId;
        private string teamId;

        public ClubhouseTests()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddUserSecrets<Credentials>();
            configurationBuilder.AddUserSecrets<PrivateTeamInformation>();
            var conf = configurationBuilder.Build();

            username = conf["Username"];
            password = conf["Password"];
            playerId = conf["OwnPlayerId"];
            teamId = conf["TeamId"];
        }

        [Fact]
        public async Task UserShouldBeRetrievePlayerById()
        {
            var mz = new ManagerzoneClient(username, password);
            var player = await mz.SearchPlayerBy(playerId);
            Assert.NotNull(player);
        }

        [Fact]
        public async Task UserShouldBeRetrievePlayers()
        {
            var mz = new ManagerzoneClient(username, password);
            var players = await mz.YourTeam(teamId);
            Assert.NotEmpty(players);
        }
    }
}
