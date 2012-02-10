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
public class ScormVersionConversion
{
    public class DataModel
    {
        public static Scorm1_2.DataModel Translate(Scorm2004.DataModel i)
        {
            if (i == null) return null;

            Scorm1_2.DataModel ret = new Scorm1_2.DataModel();
            ret.core = new Scorm1_2.coreType();
            ret.attemptNumber = i.attemptNumber;
            if (i.comments_from_learner.Count > 0) 
                ret.comments =   i.comments_from_learner[0].comment.Value;
            if (i.comments_from_lms.Count > 0) 
                ret.comments_from_lms =   i.comments_from_lms[0].comment.Value;

            if (i.completion_status != Scorm2004.completionStatusType.not_set && i.completion_status != Scorm2004.completionStatusType.unknown)
                ret.core.lesson_status = Enums.Translate(i.completion_status);
            else
                ret.core.lesson_status = Enums.Translate(i.success_status);

            ret.core.credit = Enums.Translate(i.credit);

            ret.core.entry = Enums.Translate(i.entry);
            ret.core.exit = Enums.Translate(i.exit);
            ret.interactions = Arrays.Translate(i.interactions);
            ret.launch_data = i.launch_data;
            ret.core.student_id = i.learner_id;
            ret.core.student_name = Types.Translate(i.learner_name);
            ret.student_preference = Types.Translate(i.learner_preference);
            ret.core.lesson_location = i.location;
            ret.student_data = new Scorm1_2.studentDataType();
            ret.student_data.max_time_allowed = new Scorm1_2.TimeSpan(i.max_time_allowed);
            ret.core.lesson_mode = Enums.Translate(i.mode);
            ret.objectives = Arrays.Translate(i.objectives);
           
        
            ret.core.score = Types.Translate(i.score);
            ret.core.session_time = new Scorm1_2.TimeSpan(i.session_time);
            
            ret.suspend_data = i.suspend_data;
            ret.student_data.time_limit_action = Enums.Translate(i.time_limit_action);
            ret.core.total_time = new Scorm1_2.TimeSpan(i.total_time);
            return ret;
        }

        public static Scorm2004.DataModel Translate(Scorm1_2.DataModel i)
        {
            if (i == null) return null;

            Scorm2004.DataModel ret = new Scorm2004.DataModel();
            ret.learner_name = new Scorm2004.learnerName();
            ret.score = new Scorm2004.scoreType();
            ret.learner_preference = new Scorm2004.learnerPreferenceType();
            
            ret.attemptNumber = i.attemptNumber;

            if (i.comments_from_lms != "" && i.comments_from_lms != null)
            {
                ret.comments_from_lms = new System.Collections.Generic.List<Scorm2004.commentType>();
                ret.comments_from_lms.Add(new Scorm2004.commentType());
                ret.comments_from_lms[0].comment = new Scorm2004.commentTypeComment();
                ret.comments_from_lms[0].comment.Value = i.comments_from_lms;
                ret.comments_from_lms[0].id = "Comment:1";
                ret.comments_from_lms[0].location = "";
                ret.comments_from_lms[0].timestamp = new Scorm2004.DateTime(System.DateTime.Now);
            }

            if (i.comments != "" && i.comments != null)
            {
                ret.comments_from_learner = new System.Collections.Generic.List<Scorm2004.commentType>();
                ret.comments_from_learner.Add(new Scorm2004.commentType());
                ret.comments_from_learner[0].comment = new Scorm2004.commentTypeComment();
                ret.comments_from_learner[0].comment.Value = i.comments;
                ret.comments_from_learner[0].id = "Comment:1";
                ret.comments_from_learner[0].location = "";
                ret.comments_from_learner[0].timestamp = new Scorm2004.DateTime(System.DateTime.Now);
            }
            if (i.core != null)
            {
                ret.completion_status = Enums.Translate(i.core.lesson_status);
                ret.credit = Enums.Translate(i.core.credit);
                ret.entry = Enums.Translate(i.core.entry);
                ret.exit = Enums.Translate(i.core.exit);
                ret.learner_id = i.core.student_id;
                ret.learner_name.Value = i.core.student_name;
                ret.mode = Enums.Translate(i.core.lesson_mode);
                if(i.core.score != null)
                    ret.score = Types.Translate(i.core.score);
                ret.session_time = new Scorm2004.TimeSpan(i.core.session_time);
                ret.total_time = new Scorm2004.TimeSpan(i.core.total_time);
                ret.location = i.core.lesson_location;

                if (i.core.lesson_status == Scorm1_2.lessonStatusType.failed)
                    ret.success_status = Scorm2004.successStatusType.failed;
                else if (i.core.lesson_status == Scorm1_2.lessonStatusType.passed)
                    ret.success_status = Scorm2004.successStatusType.passed;
                else
                    ret.success_status = Scorm2004.successStatusType.not_set;

            }
            if (i.student_data != null)
            {
                ret.scaled_passing_score = i.student_data.mastery_score;
                ret.time_limit_action = Enums.Translate(i.student_data.time_limit_action);
                ret.max_time_allowed = new Scorm2004.TimeSpan(i.student_data.max_time_allowed);
            }

            ret.dataModelVersion = "Scorm 2004";
            
            if(i.interactions != null)
                ret.interactions = Arrays.Translate(i.interactions);

            ret.launch_data = i.launch_data;
            
            ret.learner_name.lang = "en-US";

            if(i.student_preference != null)
                ret.learner_preference = Types.Translate(i.student_preference);

            if(i.objectives != null)
                ret.objectives = Arrays.Translate(i.objectives);

            ret.suspend_data = i.suspend_data;

            return ret;

        }
    }
    public class Types
    {

        public static Scorm1_2.objectiveType Translate(Scorm2004.objectiveType i)
        {
            if (i == null) return null;

            Scorm1_2.objectiveType ret = new Scorm1_2.objectiveType();
            if(i.success_status == Scorm2004.successStatusType.passed || i.success_status == Scorm2004.successStatusType.failed)
                ret.status = Enums.Translate(i.success_status);
            else
                ret.status = Enums.Translate(i.completion_status);
            ret.id = i.id;
           
            ret.score = Types.Translate(i.score);
           
            return ret;
        }
        public static Scorm1_2.interactionType Translate(Scorm2004.interactionType i)
        {
            if (i == null) return null;

            Scorm1_2.interactionType ret = new Scorm1_2.interactionType();
            ret.attemptNumber = i.attemptNumber;
            ret.correct_responses = Types.Translate(i.correct_responses);
       
            ret.id = i.id;
            ret.latency = i.latency;
            ret.student_response = Types.Translate(i.learner_response);
            ret.objectives = Arrays.Translate(i.objectives);
            ret.result = Enums.Translate(i.result);
            ret.time = new Scorm1_2.DateTime(i.timestamp);
            ret.type = Enums.Translate(i.type);
            ret.weighting = i.weighting;
            return ret;
        }
        /// <summary>
        /// Not yet complete
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Scorm1_2.correctResponsesType Translate(Scorm2004.correctResponsesType i)
        {
            if (i == null) return null;

            Scorm1_2.correctResponsesType ret = new Scorm1_2.correctResponsesType();
            
            ret.Items = i.Items;
            ret.ItemsElementName = Arrays.Translate(i.ItemsElementName);
            return ret;
        }
        /// <summary>
        /// only supporting LongFillin and Fillin
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Scorm1_2.studentResponseType Translate(Scorm2004.learnerResponseType i)
        {
            if (i == null) return null;

            if(i.GetType() == typeof(Scorm2004.learnerResponseTypeLearnerResponseFillIn))
            {
                Scorm1_2.studentResponseTypeStudentResponseFillIn ret = new Scorm1_2.studentResponseTypeStudentResponseFillIn();
                Scorm2004.learnerResponseTypeLearnerResponseFillIn val = (Scorm2004.learnerResponseTypeLearnerResponseFillIn)i;
                ret.lang = val.lang;
                ret.Value = val.Value;
                return ret;
            }
            if (i.GetType() == typeof(Scorm2004.learnerResponseTypeLearnerResponseLongFillIn))
            {
                Scorm1_2.studentResponseTypeStudentResponseFillIn ret = new Scorm1_2.studentResponseTypeStudentResponseFillIn();
                Scorm2004.learnerResponseTypeLearnerResponseLongFillIn val = (Scorm2004.learnerResponseTypeLearnerResponseLongFillIn)i;
                ret.lang = val.lang;
                ret.Value = val.Value;
                return ret;
            }
            return null;
        }
        public static Scorm1_2.scoreType Translate(Scorm2004.scoreType i)
        {
            if (i == null) return null;

            Scorm1_2.scoreType ret = new Scorm1_2.scoreType();
            ret.max = i.max;
            ret.min = i.min;
            ret.raw = i.raw;
          
            return ret;
        }
        public static Scorm1_2.objectiveIDType Translate(Scorm2004.objectiveIDType i)
        {
            if (i == null) return null;

            Scorm1_2.objectiveIDType ret = new Scorm1_2.objectiveIDType();
            ret.id = i.id;
            return ret;
        }
        public static string Translate(Scorm2004.learnerName i)
        {
            if (i == null) return null;

            string ret = i.Value;
  
            return ret;
        }
        public static Scorm1_2.studentPreferenceType Translate(Scorm2004.learnerPreferenceType i)
        {
            if (i == null) return null;

            Scorm1_2.studentPreferenceType ret = new Scorm1_2.studentPreferenceType();
            ret.text = Enums.Translate(i.audio_captioning);
            ret.audio = i.audio_level;
            ret.speed = i.delivery_speed;
            ret.language = i.language;
            return ret;
        }
        public static Scorm2004.objectiveType Translate(Scorm1_2.objectiveType i)
        {
            if (i == null) return null;

            Scorm2004.objectiveType ret = new Scorm2004.objectiveType();
            ret.completion_status = Enums.Translate(i.status);
            ret.description = new Scorm2004.objectiveTypeDescription();
            ret.description.lang = "en-US";
            ret.description.Value = "";
            ret.id = i.id;
            ret.progress_measure = null;
            ret.score = Types.Translate(i.score);
            ret.success_status = Enums.TranslateSuccess(i.status);
            return ret;
        }
        public static Scorm2004.interactionType Translate(Scorm1_2.interactionType i)
        {
            if (i == null) return null;

            Scorm2004.interactionType ret = new Scorm2004.interactionType();
            ret.attemptNumber = i.attemptNumber;
            ret.correct_responses = Types.Translate(i.correct_responses);
            ret.description = new Scorm2004.interactionTypeDescription();
            ret.description.lang = "en-US";
            ret.description.Value = "";
            ret.id = i.id;
            ret.latency = i.latency;
            ret.learner_response = Types.Translate(i.student_response);
            ret.objectives = Arrays.Translate(i.objectives);
            ret.result = Enums.Translate(i.result);
            ret.timestamp = new Scorm2004.DateTime(i.time);
            ret.type = Enums.Translate(i.type);
            ret.weighting = i.weighting;
            return ret;
        }
        /// <summary>
        /// Not yet complete
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Scorm2004.correctResponsesType Translate(Scorm1_2.correctResponsesType i)
        {
            if (i == null) return null;

            Scorm2004.correctResponsesType ret = new Scorm2004.correctResponsesType();

            ret.Items = i.Items;
            ret.ItemsElementName = Arrays.Translate(i.ItemsElementName);
            return ret;
        }
        /// <summary>
        /// only supporting LongFillin and Fillin
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Scorm2004.learnerResponseType Translate(Scorm1_2.studentResponseType i)
        {
            if (i == null) return null;

            if (i.GetType() == typeof(Scorm1_2.studentResponseTypeStudentResponseFillIn))
            {
                Scorm2004.learnerResponseTypeLearnerResponseFillIn ret = new Scorm2004.learnerResponseTypeLearnerResponseFillIn();
                Scorm1_2.studentResponseTypeStudentResponseFillIn val = (Scorm1_2.studentResponseTypeStudentResponseFillIn)i;
                ret.lang = val.lang;
                ret.Value = val.Value;
                return ret;
            }
          
            return null;
        }
        public static Scorm2004.scoreType Translate(Scorm1_2.scoreType i)
        {
            if (i == null) return null;

            Scorm2004.scoreType ret = new Scorm2004.scoreType();
            ret.max = i.max;
            ret.min = i.min;
            ret.raw = i.raw;
            ret.scaled = (i.raw - i.min) / (i.max - i.min);
            if(ret.scaled.HasValue)
                ret.scaled = System.Math.Round(ret.scaled.Value * 100) / 100;
            return ret;
        }
        public static Scorm2004.objectiveIDType Translate(Scorm1_2.objectiveIDType i)
        {
            if (i == null) return null;

            Scorm2004.objectiveIDType ret = new Scorm2004.objectiveIDType();
            ret.id = i.id;
            return ret;
        }
        public static Scorm2004.learnerName Translate(string i)
        {
            if (i == null) return null;

            Scorm2004.learnerName ret = new Scorm2004.learnerName();
            ret.lang = "en-US";
            ret.Value = i;
            return ret;
        }
        public static Scorm2004.learnerPreferenceType Translate(Scorm1_2.studentPreferenceType i)
        {
            if (i == null) return null;

            Scorm2004.learnerPreferenceType ret = new Scorm2004.learnerPreferenceType();
            ret.audio_captioning = Enums.Translate(i.text);
            ret.audio_level = i.audio;
            ret.delivery_speed = i.speed;
            ret.language = i.language;
            return ret;
        }
    }
    public class Arrays
    {
        public static System.Collections.Generic.List<Scorm1_2.ItemsChoiceType> Translate(System.Collections.Generic.List<Scorm2004.ItemsChoiceType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm1_2.ItemsChoiceType> ret = new System.Collections.Generic.List<Scorm1_2.ItemsChoiceType>();
            foreach (Scorm2004.ItemsChoiceType item in i)
            {
                ret.Add(Enums.Translate(item));
            }
            return ret;
        }
        public static System.Collections.Generic.List<Scorm1_2.objectiveType> Translate(System.Collections.Generic.List<Scorm2004.objectiveType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm1_2.objectiveType> ret = new System.Collections.Generic.List<Scorm1_2.objectiveType>();
            foreach (Scorm2004.objectiveType objective in i)
            {
                ret.Add(Types.Translate(objective));
            }
            return ret;
        }
        public static System.Collections.Generic.List<Scorm1_2.interactionType> Translate(System.Collections.Generic.List<Scorm2004.interactionType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm1_2.interactionType> ret = new System.Collections.Generic.List<Scorm1_2.interactionType>();
            foreach (Scorm2004.interactionType interaction in i)
            {
                ret.Add(Types.Translate(interaction));
            }
            return ret;
        }
        public static System.Collections.Generic.List<Scorm1_2.objectiveIDType> Translate(System.Collections.Generic.List<Scorm2004.objectiveIDType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm1_2.objectiveIDType> ret = new System.Collections.Generic.List<Scorm1_2.objectiveIDType>();
            foreach (Scorm2004.objectiveIDType objectiveID in i)
            {
                ret.Add(Types.Translate(objectiveID));
            }
            return ret;
        }






        public static System.Collections.Generic.List<Scorm2004.ItemsChoiceType> Translate(System.Collections.Generic.List<Scorm1_2.ItemsChoiceType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm2004.ItemsChoiceType> ret = new System.Collections.Generic.List<Scorm2004.ItemsChoiceType>();
            foreach (Scorm1_2.ItemsChoiceType item in i)
            {
                ret.Add(Enums.Translate(item));
            }
            return ret;
        }
        public static System.Collections.Generic.List<Scorm2004.objectiveType> Translate(System.Collections.Generic.List<Scorm1_2.objectiveType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm2004.objectiveType> ret = new System.Collections.Generic.List<Scorm2004.objectiveType>();
            foreach (Scorm1_2.objectiveType objective in i)
            {
                ret.Add(Types.Translate(objective));
            }
            return ret;
        }
        public static System.Collections.Generic.List<Scorm2004.interactionType> Translate(System.Collections.Generic.List<Scorm1_2.interactionType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm2004.interactionType> ret = new System.Collections.Generic.List<Scorm2004.interactionType>();
            foreach (Scorm1_2.interactionType interaction in i)
            {
                ret.Add(Types.Translate(interaction));
            }
            return ret;
        }
        public static System.Collections.Generic.List<Scorm2004.objectiveIDType> Translate(System.Collections.Generic.List<Scorm1_2.objectiveIDType> i)
        {
            if (i == null) return null;

            System.Collections.Generic.List<Scorm2004.objectiveIDType> ret = new System.Collections.Generic.List<Scorm2004.objectiveIDType>();
            foreach (Scorm1_2.objectiveIDType objectiveID in i)
            {
                ret.Add(Types.Translate(objectiveID));
            }
            return ret;
        }
    }
    public class Enums
    {
        public static Scorm2004.completionStatusType Translate(Scorm1_2.lessonStatusType i)
        {
            if (i == Scorm1_2.lessonStatusType.completed) return Scorm2004.completionStatusType.completed;
            if (i == Scorm1_2.lessonStatusType.incomplete) return Scorm2004.completionStatusType.incomplete;
            if (i == Scorm1_2.lessonStatusType.not_attempted) return Scorm2004.completionStatusType.not_attempted;
            
            return Scorm2004.completionStatusType.not_set;
        }
        public static Scorm2004.successStatusType TranslateSuccess(Scorm1_2.lessonStatusType i)
        {
            if (i == Scorm1_2.lessonStatusType.failed) return Scorm2004.successStatusType.failed;
            if (i == Scorm1_2.lessonStatusType.passed) return Scorm2004.successStatusType.passed;

            return Scorm2004.successStatusType.not_set;
        }
        public static Scorm2004.credit Translate(Scorm1_2.credit i)
        {
            if (i == Scorm1_2.credit.credit) return Scorm2004.credit.credit;
            if (i == Scorm1_2.credit.no_credit) return Scorm2004.credit.no_credit;
           
            return Scorm2004.credit.not_set;
        }
        public static Scorm2004.entry Translate(Scorm1_2.entry i)
        {
            if (i == Scorm1_2.entry.ab_initio) return Scorm2004.entry.ab_initio;
            if (i == Scorm1_2.entry.other) return Scorm2004.entry.other;
            if (i == Scorm1_2.entry.resume) return Scorm2004.entry.resume;
            

            return Scorm2004.entry.not_set;
        }
        public static Scorm2004.exit Translate(Scorm1_2.exit i)
        {
            if (i == Scorm1_2.exit.Item) return Scorm2004.exit.Item;
            if (i == Scorm1_2.exit.logout) return Scorm2004.exit.logout;
            if (i == Scorm1_2.exit.blank) return Scorm2004.exit.normal;
            if (i == Scorm1_2.exit.suspend) return Scorm2004.exit.suspend;
            if (i == Scorm1_2.exit.time_out) return Scorm2004.exit.timeout;
           

            return Scorm2004.exit.not_set;
        }
        public static Scorm2004.interactionTypeResult Translate(Scorm1_2.interactionTypeResult i)
        {
            if (i == Scorm1_2.interactionTypeResult.correct) return Scorm2004.interactionTypeResult.correct;
            if (i == Scorm1_2.interactionTypeResult.wrong) return Scorm2004.interactionTypeResult.incorrect;
            if (i == Scorm1_2.interactionTypeResult.neutral) return Scorm2004.interactionTypeResult.neutral;
            if (i == Scorm1_2.interactionTypeResult.unanticipated) return Scorm2004.interactionTypeResult.unanticipated;

            return Scorm2004.interactionTypeResult.not_set;
        }
        public static Scorm2004.interactionTypeType Translate(Scorm1_2.interactionTypeType i)
        {
            if (i == Scorm1_2.interactionTypeType.fill_in) return Scorm2004.interactionTypeType.long_fill_in;
            if (i == Scorm1_2.interactionTypeType.likert) return Scorm2004.interactionTypeType.likert;
       //     if (i == Scorm1_2.interactionTypeType.long_fill_in) return Scorm2004.interactionTypeType.long_fill_in;
            if (i == Scorm1_2.interactionTypeType.matching) return Scorm2004.interactionTypeType.matching;
            if (i == Scorm1_2.interactionTypeType.multiple_choice) return Scorm2004.interactionTypeType.multiple_choice;
            if (i == Scorm1_2.interactionTypeType.numeric) return Scorm2004.interactionTypeType.numeric;
       //     if (i == Scorm1_2.interactionTypeType.other) return Scorm2004.interactionTypeType.other;
            if (i == Scorm1_2.interactionTypeType.performance) return Scorm2004.interactionTypeType.performance;
            if (i == Scorm1_2.interactionTypeType.sequencing) return Scorm2004.interactionTypeType.sequencing;
            if (i == Scorm1_2.interactionTypeType.true_false) return Scorm2004.interactionTypeType.true_false;

            return Scorm2004.interactionTypeType.not_set;
        }
        public static Scorm2004.ItemsChoiceType Translate(Scorm1_2.ItemsChoiceType i)
        {
            if (i == Scorm1_2.ItemsChoiceType.correctResponseFillIn) return Scorm2004.ItemsChoiceType.correctResponseLongFillIn;
            if (i == Scorm1_2.ItemsChoiceType.correctResponseLikert) return Scorm2004.ItemsChoiceType.correctResponseLikert;
           // if (i == Scorm1_2.ItemsChoiceType.correctResponseLongFillIn) return Scorm2004.ItemsChoiceType.correctResponseLongFillIn;
            if (i == Scorm1_2.ItemsChoiceType.correctResponseMatching) return Scorm2004.ItemsChoiceType.correctResponseMatching;
            if (i == Scorm1_2.ItemsChoiceType.correctResponseMultipleChoice) return Scorm2004.ItemsChoiceType.correctResponseMultipleChoice;
            if (i == Scorm1_2.ItemsChoiceType.correctResponseNumeric) return Scorm2004.ItemsChoiceType.correctResponseNumeric;
          //  if (i == Scorm1_2.ItemsChoiceType.correctResponseOther) return Scorm2004.ItemsChoiceType.correctResponseOther;
            if (i == Scorm1_2.ItemsChoiceType.correctResponsePerformance) return Scorm2004.ItemsChoiceType.correctResponsePerformance;
            if (i == Scorm1_2.ItemsChoiceType.correctResponseSequencing) return Scorm2004.ItemsChoiceType.correctResponseSequencing;
            if (i == Scorm1_2.ItemsChoiceType.correctResponseTrueFalse) return Scorm2004.ItemsChoiceType.correctResponseTrueFalse;
            return Scorm2004.ItemsChoiceType.not_set;
        }
        public static Scorm2004.ItemsChoiceType1 Translate(Scorm1_2.ItemsChoiceType1 i)
        {
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseFillIn) return Scorm2004.ItemsChoiceType1.learnerResponseLongFillIn;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseLikert) return Scorm2004.ItemsChoiceType1.learnerResponseLikert;
           // if (i == Scorm1_2.ItemsChoiceType1.studentResponseLongFillIn) return Scorm2004.ItemsChoiceType1.learnerResponseLongFillIn;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseMatching) return Scorm2004.ItemsChoiceType1.learnerResponseMatching;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseMultipleChoice) return Scorm2004.ItemsChoiceType1.learnerResponseMultipleChoice;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseNumeric) return Scorm2004.ItemsChoiceType1.learnerResponseNumeric;
          //  if (i == Scorm1_2.ItemsChoiceType1.studentResponseOther) return Scorm2004.ItemsChoiceType1.learnerResponseOther;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponsePerformance) return Scorm2004.ItemsChoiceType1.learnerResponsePerformance;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseSequencing) return Scorm2004.ItemsChoiceType1.learnerResponseSequencing;
            if (i == Scorm1_2.ItemsChoiceType1.studentResponseTrueFalse) return Scorm2004.ItemsChoiceType1.learnerResponseTrueFalse;


            return Scorm2004.ItemsChoiceType1.not_set;
        }
        public static Scorm2004.learnerPreferenceAudioCaptioning Translate(Scorm1_2.learnerPreferenceAudioCaptioning i)
        {
            if (i == Scorm1_2.learnerPreferenceAudioCaptioning.no_change) return Scorm2004.learnerPreferenceAudioCaptioning.no_change;
            if (i == Scorm1_2.learnerPreferenceAudioCaptioning.off) return Scorm2004.learnerPreferenceAudioCaptioning.off;
            if (i == Scorm1_2.learnerPreferenceAudioCaptioning.on) return Scorm2004.learnerPreferenceAudioCaptioning.on;

            return Scorm2004.learnerPreferenceAudioCaptioning.not_set;
        }
        public static Scorm2004.mode Translate(Scorm1_2.mode i)
        {
            if (i == Scorm1_2.mode.browse) return Scorm2004.mode.browse;
            if (i == Scorm1_2.mode.normal) return Scorm2004.mode.normal;
            if (i == Scorm1_2.mode.review) return Scorm2004.mode.review;


            return Scorm2004.mode.not_set;
        }
        public static Scorm2004.timeLimitAction Translate(Scorm1_2.timeLimitAction i)
        {
            if (i == Scorm1_2.timeLimitAction.continue_message) return Scorm2004.timeLimitAction.continue_message;
            if (i == Scorm1_2.timeLimitAction.continue_no_message) return Scorm2004.timeLimitAction.continue_no_message;
            if (i == Scorm1_2.timeLimitAction.exit_message) return Scorm2004.timeLimitAction.exit_message;
            if (i == Scorm1_2.timeLimitAction.exit_no_message) return Scorm2004.timeLimitAction.exit_no_message;

            return Scorm2004.timeLimitAction.not_set;
        }
   
        public static Scorm1_2.lessonStatusType Translate(Scorm2004.completionStatusType i)
        {
            if (i == Scorm2004.completionStatusType.completed) return Scorm1_2.lessonStatusType.completed;
            if (i == Scorm2004.completionStatusType.incomplete) return Scorm1_2.lessonStatusType.incomplete;
            if (i == Scorm2004.completionStatusType.not_attempted) return Scorm1_2.lessonStatusType.not_attempted;
            if (i == Scorm2004.completionStatusType.unknown) return Scorm1_2.lessonStatusType.not_set;

            return Scorm1_2.lessonStatusType.not_set;
        }
        public static Scorm1_2.credit Translate(Scorm2004.credit i)
        {
            if (i == Scorm2004.credit.credit) return Scorm1_2.credit.credit;
            if (i == Scorm2004.credit.no_credit) return Scorm1_2.credit.no_credit;

            return Scorm1_2.credit.not_set;
        }
        public static Scorm1_2.entry Translate(Scorm2004.entry i)
        {
            if (i == Scorm2004.entry.ab_initio) return Scorm1_2.entry.ab_initio;
            if (i == Scorm2004.entry.other) return Scorm1_2.entry.other;
            if (i == Scorm2004.entry.resume) return Scorm1_2.entry.resume;


            return Scorm1_2.entry.not_set;
        }
        public static Scorm1_2.exit Translate(Scorm2004.exit i)
        {
            if (i == Scorm2004.exit.Item) return Scorm1_2.exit.Item;
            if (i == Scorm2004.exit.logout) return Scorm1_2.exit.logout;
            if (i == Scorm2004.exit.normal) return Scorm1_2.exit.blank;
            if (i == Scorm2004.exit.suspend) return Scorm1_2.exit.suspend;
            if (i == Scorm2004.exit.timeout) return Scorm1_2.exit.time_out;


            return Scorm1_2.exit.not_set;
        }
        public static Scorm1_2.interactionTypeResult Translate(Scorm2004.interactionTypeResult i)
        {
            if (i == Scorm2004.interactionTypeResult.correct) return Scorm1_2.interactionTypeResult.correct;
            if (i == Scorm2004.interactionTypeResult.incorrect) return Scorm1_2.interactionTypeResult.wrong;
            if (i == Scorm2004.interactionTypeResult.neutral) return Scorm1_2.interactionTypeResult.neutral;
            if (i == Scorm2004.interactionTypeResult.unanticipated) return Scorm1_2.interactionTypeResult.unanticipated;

            return Scorm1_2.interactionTypeResult.not_set;
        }
        public static Scorm1_2.interactionTypeType Translate(Scorm2004.interactionTypeType i)
        {
            if (i == Scorm2004.interactionTypeType.fill_in) return Scorm1_2.interactionTypeType.fill_in;
            if (i == Scorm2004.interactionTypeType.likert) return Scorm1_2.interactionTypeType.likert;
            if (i == Scorm2004.interactionTypeType.long_fill_in) return Scorm1_2.interactionTypeType.fill_in;
            if (i == Scorm2004.interactionTypeType.matching) return Scorm1_2.interactionTypeType.matching;
            if (i == Scorm2004.interactionTypeType.multiple_choice) return Scorm1_2.interactionTypeType.multiple_choice;
            if (i == Scorm2004.interactionTypeType.numeric) return Scorm1_2.interactionTypeType.numeric;
          //  if (i == Scorm2004.interactionTypeType.other) return Scorm1_2.interactionTypeType.other;
            if (i == Scorm2004.interactionTypeType.performance) return Scorm1_2.interactionTypeType.performance;
            if (i == Scorm2004.interactionTypeType.sequencing) return Scorm1_2.interactionTypeType.sequencing;
            if (i == Scorm2004.interactionTypeType.true_false) return Scorm1_2.interactionTypeType.true_false;

            return Scorm1_2.interactionTypeType.not_set;
        }
        public static Scorm1_2.ItemsChoiceType Translate(Scorm2004.ItemsChoiceType i)
        {
            if (i == Scorm2004.ItemsChoiceType.correctResponseFillIn) return Scorm1_2.ItemsChoiceType.correctResponseFillIn;
            if (i == Scorm2004.ItemsChoiceType.correctResponseLikert) return Scorm1_2.ItemsChoiceType.correctResponseLikert;
            if (i == Scorm2004.ItemsChoiceType.correctResponseLongFillIn) return Scorm1_2.ItemsChoiceType.correctResponseFillIn;
            if (i == Scorm2004.ItemsChoiceType.correctResponseMatching) return Scorm1_2.ItemsChoiceType.correctResponseMatching;
            if (i == Scorm2004.ItemsChoiceType.correctResponseMultipleChoice) return Scorm1_2.ItemsChoiceType.correctResponseMultipleChoice;
            if (i == Scorm2004.ItemsChoiceType.correctResponseNumeric) return Scorm1_2.ItemsChoiceType.correctResponseNumeric;
          //  if (i == Scorm2004.ItemsChoiceType.correctResponseOther) return Scorm1_2.ItemsChoiceType.correctResponseOther;
            if (i == Scorm2004.ItemsChoiceType.correctResponsePerformance) return Scorm1_2.ItemsChoiceType.correctResponsePerformance;
            if (i == Scorm2004.ItemsChoiceType.correctResponseSequencing) return Scorm1_2.ItemsChoiceType.correctResponseSequencing;
            if (i == Scorm2004.ItemsChoiceType.correctResponseTrueFalse) return Scorm1_2.ItemsChoiceType.correctResponseTrueFalse;
            return Scorm1_2.ItemsChoiceType.not_set;
        }
        public static Scorm1_2.ItemsChoiceType1 Translate(Scorm2004.ItemsChoiceType1 i)
        {
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseLongFillIn) return Scorm1_2.ItemsChoiceType1.studentResponseFillIn;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseLikert) return Scorm1_2.ItemsChoiceType1.studentResponseLikert;
            //if (i == Scorm2004.ItemsChoiceType1.learnerResponseLongFillIn) return Scorm1_2.ItemsChoiceType1.studentResponseLongFillIn;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseMatching) return Scorm1_2.ItemsChoiceType1.studentResponseMatching;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseMultipleChoice) return Scorm1_2.ItemsChoiceType1.studentResponseMultipleChoice;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseNumeric) return Scorm1_2.ItemsChoiceType1.studentResponseNumeric;
            //if (i == Scorm2004.ItemsChoiceType1.learnerResponseOther) return Scorm1_2.ItemsChoiceType1.studentResponseOther;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponsePerformance) return Scorm1_2.ItemsChoiceType1.studentResponsePerformance;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseSequencing) return Scorm1_2.ItemsChoiceType1.studentResponseSequencing;
            if (i == Scorm2004.ItemsChoiceType1.learnerResponseTrueFalse) return Scorm1_2.ItemsChoiceType1.studentResponseTrueFalse;


            return Scorm1_2.ItemsChoiceType1.not_set;
        }
        public static Scorm1_2.learnerPreferenceAudioCaptioning Translate(Scorm2004.learnerPreferenceAudioCaptioning i)
        {
            if (i == Scorm2004.learnerPreferenceAudioCaptioning.no_change) return Scorm1_2.learnerPreferenceAudioCaptioning.no_change;
            if (i == Scorm2004.learnerPreferenceAudioCaptioning.off) return Scorm1_2.learnerPreferenceAudioCaptioning.off;
            if (i == Scorm2004.learnerPreferenceAudioCaptioning.on) return Scorm1_2.learnerPreferenceAudioCaptioning.on;

            return Scorm1_2.learnerPreferenceAudioCaptioning.not_set;
        }
        public static Scorm1_2.mode Translate(Scorm2004.mode i)
        {
            if (i == Scorm2004.mode.browse) return Scorm1_2.mode.browse;
            if (i == Scorm2004.mode.normal) return Scorm1_2.mode.normal;
            if (i == Scorm2004.mode.review) return Scorm1_2.mode.review;


            return Scorm1_2.mode.not_set;
        }
        public static Scorm1_2.lessonStatusType Translate(Scorm2004.successStatusType i)
        {
            if (i == Scorm2004.successStatusType.failed) return Scorm1_2.lessonStatusType.failed;
            if (i == Scorm2004.successStatusType.passed) return Scorm1_2.lessonStatusType.passed;
            if (i == Scorm2004.successStatusType.unknown) return Scorm1_2.lessonStatusType.not_set;

            return Scorm1_2.lessonStatusType.not_set;
        }
        public static Scorm1_2.timeLimitAction Translate(Scorm2004.timeLimitAction i)
        {
            if (i == Scorm2004.timeLimitAction.continue_message) return Scorm1_2.timeLimitAction.continue_message;
            if (i == Scorm2004.timeLimitAction.continue_no_message) return Scorm1_2.timeLimitAction.continue_no_message;
            if (i == Scorm2004.timeLimitAction.exit_message) return Scorm1_2.timeLimitAction.exit_message;
            if (i == Scorm2004.timeLimitAction.exit_no_message) return Scorm1_2.timeLimitAction.exit_no_message;

            return Scorm1_2.timeLimitAction.not_set;
        }
    }
}