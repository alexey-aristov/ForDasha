using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string AssignedTo { get; set; }
        public string State { get; set; }
        public string Severity { get; set; }
        public TimeSpan OriginalTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Area { get; set; }
        public List<Item> Items { get; set; }

        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Activity { get; set; }

    }
}
