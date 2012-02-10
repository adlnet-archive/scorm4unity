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
using Scorm2004;
using Scorm1_2;
namespace ScormSerialization
{
    public abstract class ScormDatamodelCommandParser
    {
      //resize an array if you request a set on the length
        public System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        //Get some default data

        //Walk over an array
        public string RecursiveArray(object obj, Type t, string[] tokens, int index)
        {
            //got to the last token, but still have structure below this node
            if (index >= tokens.Length)
                return "Type is complex. Cannot show as string";

            //Got to an index in the array that is null
            if (obj == null)
                return "Array is null";

            //If the type is an array
            if (t.FullName.Contains("System.Collections.Generic.List") == true)
            {
                Array array = ((Array)(t.GetMethod("ToArray").Invoke(obj, null)));

                //if you requested the length, return the length
                if (tokens[index] == "length")
                    return array.Length.ToString();

                //might have a bad index format
                try
                {
                    //try to get the current token as a int
                    int idx = System.Convert.ToInt32(tokens[index]);

                    //check the bounds
                    if (idx >= array.Length)
                        return "index out of bounds";

                    //Get the object at this position in the array
                    object i = array.GetValue(idx);
                    //If the object in this array is null
                    if (i == null)
                    {
                        //if its a string, create a string
                        if (t.GetElementType() == typeof(String))
                            array.SetValue("", idx);
                        //else create an object of whatever it was suposed to be
                        else
                            array.SetValue(Activator.CreateInstance(t.GetElementType()), idx);
                    }
                    //Get the new value after null check
                    i = array.GetValue(idx);

                    //If its an array
                    if (i.GetType().FullName.Contains("System.Collections.Generic.List") == true)
                        return RecursiveArray(i, i.GetType(), tokens, index + 1);
                    else
                        return RecursiveGet(i, i.GetType(), tokens, index + 1);
                }
                //The format of the array index was incorrect
                catch (System.FormatException e)
                {
                    return "Array index invalid";
                }
            }
            //shoudl not get here, but if we did, then there is no object at this level with the 
            //same name as the token
            return "Error parsing datamodel parameter string";
        }
        protected bool IsLangStringType(Type type)
        {
            if (type.GetProperty("lang") != null && type.GetProperty("Value") != null)
                return true;
            return false;
        }
        protected String LangStringTypeToString(object inobject, Type type)
        {
            string language = (string)type.GetProperty("lang").GetValue(inobject, null);
            string value = (string)type.GetProperty("Value").GetValue(inobject, null);
            return "{lang=" + language + "}" + value;
        }
        protected object SetLangStringTypeFromString(object inobject, Type type, string data)
        {
            if (inobject == null)
                inobject = Activator.CreateInstance(type);
            string temp = data;
            string[] tokens = temp.Split(new string[] { "{lang=", "}" }, StringSplitOptions.RemoveEmptyEntries);
            string language = "en-US";
            string value = tokens[0];
            if (tokens.Length > 1)
                value = tokens[1];
            type.GetProperty("lang").SetValue(inobject, language, null);
            type.GetProperty("Value").SetValue(inobject, value, null);
            return inobject;
        }

        //Walk over the structure of an object, and try to get the value of the string in tokens
        //each level is seperated by dots, like    datamodel.learner.name
        protected string RecursiveGet(object obj, Type t, string[] tokens, int index)
        {
            //if the input is null, return null
            if (obj == null)
                return "null";
            //If the input is a string, return the string
            if (t == typeof(string))
                return (string)obj;
            //if it's a datetime, return it as a string
            if (t == typeof(Scorm2004.DateTime))
                return ((Scorm2004.DateTime)obj).ToString();
            //if it's a datetime, return it as a string
            if (t == typeof(Scorm2004.TimeSpan))
                return ((Scorm2004.TimeSpan)obj).ToString();
            if (t == typeof(Scorm1_2.DateTime))
                return ((Scorm1_2.DateTime)obj).ToString();
            //if it's a datetime, return it as a string
            if (t == typeof(Scorm1_2.TimeSpan))
                return ((Scorm1_2.TimeSpan)obj).ToString();
            //if it's a int, return it as a string
            if (t == typeof(Int32))
                return System.Convert.ToInt32(obj).ToString();
            //if it's a bool, return it as a string
            if (t == typeof(Boolean))
                return System.Convert.ToBoolean(obj).ToString();
            //if it's a enum, return it as a string
            if (t.IsEnum == true)
                return obj.ToString();
            if (IsLangStringType(t))
                return LangStringTypeToString(obj, t);
            //if it's an array, and we still have tokens to process
            if (t.FullName.Contains("System.Collections.Generic.List") && index < tokens.Length)
            {
                //cast to an array
                Array a = (Array)(t.GetMethod("ToArray").Invoke(obj, null));
                //if the token is length, then return the length
                if (tokens[index] == "length")
                    return a.Length.ToString();
                //else, get the token as an int
                int index2 = System.Convert.ToInt32(tokens[index]);
                //check the bounds on the index, if less, get the object at that index and recurse
                if (index2 < a.Length)
                    return RecursiveGet(a.GetValue(index2), a.GetType().GetElementType(), tokens, index + 1);
                else
                    return "Index out of bounds.";
            }
            //If the index is beyond the tokens, then we have found the final level
            //if its greater than or equal to length, then the final level requested is not
            //at a "leaf" of hte object tree. So, we recurse down all branches and print.
            if (index >= tokens.Length)
            {
                //start off with just the tostring of the object
                string ReturnValue = obj.ToString();
                return ReturnValue;
                //get all it's properties
                System.Reflection.PropertyInfo[] propertyInfo2 = t.GetProperties();
                //if the current object is an array
                if (t.FullName.Contains("System.Collections.Generic.List"))
                {
                    //copy the tokens to a new array
                    string[] newtokens = new string[tokens.Length + 1];

                    for (int i = 0; i < tokens.Length; i++)
                        newtokens[i] = tokens[i];

                    //cast to an array
                    Array a = (Array)(t.GetMethod("ToArray").Invoke(obj, null));
                    //write out recursively all the sub parts of the array
                    ReturnValue += "\n";
                    for (int i = 0; i < tokens.Length - 2; i++)
                        ReturnValue += "\t";
                    ReturnValue += "length\t=\t" + a.Length.ToString() + "\n";
                    for (int i = 0; i < a.Length; i++)
                    {
                        newtokens[newtokens.Length - 1] = i.ToString();
                        ReturnValue += "\nindex " + i.ToString() + "\t=\n" + RecursiveGet(a.GetValue(i), t.GetElementType(), newtokens, index + 1);
                    }
                    return ReturnValue + "\n";

                }
                //not an array
                else
                {
                    //for every property
                    foreach (System.Reflection.PropertyInfo info in propertyInfo2)
                    {
                        //copy the tokens to an new array
                        string[] newtokens = new string[tokens.Length + 1];

                        for (int i = 0; i < tokens.Length; i++)
                            newtokens[i] = tokens[i];

                        //Get the value of all of the sub objects
                        newtokens[newtokens.Length - 1] = info.Name;
                        object obj2 = info.GetValue(obj, null);

                        ReturnValue += "\n";
                        for (int i = 0; i < tokens.Length - 2; i++)
                            ReturnValue += "\t";
                        ReturnValue += info.Name + "\t=\t" + RecursiveGet(obj2, info.PropertyType, newtokens, index + 1);
                    }
                }
                return ReturnValue;
            }
            //at this point, we are either at a leaf in the tree and the end of the query, or there is more
            //query and more structure
            System.Reflection.FieldInfo[] fieldInfo = t.GetFields();

            //loop over all the fields of this object
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                //if you find the propety, then output it
                if (info.Name == tokens[index])
                {
                    if (info.FieldType == typeof(int))
                        return info.GetValue(obj).ToString();
                    if (info.FieldType == typeof(string))
                        return info.GetValue(obj).ToString();
                    if (info.FieldType == typeof(bool))
                        return info.GetValue(obj).ToString();
                    if (IsLangStringType(info.FieldType))
                        return LangStringTypeToString(info.GetValue(obj), info.FieldType);
                }
            }

            //loop over all the properties of this object
            System.Reflection.PropertyInfo[] propertyInfo = t.GetProperties();

            foreach (System.Reflection.PropertyInfo info in propertyInfo)
            {
                //When you find the property at the end of the string, output it
                if (info.Name == tokens[index])
                {
                    if (info.GetValue(obj, null) == null)
                        return "null";
                    if (info.PropertyType == typeof(int))
                        return info.GetValue(obj, null).ToString();
                    if (info.PropertyType == typeof(string))
                        return info.GetValue(obj, null).ToString();
                    if (info.PropertyType == typeof(bool))
                        return info.GetValue(obj, null).ToString();
                    if (IsLangStringType(info.PropertyType))
                        return LangStringTypeToString(info.GetValue(obj, null), info.PropertyType);
                    //If it's an array, keep going deeper. 
                    //If the array IS the answer, should have been caught above
                    if (info.PropertyType.FullName.Contains("System.Collections.Generic.List") == true)
                    {
                        return RecursiveGet(info.GetValue(obj, null), info.PropertyType, tokens, index + 1);
                    }
                    //drop down one level deeper, checks at start of function will deal with simple types
                    return RecursiveGet(info.GetValue(obj, null), info.PropertyType, tokens, index + 1);
                }
            }
            //should not get to this point unless one of the tokens in the query does not match the structure
            //of the object
            return "Error parsing datamodel parameter string";
        }

        //works like recursive get. Walks teh tree looking for match to the query string, then sets it
        protected string RecursiveSet(object obj, Type t, string[] tokens, int index)
        {

            //must set a leaf node
            if (index >= tokens.Length)
                return "Type is complex. Cannot show as string";

            System.Reflection.FieldInfo[] fieldInfo = t.GetFields();

            //loop over all the feilds, if you find one that matches, set it
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                if (info.Name == tokens[index])
                {
                    if (info.FieldType == typeof(int))
                        info.SetValue(obj, System.Convert.ToInt32(tokens[tokens.Length - 1]));
                    if (info.FieldType == typeof(string))
                        info.SetValue(obj, tokens[tokens.Length - 1]);
                    if (info.FieldType == typeof(bool))
                        info.SetValue(obj, System.Convert.ToBoolean(tokens[tokens.Length - 1]));
                    if (IsLangStringType(info.FieldType))
                        info.SetValue(obj, SetLangStringTypeFromString(null, info.FieldType, tokens[tokens.Length - 1]));
                }
            }

            System.Reflection.PropertyInfo[] propertyInfo = t.GetProperties();

            //Loop over all of the properties
            bool isMember = false;
            foreach (System.Reflection.PropertyInfo info in propertyInfo)
            {
                //if you find the one that matches, set it
                if (info.Name == tokens[index])
                {
                    isMember = true;

                    if (info.PropertyType == typeof(int))
                    {
                        info.SetValue(obj, System.Convert.ToInt32(tokens[tokens.Length - 1]), null);
                        return "ok";
                    }
                    if (info.PropertyType == typeof(string))
                    {
                        info.SetValue(obj, tokens[tokens.Length - 1], null);
                        return "ok";
                    }
                    if (info.PropertyType == typeof(bool))
                    {
                        info.SetValue(obj, System.Convert.ToBoolean(tokens[tokens.Length - 1]), null);
                        return "ok";
                    }
                    if (IsLangStringType(info.PropertyType))
                    {
                        info.SetValue(obj, SetLangStringTypeFromString(null, info.PropertyType, tokens[tokens.Length - 1]), null);
                    }
                    //unless of course it's an array. The, we have to go deeper
                    if (info.PropertyType.FullName.Contains("System.Collections.Generic.List") == true)
                    {
                        //If the object at this level is null, we have to create one to keep going deeper. 
                        //This allows us to set on an empty array comments.0.comment
                        object array = info.GetValue(obj, null);
                        if (array == null)
                            info.SetValue(obj, Array.CreateInstance(info.PropertyType.GetElementType(), System.Convert.ToInt32(tokens[tokens.Length - 1])), null);
                        array = info.GetValue(obj, null);
                        //If you wanted to set the length of hte array, we have to resize it
                        if (tokens[index + 1] == "length")
                        {
                            //try and catch format for number
                            try
                            {
                                info.SetValue(obj, ResizeArray((Array)array, System.Convert.ToInt32(tokens[index + 2])), null);
                            }
                            catch (System.FormatException e)
                            {
                                return e.Message;
                            }
                            return "ok.";
                        }
                        //Cant set it if it's out of bounds
                        if (System.Convert.ToInt32(tokens[index + 1]) >= ((Array)(array.GetType().GetMethod("ToArray").Invoke(array, null))).Length)
                            return "Index out of bounds.";

                        //Get the value of the object
                        object target1 = ((Array)array).GetValue(System.Convert.ToInt32(tokens[index + 1]));
                        if (target1 == null)
                            if (info.PropertyType.GetElementType() != typeof(string))
                                ((Array)array).SetValue(Activator.CreateInstance(info.PropertyType.GetElementType()), System.Convert.ToInt32(tokens[index + 1]));
                            else
                                ((Array)array).SetValue("", System.Convert.ToInt32(tokens[index + 1]));
                        target1 = ((Array)array).GetValue(System.Convert.ToInt32(tokens[index + 1]));

                        //Set it for simple types
                        if (target1.GetType() == typeof(String))
                        {
                            ((Array)array).SetValue(tokens[tokens.Length - 1], System.Convert.ToInt32(tokens[index + 1]));
                            return "ok";
                        }
                        if (target1.GetType() == typeof(Boolean))
                        {
                            ((Array)array).SetValue(System.Convert.ToBoolean(tokens[tokens.Length - 1]), System.Convert.ToInt32(tokens[index + 1]));
                            return "ok";
                        }
                        if (target1.GetType() == typeof(Int32))
                        {
                            ((Array)array).SetValue(System.Convert.ToInt32(tokens[tokens.Length - 1]), System.Convert.ToInt32(tokens[index + 1]));
                            return "ok";
                        }
                        if (IsLangStringType(target1.GetType()))
                        {
                            ((Array)array).SetValue(SetLangStringTypeFromString(null, target1.GetType(), tokens[tokens.Length - 1]), System.Convert.ToInt32(tokens[index + 1]));
                        }
                        //Not at the end, so continue deeper
                        return RecursiveSet(target1, info.PropertyType.GetElementType(), tokens, index + 2);
                    }

                    //It's not an array, so get the target object
                    object target = info.GetValue(obj, null);

                    //Try to set it to a new instance if it is null
                    if (target == null)
                        info.SetValue(obj, Activator.CreateInstance(info.PropertyType), null);

                    //If its an enum, lookup the right value and set it
                    if (info.PropertyType.IsEnum == true)
                    {
                        foreach (object s in Enum.GetValues(info.PropertyType))
                        {
                            if (tokens[tokens.Length - 1] == s.ToString())
                            {
                                info.SetValue(obj, s, null);
                                return "ok";
                            }
                        }
                    }
                    //didnt set anything, go deeper
                    return RecursiveSet(info.GetValue(obj, null), info.PropertyType, tokens, index + 1);
                }
            }
            //We did not find a member with that name
            if (!isMember)
                return "Error parsing datamodel parameter string";
            else
                return "Complex types cannot be set. Set a sub property.";
        }


        protected string RecursiveShow(object obj, Type t, string[] tokens, int index)
        {
            if (index == tokens.Length)
            {
                if (t.FullName.Contains("System.Collections.Generic.List") == false)
                {
                    System.Reflection.PropertyInfo[] propertyInfo = t.GetProperties();
                    string result = "";
                    foreach (System.Reflection.PropertyInfo info in propertyInfo)
                    {
                        result += info.Name + "\n";
                    }
                    if (result == "")
                    {
                        result = t.Name + "\n";
                        if (t.IsEnum)
                        {
                            foreach (object s in Enum.GetValues(t))
                            {
                                result += s.ToString() + "\n";
                            }
                        }
                    }
                    return result;
                }
                else
                {
                    return "length \n";
                }
            }
            else
            {
                System.Reflection.PropertyInfo[] propertyInfo = t.GetProperties();
                foreach (System.Reflection.PropertyInfo info in propertyInfo)
                {
                    if (info.Name == tokens[index])
                    {
                        if (info.PropertyType.FullName.Contains("System.Collections.Generic.List") == false)
                        {
                            if (info.PropertyType != typeof(String) && info.PropertyType != typeof(Int32) && info.PropertyType != typeof(Boolean))
                                return RecursiveShow(Activator.CreateInstance(info.PropertyType), info.PropertyType, tokens, index + 1);
                            else
                                return info.PropertyType.Name;
                        }
                        else
                        {
                            if (index + 1 == tokens.Length)
                                return RecursiveShow(info.GetValue(obj, null), info.PropertyType, tokens, index + 1);
                            else
                            {
                                int idx = System.Convert.ToInt32(tokens[index + 1]);
                                Array a = (Array)info.GetValue(obj, null);
                                if (info.PropertyType.GetElementType().FullName.Contains("System.Collections.Generic.List") == false)
                                {
                                    if (info.PropertyType.GetElementType() != typeof(String) && info.PropertyType.GetElementType() != typeof(Int32) && info.PropertyType.GetElementType() != typeof(Boolean))
                                        return RecursiveShow(Activator.CreateInstance(info.PropertyType.GetElementType()), info.PropertyType.GetElementType(), tokens, index + 2);
                                    else
                                        return info.PropertyType.GetElementType().Name;


                                }
                                else
                                    return RecursiveShow(Array.CreateInstance(info.PropertyType.GetElementType(), 0), info.PropertyType.GetElementType(), tokens, index + 2);
                            }

                        }
                    }
                }
            }
            return "error";
        }
    }
    public class ScormDatamodelCommandParser1_2 : ScormDatamodelCommandParser
    {
        public Scorm1_2.DataModel datamodel;
        public static Scorm1_2.DataModel GetTestData1_2()
        {
         
            Scorm1_2.DataModel data = new Scorm1_2.DataModel();
            data.comments = "these are comments!";
            data.comments_from_lms = "these are commetns from the lms";
            data.core = new coreType();
            data.core.credit = Scorm1_2.credit.credit;
            data.core.entry = Scorm1_2.entry.ab_initio;
            data.core.lesson_location = "location data";
            data.core.lesson_mode = Scorm1_2.mode.review;
            data.core.lesson_status = lessonStatusType.passed;
            data.core.score = new Scorm1_2.scoreType();
            data.core.score.max = new decimal(12);
            data.core.score.min = new decimal(1);
            data.core.score.raw = new decimal(32);
            data.core.session_time = new Scorm1_2.TimeSpan(new System.TimeSpan(0, 0, 100,1));
            data.core.student_id = "sdasdf";
            data.core.student_name = "Jono";
            data.core.total_time = new Scorm1_2.TimeSpan(new System.TimeSpan(1, 1,1,0));
            data.interactions = new List<Scorm1_2.interactionType>();
            data.interactions.Add(new Scorm1_2.interactionType());
            data.interactions[0].id = "asdf";
            data.interactions[0].latency = "asdf";
            data.interactions[0].objectives = new List<Scorm1_2.objectiveIDType>();
            data.interactions[0].objectives.Add(new Scorm1_2.objectiveIDType("adsffa"));
            data.interactions[0].objectives.Add(new Scorm1_2.objectiveIDType("fdfa"));
            data.interactions[0].result = Scorm1_2.interactionTypeResult.correct;
            data.interactions[0].student_response = new studentResponseTypeStudentResponseFillIn();
            ((studentResponseTypeStudentResponseFillIn)data.interactions[0].student_response).Value = "asdf";
            data.interactions[0].time = new Scorm1_2.DateTime(System.DateTime.Now);
            data.interactions[0].type = Scorm1_2.interactionTypeType.fill_in;
            data.interactions[0].weighting = new decimal(1);


            data.interactions.Add(new Scorm1_2.interactionType());
            data.interactions[1].id = "otherid";
            data.interactions[1].latency = "lat";
            data.interactions[1].objectives = new List<Scorm1_2.objectiveIDType>();
            data.interactions[1].objectives.Add(new Scorm1_2.objectiveIDType("obj0"));
            data.interactions[1].objectives.Add(new Scorm1_2.objectiveIDType("obj1"));
            data.interactions[1].result = Scorm1_2.interactionTypeResult.unanticipated;
            data.interactions[1].student_response = new studentResponseTypeStudentResponseFillIn();
            ((studentResponseTypeStudentResponseFillIn)data.interactions[0].student_response).Value = "otherdata";
            data.interactions[1].time = new Scorm1_2.DateTime(System.DateTime.Now);
            data.interactions[1].type = Scorm1_2.interactionTypeType.fill_in;
            data.interactions[1].weighting = new decimal(14);

            data.launch_data = "asdfasdfasdfasfa";
            data.objectives = new List<Scorm1_2.objectiveType>();
            data.objectives.Add(new Scorm1_2.objectiveType());
            data.objectives[0].id = "asdf";
            data.objectives[0].score = new Scorm1_2.scoreType();
            data.objectives[0].score.max = new decimal(1);
            data.objectives[0].score.min = new decimal(0);
            data.objectives[0].score.raw = new decimal(.5);
            data.objectives[0].status = lessonStatusType.completed;

            data.objectives.Add(new Scorm1_2.objectiveType());
            data.objectives[1].id = "otehr id";
            data.objectives[1].score = new Scorm1_2.scoreType();
            data.objectives[1].score.max = new decimal(.1);
            data.objectives[1].score.min = new decimal(0.4);
            data.objectives[1].score.raw = new decimal(.58);
            data.objectives[1].status = lessonStatusType.incomplete;

            data.objectives.Add(new Scorm1_2.objectiveType());
            data.objectives[2].id = "otehr id2";
            data.objectives[2].score = new Scorm1_2.scoreType();
            data.objectives[2].score.max = new decimal(1.1);
            data.objectives[2].score.min = new decimal(10.4);
            data.objectives[2].score.raw = new decimal(1.58);
            data.objectives[2].status = lessonStatusType.passed;


            data.student_data = new studentDataType();
            data.student_data.mastery_score = new decimal(1);
            data.student_data.max_time_allowed = new Scorm1_2.TimeSpan(new System.TimeSpan(1,1,1,1));
            data.student_data.time_limit_action = Scorm1_2.timeLimitAction.continue_message;
            data.student_preference = new studentPreferenceType();
            data.student_preference.audio = new decimal(0);
            data.student_preference.text = Scorm1_2.learnerPreferenceAudioCaptioning.no_change;
            data.student_preference.language = "engl";
            data.student_preference.speed = 342;
            data.suspend_data = "asdfasdfadsf";



            return data;

        }

        public String Process(string originput)
        {
            string input = "";
            bool inQuotes = false;
            for (int i = 0; i < originput.Length; i++)
            {
                if (originput[i] != ' ' && originput[i] != '"' && !inQuotes)
                    input += originput[i];
                else if (originput[i] == '"')
                    inQuotes = inQuotes == false;
                else if (originput[i] == ' ' && !inQuotes)
                    input += ' ';
                else if (originput[i] == ' ' && inQuotes)
                    input += '^';
                else if (originput[i] != ' ' && inQuotes)
                    input += originput[i];
            }

            string[] tokens = input.Split(new char[] { '|', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < tokens.Length; j++)
            {

                string temp = tokens[j];
                tokens[j] = "";
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] != '^')
                        tokens[j] += temp[i];
                    else
                        tokens[j] += ' ';
                }



            }
            string command = tokens[0];



            //if (command == "Initialize")
            //{
            //    RtwsProviderWrapper mProvider = new RtwsProviderWrapper("http://localhost/RtwsProvider/WebService.asmx");
            //    datamodel = mProvider.Get();
            //    return "ok";
            //}
            //if (command == "Commit")
            //{
            //    RtwsProviderWrapper mProvider = new RtwsProviderWrapper("http://localhost/RtwsProvider/WebService.asmx");
            //    mProvider.Set(datamodel);
            //    return "ok";
            //}
            //if (command == "GetAttempt")
            //{
            //    RtwsProviderWrapper mProvider = new RtwsProviderWrapper("http://localhost/RtwsProvider/WebService.asmx");
            //    datamodel = mProvider.GetAttempt(System.Convert.ToInt32(tokens[1]));
            //    return "ok";
            //}
            if (command == "Get")
            {
                return RecursiveGet(datamodel, datamodel.GetType(), tokens, 1);
            }
            if (command == "Set")
            {
                return RecursiveSet(datamodel, datamodel.GetType(), tokens, 1);
            }
            if (command == "Show")
            {
                return RecursiveShow(datamodel, datamodel.GetType(), tokens, 1);
            }
            return "Unknown Command";
        }

    }
    public class ScormDatamodelCommandParser2004 : ScormDatamodelCommandParser
    {
        public Scorm2004.DataModel datamodel;
        public Scorm2004.DataModel GetTestData2004()
        {

            return new Scorm2004.DataModel();

        }

        public String Process(string originput)
        {
            string input = "";
            bool inQuotes = false;
            for (int i = 0; i < originput.Length; i++)
            {
                if (originput[i] != ' ' && originput[i] != '"' && !inQuotes)
                    input += originput[i];
                else if (originput[i] == '"')
                    inQuotes = inQuotes == false;
                else if (originput[i] == ' ' && !inQuotes)
                    input += ' ';
                else if (originput[i] == ' ' && inQuotes)
                    input += '^';
                else if (originput[i] != ' ' && inQuotes)
                    input += originput[i];
            }

            string[] tokens = input.Split(new char[] { '|', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < tokens.Length; j++)
            {

                string temp = tokens[j];
                tokens[j] = "";
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] != '^')
                        tokens[j] += temp[i];
                    else
                        tokens[j] += ' ';
                }



            }
            string command = tokens[0];



            //if (command == "Initialize")
            //{
            //    RtwsProviderWrapper mProvider = new RtwsProviderWrapper("http://localhost/RtwsProvider/WebService.asmx");
            //    datamodel = mProvider.Get();
            //    return "ok";
            //}
            //if (command == "Commit")
            //{
            //    RtwsProviderWrapper mProvider = new RtwsProviderWrapper("http://localhost/RtwsProvider/WebService.asmx");
            //    mProvider.Set(datamodel);
            //    return "ok";
            //}
            //if (command == "GetAttempt")
            //{
            //    RtwsProviderWrapper mProvider = new RtwsProviderWrapper("http://localhost/RtwsProvider/WebService.asmx");
            //    datamodel = mProvider.GetAttempt(System.Convert.ToInt32(tokens[1]));
            //    return "ok";
            //}
            if (command == "Get")
            {
                return RecursiveGet(datamodel, datamodel.GetType(), tokens, 1);
            }
            if (command == "Set")
            {
                return RecursiveSet(datamodel, datamodel.GetType(), tokens, 1);
            }
            if (command == "Show")
            {
                return RecursiveShow(datamodel, datamodel.GetType(), tokens, 1);
            }
            return "Unknown Command";
        }

    }
}
