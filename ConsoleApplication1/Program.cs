﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DashasFromTfs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\alexey0206\Documents\Downloads\data.json";
            string text = File.ReadAllText(filePath);
            var t = JsonConvert.DeserializeObject<Item>(text);
            List<Tuple<Item,string>> list =new List<Tuple<Item, string>>();
            AddItems(list,t,-1,"1",0);
            list = list.Select(a =>new Tuple<Item,string>(a.Item1,a.Item2.Substring(4))).OrderBy(a => Int32.Parse(a.Item2.Split('.')[0])).ToList();
            string header = "id,name,date,time,type,activity,severity,employee";
            List< string> lines =new List<string>();
            lines.Add(header);
            lines.AddRange(
                list.Select(
                    a =>
                        $"{a.Item2},{a.Item1.Name},{getDate(a.Item1.Date)},{getTime(a.Item1.Time)},{a.Item1.Type},{a.Item1.Activity},{a.Item1.Severity},{a.Item1.AssignedTo}")
                    .ToList());
            
                File.WriteAllLines(@"C:\Users\alexey0206\Documents\Downloads\data.out.txt",lines);
            int y = 0;
        }

        private static string getDate(DateTime dateTime)
        {
            return dateTime == DateTime.MinValue ? "" : dateTime.ToString("yyyy-mm-dd");
        }
        private static string getTime(TimeSpan time)
        {
            return time == TimeSpan.Zero ? "" : time.ToString();
        }
        static void AddItems(List<Tuple<Item, string>> list,Item item,int depth,string prev,int id)
        {
            depth += 1;
            if (item.Items==null||item.Items.Count==0)
            {
                list.Add(new Tuple<Item, string>(item,prev+"."+id));
            }
            else
            {
                int i = 1;
                foreach (var item1 in item.Items)
                {
                    AddItems(list,item1,depth, prev + "." + id, i);
                    //item1.Items = null;
                    i++;
                }
            }
        }

       
    }
}