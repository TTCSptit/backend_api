using job.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace scratch
{
    public class FixJobStatus
    {
        public static void Run(JobPtitContext context)
        {
            var jobs = context.Jobs.Where(j => j.Status == 0).ToList();
            foreach (var j in jobs)
            {
                j.Status = 1;
            }
            context.SaveChanges();
            Console.WriteLine($"Updated {jobs.Count} jobs to Status = 1.");
        }
    }
}
