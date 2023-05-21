using System.Collections.Generic;

namespace MeetingAppCore.DebugTracker
{
    public class FunctionTracker
    {
        //
        protected static FunctionTracker instance;
        private FunctionTracker()
        {
            All = new List<string>();
            Api = new SortedSet<string>();
            Hub = new SortedSet<string>();
            Tracker = new SortedSet<string>();
            Service = new SortedSet<string>();
            Repo= new SortedSet<string>();

        }
        //public static FunctionTracker Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new FunctionTracker();
        //        return instance;
        //    }
        //}
        public static FunctionTracker Instance ()
        {
            //get
            //{
                if (instance == null)
                    instance = new FunctionTracker();
                return instance;
            //}
        }
        public List<string> All { get; set; }
        public SortedSet<string> Api { get; set; }
        public SortedSet<string> Hub { get; set; }
        public SortedSet<string> Tracker { get; set; }
        public SortedSet<string> Service { get; set; }
        public SortedSet<string> Repo { get; set; }
        public void AddApiFunc(string apiFunc)
        {
             All.Add(apiFunc);
            Api.Add(apiFunc);
        }
        public void AddHubFunc(string HubFunc)
        {
            All.Add(HubFunc);
            Hub.Add(HubFunc);
        }
        public void AddTrackerFunc(string apiFunc)
        {
            All.Add(apiFunc);
            Api.Add(apiFunc);
        }
        public void AddServicceFunc(string apiFunc)
        {
            All.Add(apiFunc);
            Api.Add(apiFunc);
        }
        public void AddRepoFunc(string apiFunc)
        {
            All.Add(apiFunc);
            Api.Add(apiFunc);
        }
    }
}
