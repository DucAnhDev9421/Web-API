using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WeeklyEnrollmentStatsDto>> GetWeeklyEnrollmentStatsAsync()
        {
            var enrollments = await _context.Enrollments.ToListAsync();
            var grouped = enrollments
                .GroupBy(e => ISOWeek.GetWeekOfYear(e.EnrolledAt))
                .Select(g => new
                {
                    Week = g.Key,
                    Year = g.First().EnrolledAt.Year,
                    StartDate = FirstDateOfWeekISO8601(g.First().EnrolledAt.Year, g.Key),
                    EndDate = FirstDateOfWeekISO8601(g.First().EnrolledAt.Year, g.Key).AddDays(6),
                    EnrollmentCount = g.Count()
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Week)
                .ToList();

            var result = grouped.Select(g => new WeeklyEnrollmentStatsDto
            {
                Week = $"{g.Year}-W{g.Week}",
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                EnrollmentCount = g.EnrollmentCount
            }).ToList();
            return result;
        }

        // Helper: Lấy ngày đầu tuần theo chuẩn ISO 8601
        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var weekNum = weekOfYear;
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }
} 