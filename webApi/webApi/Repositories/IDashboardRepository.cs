using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webApi.Repositories
{
    public class WeeklyEnrollmentStatsDto
    {
        public string Week { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EnrollmentCount { get; set; }
    }

    public interface IDashboardRepository
    {
        Task<List<WeeklyEnrollmentStatsDto>> GetWeeklyEnrollmentStatsAsync();
    }
} 