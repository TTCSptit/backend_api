using job.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace scratch
{
    public class CheckDbSchema
    {
        public static void Run(JobPtitContext context)
        {
            var companyType = context.Model.FindEntityType(typeof(job.Models.Company));
            var properties = companyType.GetProperties();
            Console.WriteLine("Columns in Companies table:");
            foreach (var prop in properties)
            {
                Console.WriteLine($"- {prop.Name} ({prop.ClrType.Name})");
            }
        }
    }
}
