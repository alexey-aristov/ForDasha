using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;

namespace DashasFromTfs
{
    public class Program
    {
        private static WorkItemStore _workItemStore;
        private static int _counter = 0;
        static void Main(string[] args)
        {
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("URI"));
            var sevice = tfs.GetService<WorkItemStore>();
            _workItemStore = sevice;
            int i = 0;
            var project = sevice.Projects["VAR1"];
            var qq = project.QueryHierarchy.Where(a => a.Name == "My Queries").First();

            //var query = (qq as QueryFolder).Where(a => a.Name == "New query").First();
            var query = (qq as QueryFolder).Where(a => a.Name == "New Query 1").First();

            var queryDefinition = (query as QueryDefinition);
            var variables = new Dictionary<string, string>() { { "project", "VAR1" } };

            var result = sevice.Query(queryDefinition.QueryText, variables);
            Item root = new Item();
            root.Items = result.Cast<WorkItem>().Select(ReadItem).Where(a => a != null).ToList();

            root.Items = root.Items.OrderBy(a => a.CreatedDate).ToList();
            var res = JsonConvert.SerializeObject(root, Formatting.Indented);
            File.WriteAllText("D:\\Dasha\\Test2", res);
            int y = 0;
        }

        private static Item ReadItem(WorkItem workitem)
        {
            Item item = null;
            try
            {

                if (workitem.Type.Name == "TimeSheet")
                {
                    var tt = workitem.Fields.Cast<Field>().Select(a => a.Name + "," + a.Value).ToList();
                    item = new TimeSheet()
                    {
                        AssignedTo = workitem.Fields["Created By"].Value.ToString(),
                        Date =
                            DateTime.Parse(workitem.Fields["TimeReg Report Date"].Value.ToString().Substring(0, 9),
                                CultureInfo.InvariantCulture), //"dd-MMM-yy-dddd hh:mm"
                        Time =
                            TimeSpan.FromHours(double.Parse(workitem.Fields["TimeReg Duration"].Value?.ToString() ?? "0")),
                        Type = workitem.Type.Name,
                        Activity = workitem.Fields["Activity"].Value.ToString()
                    };
                }
                else
                {

                    item = new Item
                    {
                        AssignedTo = workitem.Fields[CoreField.AssignedTo].Value.ToString(),
                        Items = workitem.Links.Cast<Link>()
                            .Where(a => a is RelatedLink)
                            .Select(
                                a =>
                                    new
                                    {
                                        Link = a as RelatedLink,
                                        Item = _workItemStore.GetWorkItem((a as RelatedLink).RelatedWorkItemId)
                                    })
                            .Where(a => a.Item.Type.Name == "TimeSheet" || a.Link.LinkTypeEnd.Name == "Child")
                            .Select(a => ReadItem(a.Item))
                            .Where(a => a != null)
                            .OrderBy(a => a.CreatedDate).ToList()
                            .ToList()
                    };
                }
                item.Area = workitem.Fields.Contains("Area Path") ? workitem.Fields["Area Path"].Value.ToString() : null;
                item.CreatedDate =
                    DateTime.Parse(workitem.Fields["Created Date"].Value.ToString().Substring(0, 9),
                        CultureInfo.InvariantCulture) +
                    TimeSpan.Parse(workitem.Fields["Created Date"].Value.ToString().Split(' ')[1]);
                item.State = workitem.Fields["State"].Value.ToString();
                item.Severity = workitem.Fields.Contains("Severity")
                    ? workitem.Fields["Severity"].Value.ToString()
                    : null;
                item.Name = workitem.Title;
                item.Id = workitem.Id;
                item.Type = workitem.Type.Name;
                item.OriginalTime = workitem.Fields.Contains("Original Estimate")
                                    && (workitem.Fields["Original Estimate"].Value != null)
                    ? TimeSpan.FromHours(double.Parse(workitem.Fields["Original Estimate"].Value.ToString()))
                    : TimeSpan.Zero;

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return item;
            }
            finally
            {
                Console.WriteLine("{2})Item {0} read. Title:{1}", item?.Id.ToString() ?? "null", item?.Name ?? "null", _counter++);
            }
        }
    }

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

    }

    public class TimeSheet : Item
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Activity { get; set; }
    }
}
