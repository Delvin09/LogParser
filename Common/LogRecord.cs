using System;

namespace Common
{
    public class LogRecord
    {
        public int Id { get; set; }

        public DateTime RequestDateTime { get; set; }

        public string Host { get; set; }

        public string Route { get; set; }

        public string QueryParameters { get; set; }

        public int ResultCode { get; set; }

        public int ResponseSize { get; set; }

        public string Geolocation { get; set; }
    }
}
