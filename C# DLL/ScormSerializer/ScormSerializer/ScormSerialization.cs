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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ScormSerialization
{
    //Using an API wrapper, walk the datastructure and call the GET methods from the API
    //in order to populate the data
    public class ScormDeSerializer
    {
        //the datamodel to write into
        Object datamodel;
        //used to keep track of the current identifier
        Stack<string> stack;
        //constructor
        public ScormDeSerializer(Object input)
        {
            datamodel = input;
        }
        private bool DeserializeThis(System.Reflection.MemberInfo m)
        {
            object[] atts = m.GetCustomAttributes(false);
            foreach (object a in atts)
            {
                if (a.GetType() == typeof(WriteOnlyAttribute) || a.GetType() == typeof(IgnoreAttribute))
                    return false;
            }
            return true;
        }
        //Given an API wrapper, deserialize to the datamodel
        public Object Deserialize(ScormWrapper wrapper)
        {
            stack = new Stack<string>();
            DeserializeObject(datamodel, datamodel.GetType(), wrapper);
            return datamodel;
        }
        //Get the current string that represents the scorm data model element of the current position
        //in the data structure
        string GetCurrentIdentifier()
        {
            string total = "";
            List<string> command = new List<string>();
            foreach (string i in stack)
            {
                command.Add(i);
                command.Add(".");
            }

            command.Reverse();
            foreach (string i in command)
                total += i;
            total = total.Substring(1);

            total = "cmi." + total;

            return total;
        }
        //Read in the value of a simple type from the wrapper
        object DeserializeIntegral(object inobject,Type elementtype, ScormWrapper wrapper)
        {
            if (elementtype == typeof(string))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(string)).ToString();
            if (elementtype == typeof(int))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(int)).ToInt();
            if (elementtype == typeof(Decimal) || elementtype == typeof(System.Nullable<Decimal>))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(Decimal)).ToDecimal();
            if (elementtype == typeof(bool))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(bool)).ToBool();
            if (elementtype.IsEnum)
                inobject = wrapper.Get(GetCurrentIdentifier(), inobject.GetType()).ToEnum(inobject.GetType());
            if (elementtype == typeof(Scorm2004.DateTime))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(Scorm2004.DateTime)).ToDateTime2004();
            if (elementtype == typeof(Scorm2004.TimeSpan))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(Scorm2004.TimeSpan)).ToTimeSpan2004();
            if (elementtype == typeof(Scorm1_2.DateTime))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(Scorm1_2.DateTime)).ToDateTime1_2();
            if (elementtype == typeof(Scorm1_2.TimeSpan))
                inobject = wrapper.Get(GetCurrentIdentifier(), typeof(Scorm1_2.TimeSpan)).ToTimeSpan1_2();
            return inobject;

        }
        private bool IsLangStringType(Type type)
        {
            if (type.GetProperty("lang") != null && type.GetProperty("Value") != null)
                return true;
            return false;
        }
        //If it's a specially formatted string with lang and value portions, treat is as one string and encode the language
        //in braces
        private object DeSerializeLangString(object inobject,Type type, ScormWrapper wrapper)
        {
            
            if(inobject == null)
                inobject = Activator.CreateInstance(type);

            try
            {
                // string language = (string)type.GetProperty("lang").GetValue(inobject, null);
                // string value = (string)type.GetProperty("Value").GetValue(inobject, null);
                //stack.Push("{" + language + "}" + value);
                //stack.Pop();

                string temp = wrapper.Get(GetCurrentIdentifier(),typeof(string)).ToString();
                if (temp.IndexOf("{lang=") != -1)
                {
                    string[] tokens = temp.Split(new string[] { "{lang=", "}" }, StringSplitOptions.RemoveEmptyEntries);
                    string language = tokens[0];
                    string value = "";
                    if (tokens.Length > 1)
                        value = tokens[1];
                    type.GetProperty("lang").SetValue(inobject, language, null);
                    type.GetProperty("Value").SetValue(inobject, value, null);
                }
                else
                {
                    type.GetProperty("lang").SetValue(inobject, "en-US", null);
                    type.GetProperty("Value").SetValue(inobject, temp, null);
                }
            }
            catch (Exception e)
            {
            }
            return inobject;
        }
        //read in the value of a complex type from the wrapper, recurse if necessary
        object DeserializeObject(object inobject,Type otype, ScormWrapper wrapper)
        {
            //if the object is null and not an array, create a new instance of the 
            //type it should be
            if (!otype.Name.Contains("System.Collections.Generic.List"))
            {
                //little hacky to handle strings
                if (inobject == null && otype != typeof(string))
                    inobject = Activator.CreateInstance(otype);
                if (inobject == null && otype == typeof(string))
                    inobject = "";
            }
            //if it is an array, call the length and create a new array of that length
            else
            {
                //if (inobject == null)
                {
                    
                    inobject = System.Activator.CreateInstance(otype);
                }
            }

          

            //If its not an array iterate over the members
            if (!otype.FullName.Contains("System.Collections.Generic.List"))
            {
                PropertyInfo[] members = otype.GetProperties();
                foreach (PropertyInfo mi in members)
                {
                    if (!mi.Name.Contains("Specified") && DeserializeThis(mi))
                    {
                        //sanity check. should not be null
                        if (inobject == null)
                            return null;
                        //If its a simple type, call DeserializeIntegral
                        if (mi.PropertyType == typeof(string) ||
                            mi.PropertyType == typeof(int) ||
                            mi.PropertyType == typeof(float) ||
                            mi.PropertyType == typeof(bool) ||
                            mi.PropertyType == typeof(Scorm1_2.DateTime) ||
                            mi.PropertyType == typeof(Scorm1_2.TimeSpan) ||
                            mi.PropertyType == typeof(Scorm2004.DateTime) ||
                            mi.PropertyType == typeof(Scorm2004.TimeSpan) ||
                            mi.PropertyType == typeof(Decimal) ||
                            mi.PropertyType == typeof(System.Nullable<Decimal>) ||
                            mi.PropertyType.IsEnum
                            )
                        {
                            stack.Push(mi.Name);
                            mi.SetValue(inobject, DeserializeIntegral(mi.GetValue(inobject, null), mi.PropertyType, wrapper), null);
                            stack.Pop();
                        }
                        else if (IsLangStringType(mi.PropertyType))
                        {
                            stack.Push(mi.Name);
                            mi.SetValue(inobject, DeSerializeLangString(mi.GetValue(inobject, null), mi.PropertyType, wrapper), null);
                            stack.Pop();
                        }
                        //Its a complex type, push its name on the stack and recurse
                        else
                        {
                            stack.Push(mi.Name);
                            mi.SetValue(inobject, DeserializeObject(mi.GetValue(inobject, null), mi.PropertyType, wrapper), null);
                            stack.Pop();
                        }
                    }

                }
            }
            //This object is an array - not that the clause above will create a blank array of the proper 
            //length, so this should never be null
            else
            {
                //Loop over every entry in the array
                stack.Push("_count");
                int length = wrapper.Get(GetCurrentIdentifier(), typeof(int)).ToInt();
                stack.Pop();
                int i = 0;
                Array arrray = null;

                    MethodInfo[] methods232 = otype.GetMethods();
                    foreach (MethodInfo m in methods232)
                    {
                        if (m.Name == "ToArray")
                            arrray = (Array)m.Invoke(inobject, null);
                    }
                    otype = arrray.GetType();
                    for (i = 0; i < length; i++)
                {
                    //push the array index on the identifier name stack
                    stack.Push(i.ToString());
                    //If it's a simple element, deserialize and set it on the array
                    if (otype.GetElementType() == typeof(string) ||
                        otype.GetElementType() == typeof(int) ||
                        otype.GetElementType() == typeof(float) ||
                        otype.GetElementType() == typeof(bool) ||
                        otype.GetElementType() == typeof(Scorm2004.DateTime) ||
                        otype.GetElementType() == typeof(Scorm2004.TimeSpan) ||
                        otype.GetElementType() == typeof(Scorm1_2.DateTime) ||
                        otype.GetElementType() == typeof(Scorm1_2.TimeSpan) ||
                        otype.GetElementType() == typeof(Decimal) ||
                        otype.GetElementType().IsEnum
                        )
                        arrray.SetValue(DeserializeIntegral(arrray.GetValue(i), otype.GetElementType(), wrapper), i);
                    else if (IsLangStringType(otype.GetElementType()))
                    { 
                        arrray.SetValue(DeSerializeLangString(arrray.GetValue(i), otype.GetElementType(), wrapper), i);
                    }
                    else
                        //Its a complex object, deserialize it an set it on the array
                        inobject.GetType().GetMethod("Add").Invoke(inobject,new object[]{DeserializeObject(null, otype.GetElementType(), wrapper)});
                    //pop the index off the identifier name stack
                    stack.Pop();
                }
            } 
            //return the object - important because the inobject could have changed from null to something, and
            //the parent object must know that the value has been set
            return inobject;
        }
       
    }
    //Serialize a CocD type object to an API wrapper that handles the set calls
    public class ScormSerializer
    {
        //the object to serialize
        public Object _object;
        ScormWrapper wrapper;
        private bool SerializeThis(System.Reflection.MemberInfo m)
        {
            object[] atts = m.GetCustomAttributes(false);
            foreach (object a in atts)
            {
                if (a.GetType() == typeof(ReadOnlyAttribute) || a.GetType() == typeof(IgnoreAttribute))
                    return false;
            }
            return true;
        }
        //A constructor
        public ScormSerializer(Object inobject)
        {
            _object = inobject;
            PrevioiusSerialization = new List<ScormSet>();
        }
        //serialize to an API wrapper
        public void Serialize(ScormWrapper w)
        {
            wrapper = w;
            List<ScormSet> sets = Serialize();
            //foreach (ScormSet ss in sets)
            //    wrapper.Set(ss);
        }
        //Get as a list of set commands
        public List<ScormSet> Serialize()
        {
            Serialized = new List<ScormSet>();
            stack = new Stack<string>();
            SerializeObject(_object);
            return Serialized;
        }
        //Create a Set command for the identifier on top of the stack
        //NOTE: unlike GET, the last value on the SET stack is assumed to be the new value
        private void RecordStack(Type ExpectedDataType)
        {
            string total = "";
            //Add the dot notation
            System.Collections.ArrayList thiscommand = new System.Collections.ArrayList();
            foreach (string i in stack)
            {
                thiscommand.Add(i);
                thiscommand.Add(".");
            }

            //some string fixup
            thiscommand.Reverse();
            thiscommand.RemoveAt(thiscommand.Count - 2);
            string value = (string)thiscommand[thiscommand.Count - 1];
            thiscommand.RemoveAt(thiscommand.Count - 1);
            
            foreach (string i in thiscommand)
            {
                total += i;
            }
            //Total is not the name of the identifier
            //value is the value
            total = total.Substring(1);

            // add cmi. to all data model identifiers
            total = "cmi." + total;


            ScormSet ss = new ScormSet(total, value, ExpectedDataType);

            //set this on the serilization list
            //NOTE: we could just call the wrapper here directly
            wrapper.Set(ss);
        }
        private bool IsLangStringType(Type type)
        {
            if (type.GetProperty("lang") != null && type.GetProperty("Value") != null)
                return true;
            return false;
        }
        //If it's a specially formatted string with lang and value portions, treat is as one string and encode the language
        //in braces
        private void SerializeLangString(object inobject)
        {
            Type type = inobject.GetType();
            string language = (string)type.GetProperty("lang").GetValue(inobject, null);
            string value = (string)type.GetProperty("Value").GetValue(inobject, null);
            stack.Push("{lang=" + language + "}" + value);
            RecordStack(typeof(string));
            stack.Pop();
        }
        public static string TimeSpanToString(TimeSpan val)
        {
            string s = "";
            s += "P";
            s += val.Days.ToString() + "DT";
            s += val.Hours.ToString() + "H";
            s += val.Minutes.ToString() + "M";
            s += val.Seconds.ToString() + "S";
            return s;
        }
        //Serialize a simple value
        private void SerializeIntegral(object inobject)
        {
            //parse the enum into a string
            if (inobject.GetType().IsEnum)
            {
                string enumname = Enum.GetNames(inobject.GetType())[(int)inobject];
                
                stack.Push(enumname);
                RecordStack(inobject.GetType());
                stack.Pop();
            }
           
            //integral types other than enums
            else
            {
                //Put strings in quotes
                if (inobject.GetType() == typeof(string))
                {
                    stack.Push(inobject.ToString());
                    RecordStack(typeof(string));
                    stack.Pop();
                }
                else if (inobject.GetType() == typeof(Scorm1_2.DateTime))
                {
                    Scorm1_2.DateTime val = (Scorm1_2.DateTime)inobject;

                    stack.Push(val.ToString());
                    RecordStack(typeof(Scorm1_2.DateTime));
                    stack.Pop();
                }
                else if (inobject.GetType() == typeof(Scorm1_2.TimeSpan))
                {
                    Scorm1_2.TimeSpan val = (Scorm1_2.TimeSpan)inobject;

                    stack.Push(val.ToString());
                    RecordStack(typeof(Scorm1_2.TimeSpan));
                    stack.Pop();
                }
                else if (inobject.GetType() == typeof(Scorm2004.DateTime))
                {
                    Scorm2004.DateTime val = (Scorm2004.DateTime)inobject;

                    stack.Push(val.ToString());
                    RecordStack(typeof(Scorm2004.DateTime));
                    stack.Pop();
                }
                else if (inobject.GetType() == typeof(Scorm2004.TimeSpan))
                {
                    Scorm2004.TimeSpan val = (Scorm2004.TimeSpan)inobject;

                    stack.Push(val.ToString());
                    RecordStack(typeof(Scorm2004.TimeSpan));
                    stack.Pop();
                }
                else
                {
                    stack.Push(inobject.ToString());
                    RecordStack(typeof(string));
                    stack.Pop();
                }
            }
            //Create the set command for this level in the recursion
           
        }
        //Serialize an object
        private void Serialize(object inobject, Type mType)
        {
            //Sanity check - no set required for null objects
            if (inobject == null)
                return;
            ///If it's a simple type, call serialize integral
            if (mType == typeof(string) ||
                mType == typeof(int) ||
                mType == typeof(float) ||
                mType == typeof(bool) ||
                mType == typeof(Scorm2004.DateTime) ||
                mType == typeof(Scorm2004.TimeSpan) ||
                mType == typeof(Scorm1_2.DateTime) ||
                mType == typeof(Scorm1_2.TimeSpan) ||
                mType == typeof(Decimal) ||
                mType.IsEnum
                )
            {
                SerializeIntegral(inobject);
            }
            else if(IsLangStringType(inobject.GetType()))
            {
                SerializeLangString(inobject);
            }
            //It's a complex object, serialize as an object
            else
            {
                SerializeObject(inobject);
            }
        }
        //Serialize a complex object to Set commands
        private void SerializeObject(object inobject)
        {
            //The type of object
            Type otype = inobject.GetType();
            //Walk each member if its an array
            if (otype.FullName.Contains("System.Collections.Generic.List"))
                {
                    Array mia = null;
                    MethodInfo[] methods232 = otype.GetMethods();
                    
                    mia = (Array)(otype.GetMethod("ToArray").Invoke(inobject, null));
                    
                    
                    
                    //Set the length of the array
                    //NOTE: some implementations of SCORM don't let you set the length like this
                 //   stack.Push("_count");
                 //   stack.Push(mia.Length.ToString());
                 //   RecordStack();
                 //   stack.Pop();
                 //   stack.Pop();

                    //Push the index on the identifier name stack and process the object in the array
                    
                    for (int i = 0; i < mia.Length; i++)
                    {
                        stack.Push(i.ToString());
                        Serialize(mia.GetValue(i),mia.GetType().GetElementType());
                        stack.Pop();
                        
                    }
                }
            else{
                //This object is not an array
                //Iterate over all the properties of the object
                PropertyInfo[] members = otype.GetProperties();
                members = SortByPriority(members);
                foreach (PropertyInfo mi in members)
                {
                    if (!mi.Name.EndsWith("Specified"))
                    {
                        //Push the name of the member on the stack and recurse
                        stack.Push(mi.Name);
                        object o = mi.GetValue(inobject, null);
                        if(o != null && SerializeThis(mi))
                            Serialize(o, o.GetType());
                        stack.Pop();
                    }
                    
                }
            }
        }
        private int compare(Object x, Object y)
            {PropertyInfo p1 = (PropertyInfo)x;
                PropertyInfo p2 = (PropertyInfo)y;
                int priority1 = 1000000;
                int priority2 = 1000000;
                foreach (object o in p1.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(ScormSerialization.PriorityAttribute))
                    {
                        priority1 = ((PriorityAttribute)o).Priority;
                    }
                }
                foreach (object o in p2.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(ScormSerialization.PriorityAttribute))
                    {
                        priority2 = ((PriorityAttribute)o).Priority;
                    }
                }
                return priority1 < priority2 ? 1 : 0;}

        //OMG it's bubble sort! - Array.Sort fails under MONO
        private PropertyInfo[] SortByPriority(PropertyInfo[] members)
        {
            bool swapped = false;
            int n = members.Length;
            do
            {
                swapped = false;
                for (int i = 1; i < n-1; i++)
                {
                    if(compare(members[i-1],members[i]) == -1)
                    {
                        PropertyInfo mitemp = members[i - 1];
                        members[i - 1] = members[i];
                        members[i] = mitemp;
                        swapped = true;
                    }
                }
                n--;
            } while (swapped);

       
            return members;
        }
        //List of the set commands for this object
        private List<ScormSet> Serialized;
        //TODO: use this to filter unnecessary set calls when re-serializing same object
        private List<ScormSet> PrevioiusSerialization;
        //The identifier name stack
        Stack<string> stack;
    }
    public enum SetResult {Ok,Error};
    public enum GetResult {Ok,Error};
    public abstract class ScormCommand
    {
        private string identifier;
        private string value;
        private Type ExpectedDataType;

        public abstract string FilterString(string s);
     
        public void SetIdentifier(string s)
        {
            identifier = FilterString(s);
        }
        public void SetValue(string s)
        {
            value = FilterString(s);
        }
        public string GetValue()
        {
            return FilterString(value);
        }
        public string GetIdentifier()
        {
            return FilterString(identifier);
        }
        public Type GetExpectedType()
        {
            return ExpectedDataType;
        }
        public void SetExpectedType(Type edt)
        {
            ExpectedDataType = edt;
        }
    }
    //A command to the scorm wrapper to set a value
    public class ScormSet : ScormCommand
    {
        public override string FilterString(string s)
        {
            if (s == "ab_initio") s = "ab-initio";
            if (s == "no_credit") s = "no-credit";
            if (s == "fill_in") s = "fill-in";
            if (s == "long_fill_in") s = "long-fill-in";
            if (s == "multiple_choice") s = "multiple-choice";
            if (s == "true_false") s = "true-false";
            
            if (s == "not_attempted") s = "not attempted";
            if (s == "continue_message") s = "continue,message";
            if (s == "continue_no_message") s = "continue,no message";
            if (s == "exit_message") s = "exit,message";
            if (s == "exit_no_message") s = "exit,no message";
            if (s == "blank") s = "";
            if (s == "off") s = "-1";
            if (s == "no_change") s = "0";
            if (s == "on") s = "1";
            
                       
            return s;
        }
        public ScormSet(string i, string v, Type EDT)
        {
            SetIdentifier(i);
            SetValue(v);
            SetExpectedType(EDT);
        }
    }
    //A structure return by the wrapper Get command
    public class ScormGet : ScormCommand
    {
        public override string FilterString(string s)
        {
            if (s == "ab-initio") s = "ab_initio";
            if (s == "no-credit") s = "no_credit";
            if (s == "fill-in") s = "fill_in";
            if (s == "long-fill-in") s = "long_fill_in";
            if (s == "multiple-choice") s = "multiple_choice";
            if (s == "true-false") s = "true_false";
            if (s == "continue,message") s = "continue_message";
            if (s == "continue,no message") s = "continue_no_message";
            if (s == "exit,message") s = "exit_message";
            if (s == "exit,no message") s = "exit_no_message";
            if (s == "not attempted") s = "not_attempted";
            return s;
        }
        public ScormGet(string i, string v, Type EDT)
        {
            SetIdentifier(i);
            SetValue(v);
            SetExpectedType(EDT);
        }
        //Convert to string
        public override string ToString()
        {
            return GetValue();
        }
        //convert to int
        public int ToInt()
        {
            try
            {
                if (GetValue() == "null") return 0;
                if (GetValue() == "") return 0;
                return System.Convert.ToInt32(GetValue());
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        //convert to an enum
        public object ToEnum(Type type)
        {
            try{
                if (GetValue() == "null") return Enum.Parse(type, "not_set");
                if (GetValue() == "") return Enum.Parse(type, "not_set");
                return Enum.Parse(type, GetValue());
            }
            catch (Exception e)
            {
                return Enum.GetValues(type).GetValue(0);
            }
        }
        //convert to a bool
        public bool ToBool()
        {
            try
            {
                if (GetValue() == "null") return false;
                if (GetValue() == "") return false;
                return System.Convert.ToBoolean(GetValue());
            }
            catch (Exception e)
            {
                return false;
            }
        }
        //convert to a bool
        public Decimal ToDecimal()
        {
            try
            {
                if (GetValue() == "null") return 0;
                if (GetValue() == "") return 0;
                return System.Convert.ToDecimal(GetValue());
            }
            catch (Exception e)
            {
                return new Decimal(0.0f);
            }
        }
        //Convert to a datetime
        public Scorm2004.DateTime ToDateTime2004()
        {
            if (GetValue() == "null") return new Scorm2004.DateTime(DateTime.Now);
            if (GetValue() == "") return new Scorm2004.DateTime(DateTime.Now);
            try
            {
                return Scorm2004.DateTime.Parse(GetValue());
            }
            catch (Exception e)
            {
                return new Scorm2004.DateTime(DateTime.Now);
            }

        }
        //Convert to a datetime
        public Scorm1_2.DateTime ToDateTime1_2()
        {
            if (GetValue() == "null") return new Scorm1_2.DateTime(DateTime.Now);
            if (GetValue() == "") return new Scorm1_2.DateTime(DateTime.Now);
            try
            {
                return Scorm1_2.DateTime.Parse(GetValue());
            }
            catch (Exception e)
            {
                return new Scorm1_2.DateTime(DateTime.Now);
            }

        }
        //Convert to a datetime
        public Scorm2004.TimeSpan ToTimeSpan2004()
        {
            if (GetValue() == "null") return new Scorm2004.TimeSpan(new TimeSpan());
            if (GetValue() == "") return new Scorm2004.TimeSpan(new TimeSpan());

            try{
                return Scorm2004.TimeSpan.Parse(GetValue());
            }
            catch (Exception e)
            {
                return new Scorm2004.TimeSpan(new TimeSpan());
            }
        }
        public Scorm1_2.TimeSpan ToTimeSpan1_2()
        {
            if (GetValue() == "null") return new Scorm1_2.TimeSpan(new TimeSpan());
            if (GetValue() == "") return new Scorm1_2.TimeSpan(new TimeSpan());

            try
            {
                return Scorm1_2.TimeSpan.Parse(GetValue());
            }
            catch (Exception e)
            {
                return new Scorm1_2.TimeSpan(new TimeSpan());
            }
        }
        
    }
    
    //This is the interface that the Scorm API bridge should use
    //For Unity, create an object that uses this interface, and passes get/set commands out to javascript
    public interface ScormWrapper
    {
        SetResult Set(ScormSet set);
        ScormGet Get(string identifier,Type EDT);
    }
    //This is a ScormWrapper that uses a ScormDatamodelCommandParser to simulate an LMS. 
    //The command parser takes Get/Set commands in the format {get|set identifier [value]}
    public class InMemoryScormSimulator2004 : ScormWrapper
    {
        public ScormDatamodelCommandParser2004 parser;
       
        public InMemoryScormSimulator2004()
        {
            parser = new ScormDatamodelCommandParser2004();
            parser.datamodel = new Scorm2004.DataModel();
            
        }
        public SetResult Set(ScormSet set)
        {
           //  set = mConverter.Map2004_to_12(set);

             string commandstring = set.GetIdentifier() + " " + set.GetValue();
                commandstring = commandstring.Replace("._count", ".length");
                commandstring = commandstring.Substring(4);
                commandstring = "Set " + commandstring;
                parser.Process(commandstring);
                Console.WriteLine(commandstring);
            
            return SetResult.Ok;
        }
        public ScormGet Get(string identifier, Type EDT)
        {
            ScormGet get = new ScormGet(identifier, "", EDT);
          //  get = mConverter.Map2004_to_12(get);

            get.SetIdentifier(get.GetIdentifier().Replace("._count", ".length"));
            get.SetIdentifier(get.GetIdentifier().Substring(4));
            string returnval = parser.Process("Get " + get.GetIdentifier());
            get.SetValue(returnval);
          //  get = mConverter.Map12_to_2004(get);
            Console.WriteLine("Get " + get.GetIdentifier() + " " + get.GetValue());
            
            return get  ;
        }
    }

    //This is a ScormWrapper that uses a ScormDatamodelCommandParser to simulate an LMS. 
    //The command parser takes Get/Set commands in the format {get|set identifier [value]}
    public class InMemoryScormSimulator1_2 : ScormWrapper
    {
        public ScormDatamodelCommandParser1_2 parser;

        public InMemoryScormSimulator1_2()
        {
            parser = new ScormDatamodelCommandParser1_2();
            parser.datamodel = new Scorm1_2.DataModel();

        }
        public SetResult Set(ScormSet set)
        {
            //  set = mConverter.Map2004_to_12(set);

            string commandstring = set.GetIdentifier() + " " + set.GetValue();
            commandstring = commandstring.Replace("._count", ".length");
            commandstring = commandstring.Substring(4);
            commandstring = "Set " + commandstring;
            parser.Process(commandstring);
            Console.WriteLine(commandstring);

            return SetResult.Ok;
        }
        public ScormGet Get(string identifier, Type EDT)
        {
            ScormGet get = new ScormGet(identifier, "", EDT);
            //  get = mConverter.Map2004_to_12(get);

            get.SetIdentifier(get.GetIdentifier().Replace("._count", ".length"));
            get.SetIdentifier(get.GetIdentifier().Substring(4));
            string returnval = parser.Process("Get " + get.GetIdentifier());
            get.SetValue(returnval);
            //  get = mConverter.Map12_to_2004(get);
            Console.WriteLine("Get " + get.GetIdentifier() + " " + get.GetValue());

            return get;
        }
    }
    //This is a ScormWrapper that uses a ScormDatamodelCommandParser to simulate an LMS. 
    //The command parser takes Get/Set commands in the format {get|set identifier [value]}
    public class JavascriptScormSimulator : ScormWrapper
    {
        public ScormDatamodelCommandParser2004 parser;
        public JavascriptScormSimulator()
        {
            parser = new ScormDatamodelCommandParser2004();
            parser.datamodel = new Scorm2004.DataModel();
        }
        public SetResult Set(ScormSet set)
        {
            
            // strip off cmi. for test harness
            string element = set.GetIdentifier().Replace("cmi.", "");

            string commandstring = "Set " + element + " " + "\"" + set.GetValue() + "\"";
            parser.Process(commandstring);
            Console.WriteLine("API_1484_11.SetValue(\"" + set.GetIdentifier() + "\",\"" + set.GetValue() + "\");");
            return SetResult.Ok;
        }
        public ScormGet Get(string identifier,Type EDT)
        {
            // strip off cmi. for test harness
            string element = identifier.Replace("cmi.", "");

            string returnval = parser.Process("Get " + element);
            Console.WriteLine("API_1484_11.GetValue(\"" + identifier + "\");");
            Console.WriteLine(returnval);
            return new ScormGet(identifier, returnval,EDT); ;
        }
    }
}
