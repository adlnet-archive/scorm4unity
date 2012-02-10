/***********************************************************************************************************************
 *
 * Unity-SCORM Integration Toolkit Version 1.0 Beta
 * ==========================================
 *
 * Copyright (C) 2011, by ADL (Advance Distributed Learning). (http://www.adlnet.gov)
 * http://www.adlnet.gov/UnityScormIntegration/
 *
 ***********************************************************************************************************************
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 *
 **********************************************************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scorm2004
{
    public class DateTime
    {
        public System.DateTime datetime;
        public DateTime(System.DateTime i)
        {
            if (i != null)
                datetime = i;
            else
                datetime = new System.DateTime();
        }
        public DateTime()
        {
            datetime = System.DateTime.Now;
        }
        public DateTime(Scorm1_2.DateTime i)
        {
            if (i != null)
                datetime = i.datetime;
            else
                datetime = new System.DateTime();
        }
        public DateTime(Scorm2004.DateTime i)
        {
            if (i != null)
                datetime = i.datetime;
            else
                datetime = new System.DateTime();
        }
        public static DateTime Parse(string input)
        {
            System.DateTime t = System.DateTime.Parse(input);
            return new DateTime(t);
        }
        public override string ToString()
        {
            return datetime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fK");
        }
    }
    public class TimeSpan
    {
        public System.TimeSpan timespan;
        public TimeSpan(System.TimeSpan t)
        {
            if (t != null)
                timespan = t;
            else
                timespan = new System.TimeSpan();
        }
        public TimeSpan()
        {
            timespan = new System.TimeSpan();
        }
        public TimeSpan(Scorm1_2.TimeSpan t)
        {
            if (t != null)
                timespan = t.timespan;
            else
                timespan = new System.TimeSpan();
        }
        public TimeSpan(Scorm2004.TimeSpan t)
        {
            if (t != null)
                timespan = t.timespan;
            else
                timespan = new System.TimeSpan();
        }
        static public TimeSpan Parse(string i)
        {
            string[] dt = i.Split(new char[] { 'P', 'T' });
            string d = "";
            string t = "";
            if (dt.Length == 3)
            {
                d = dt[1];
                t = dt[2];
            }
            else
            {
                if (i.IndexOf('P') != -1)
                    d = dt[1];
                else
                    t = dt[1];

            }

            System.TimeSpan ts = new System.TimeSpan();
            if (d.IndexOf('Y') != -1)
            {
                int val = System.Convert.ToInt16(d.Substring(0, d.IndexOf('Y')));
                d = d.Substring(d.IndexOf('Y') + 1);
                ts.Add(new System.TimeSpan(365 * val, 0, 0, 0));
            }
            if (d.IndexOf('M') != -1)
            {
                int val = System.Convert.ToInt16(d.Substring(0, d.IndexOf('M')));
                d = d.Substring(d.IndexOf('M') + 1);
                ts.Add(new System.TimeSpan(30 * val, 0, 0, 0));
            }
            if (d.IndexOf('D') != -1)
            {
                int val = System.Convert.ToInt16(d.Substring(0, d.IndexOf('D')));
                d = d.Substring(d.IndexOf('D') + 1);
                ts.Add(new System.TimeSpan(val, 0, 0, 0));
            }
            if (t.IndexOf('H') != -1)
            {
                int val = System.Convert.ToInt16(t.Substring(0, t.IndexOf('H')));
                t = t.Substring(t.IndexOf('H') + 1);
                ts.Add(new System.TimeSpan(0, val, 0, 0));
            }
            if (t.IndexOf('M') != -1)
            {
                int val = System.Convert.ToInt16(t.Substring(0, t.IndexOf('M')));
                t = t.Substring(t.IndexOf('M') + 1);
                ts.Add(new System.TimeSpan(0, 0, val, 0));
            }
            if (t.IndexOf('S') != -1)
            {
                int val = System.Convert.ToInt16(t.Substring(0, t.IndexOf('S')));
                t = t.Substring(t.IndexOf('S') + 1);
                ts.Add(new System.TimeSpan(0, 0, 0, val));
            }
            return new TimeSpan(ts);
        }
        public string ToString()
        {
            string s = "";
            s += "P";
            s += timespan.Days.ToString() + "DT";
            s += timespan.Hours.ToString() + "H";
            s += timespan.Minutes.ToString() + "M";
            s += timespan.Seconds.ToString() + "S";
            return s;
        }

    }
}
