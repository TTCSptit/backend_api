using job.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace scratch
{
    public class CheckJobs
    {
        public static void Run(JobPtitContext context)
        {
            var categories = context.Categories
                .Select(c => new 
                { 
                    c.Name, 
                    JobCount = c.Jobs.Count(),
                    ActiveJobs = c.Jobs.Count(j => j.Status == 1)
                })
                .ToList();

            foreach (var c in categories)
            {
                Console.WriteLine($"{c.Name}: Total={c.JobCount}, Active={c.ActiveJobs}");
            }
        }
    }
}
