using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CovidChart.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CovidChart.API.Models
{
    public class CovidService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CovidHub> _hubContext;

        public CovidService(AppDbContext context, IHubContext<CovidHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IQueryable<Covid> GetList()
        {
            return _context.Covids.AsQueryable();
        }

        public async Task SaveCovid(Covid covid)
        {
            await _context.Covids.AddAsync(covid);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveCovidList", GetCovidChartList());
        }

        public List<CovidChart> GetCovidChartList()
        {
            List<CovidChart> covidCharts = new List<CovidChart>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "select tarih,[1],[2],[3],[4],[5] FROM" +
                                      "\r\n(select [City],[Count],CAST([CovidDate] as date) as tarih from Covids) as covidT" +
                                      "\r\nPIVOT\r\n(Sum(Count) for City IN([1],[2],[3],[4],[5])) as ptable" +
                                      "\r\norder by tarih asc";

                command.CommandType = System.Data.CommandType.Text;

                _context.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CovidChart c  = new CovidChart();
                        c.CovidDate = reader.GetDateTime(0).ToShortDateString();

                        Enumerable.Range(1,5).ToList().ForEach(x =>
                        {
                            if (System.DBNull.Value.Equals(reader[x]))
                            {
                                c.Counts.Add(0);
                            }
                            else
                            {
                                c.Counts.Add(reader.GetInt32(x));
                            }

                        });

                        covidCharts.Add(c);
                    }
                    
                }

                _context.Database.CloseConnection();

                return covidCharts;


            }
        }

    }
}
