using ALLINONEPROJECTWITHOUTJS.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ALLINONEPROJECTWITHOUTJS.Services
{
    public class PartyService : IPartyService
    {
        private readonly string _connectionString;
        public PartyService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public List<PartyMaster> GetAllParties()
        {
            var parties = new List<PartyMaster>();

            using var con = new SqlConnection(_connectionString);
            using var sda = new SqlDataAdapter("select * from partymasters", con);
            var partyData = new DataTable();
            sda.Fill(partyData);

            parties = partyData.AsEnumerable()
                .Select(x => new PartyMaster
                {
                    Id = Convert.ToInt32(x["Id"]),
                    Name = Convert.ToString(x["Name"]) ?? ""
                })
                .ToList();

            if (parties.Count == 0)
                parties.Add(new PartyMaster());

            return parties;
        }
    }
}
