using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace IronPortIntegration
{
    public class GrepLogEntry
    {
        public int MID { get; private set; }
        public int ICID { get; private set; }
        public string DateTimeAsString { get; private set; }
        public string Severity { get; private set; }
        public string RawData { get; private set; }

        private GrepResultParser _resultParser;

        public GrepLogEntry(string rawData, GrepResultParser resultParser = null)
        {
            _resultParser = null != resultParser ? resultParser : new GrepResultParser();

            RawData = rawData;
            DateTimeAsString = _resultParser.ParseLogDate(RawData);
            Severity = _resultParser.ParseLogSeverity(RawData);
            MID = _resultParser.ParseMID(RawData);
            ICID = _resultParser.ParseICID(RawData);
        }

        public override string ToString()
        {
            return string.Format("DateTime: {0}\nSeverity: {1}\nMID: {2}\nRaw: {3}",
                DateTimeAsString,
                Severity,
                MID,
                RawData);
        }
    }
}
