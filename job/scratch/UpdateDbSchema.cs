using job.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace scratch
{
    public class UpdateDbSchema
    {
        public static void Run(JobPtitContext context)
        {
            try {
                context.Database.ExecuteSqlRaw("ALTER TABLE Companies ADD Industry NVARCHAR(MAX) NULL");
                Console.WriteLine("Added Industry column.");
            } catch (Exception ex) { Console.WriteLine($"Industry column error: {ex.Message}"); }

            try {
                context.Database.ExecuteSqlRaw("ALTER TABLE Companies ADD Size NVARCHAR(MAX) NULL");
                Console.WriteLine("Added Size column.");
            } catch (Exception ex) { Console.WriteLine($"Size column error: {ex.Message}"); }

            try {
                context.Database.ExecuteSqlRaw("ALTER TABLE Companies ADD Founded NVARCHAR(MAX) NULL");
                Console.WriteLine("Added Founded column.");
            } catch (Exception ex) { Console.WriteLine($"Founded column error: {ex.Message}"); }
        }
    }
}
