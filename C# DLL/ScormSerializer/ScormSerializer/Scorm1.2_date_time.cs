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

namespace Scorm1_2
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
            return new DateTime(System.DateTime.Parse(input));
        }
        public override string ToString()
        {
            return datetime.ToString("HH:mm:ss.f");
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
            string[] tokens = i.Split(new char[] { ':', ',' });
            System.TimeSpan newspan = new System.TimeSpan(
            System.Convert.ToInt16(tokens[0]),
             System.Convert.ToInt16(tokens[1]),
             System.Convert.ToInt16(tokens[2]),
             tokens.Length == 4 ? System.Convert.ToInt16(tokens[3]) : 0);
            return new TimeSpan(newspan);
        }
        public string ToString()
        {
            return timespan.Hours.ToString("0000") + ":" + timespan.Minutes.ToString("00") + ":" + timespan.Seconds.ToString("00") + "." + timespan.Milliseconds.ToString("00");
        }

    }
}
