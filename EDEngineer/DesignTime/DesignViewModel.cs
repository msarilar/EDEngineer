using System;
using System.Collections.ObjectModel;
using EDEngineer.Models;
using NodaTime;

namespace EDEngineer.DesignTime
{
    public class DesignViewModel
    {
        public ObservableCollection<DesignFilter> GradeFilters { get; set; }
        public DesignState State { get; set; }
        public ObservableCollection<DesignBluePrint> Blueprints { get; set; }
        public DesignInstant LastUpdate { get; set; }
        public string LogDirectory { get; set; }
    }

    public class DesignInstant
    {
        public override string ToString()
        {
            return Instant.FromDateTimeOffset(new DateTimeOffset(Year, Month, Day, Hour, Minute, Second, TimeSpan.Zero)).ToString();
        }

        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get;set; }
    }

    public class DesignBluePrint
    {
        public int Grade { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public double Progress { get; set; }
        public int CanCraftCount { get; set; }
        public bool Favorite { get; set; }
        public bool Ignored { get; set; }
    }

    public class DesignFilter
    {
        public bool Checked { get; set; }
        public string FilterName { get; set; }
    }

    public class DesignState
    {
        public ObservableCollection<Wrapper> Cargo { get; set; }
    }

    public class DesignEntry
    {
        public Rarity Rarity { get; set; }
        public EntryData Data { get; set; }
        public int Count { get; set; }
        public int FavoriteCount { get; set; }
    }

    public class Wrapper
    {
        public DesignEntry Value { get; set; }
    }
}
