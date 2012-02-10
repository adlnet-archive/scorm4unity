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
using UnityEngine;
using System.Collections;
using ScormSerialization;
using Scorm2004;
public class ScormManager : MonoBehaviour
{
    static ScormSerialization.Unity_ScormBridge ScormBridge;
	static string ObjectName;
	static bool Initialized = false;
	static Scorm2004.DataModel StudentRecord;
	static int MainThreadID;
	static List<string> Log;
    
	
	///<summary>
	///Internal communication with Javascript components
	///</summary>
	///<remarks>
	///Serializes the entire SCORM datamodel object to the correct API calls, and sends to the 
	///Javascript API
	///</remarks> 
    private static void SubmitStudentData_imp()
	{
		try{
		if(!CheckThread())return;
		WaitForInitialize(); 
		if(ScormBridge.IsScorm2004)
		{
	        ScormSerializer serializer = new ScormSerializer(StudentRecord);
	        serializer.Serialize(ScormBridge);
			ScormBridge.Commit();
		}else
		{
			Scorm1_2.DataModel tempdata = ScormVersionConversion.DataModel.Translate(StudentRecord);
			ScormSerializer serializer = new ScormSerializer(tempdata);
	        serializer.Serialize(ScormBridge);
			ScormBridge.Commit();	
		}
		}catch(System.Exception e)
		{
				UnityEngine.Application.ExternalCall("DebugPrint", "***ERROR***" + e.Message +"<br/>" + e.StackTrace + "<br/>" + e.Source );
		}
		GameObject.Find(ObjectName).BroadcastMessage("Scorm_Commit_Complete");
    }
	///<summary>
	///Write all the modifications to the Student data to the LMS
	///</summary>
	///<remarks>
	///Serializes the entire SCORM datamodel object to the correct API calls, and sends to the 
	///Javascript API. Launchs a seperate thread for the work - will fire "Scorm_Commit_Complete"
	///when operation is complete.
	///</remarks> 
	public static void Commit()
	{
		System.Threading.ThreadStart start = new System.Threading.ThreadStart(SubmitStudentData_imp);
        System.Threading.Thread t = new System.Threading.Thread(start);
        t.Start();
	}
	///<summary>
	///Close the course. Be sure to call Commit first if you want to save your data.
	///</summary>
	///<remarks>
	///Will cause the browser to close the window, and signal to the LMS that the course is over.
	///Be sure to save your data to the LMS by calling Commit first, then waiting for Scorm_Commit_Complete.
	///</remarks> 
	public static void Terminate()
	{
		ScormBridge.Terminate();
	}
	///<summary>
	///Internal implementation of communication with the LMS
	///</summary>
	///<remarks>
	///Read all the data from the LMS into the internal data structure. Runs in a seperate thread.
	///Will fire "Scorm_Initialize_Complete" when the datamodel is ready to be manipulated
	///</remarks> 
	private static void Initialize_imp()
	{
		if(!CheckThread())return;
		ScormBridge = new Unity_ScormBridge(ObjectName,"ScormValueCallback");
		ScormBridge.Initialize();
		try{
			if(ScormBridge.IsScorm2004)
			{
				StudentRecord = new Scorm2004.DataModel();	
				ScormDeSerializer deserializer = new ScormDeSerializer(StudentRecord);
		        StudentRecord = (Scorm2004.DataModel)deserializer.Deserialize(ScormBridge);
			}else
			{
				Scorm1_2.DataModel tempStudentRecord = new Scorm1_2.DataModel();	
				ScormDeSerializer deserializer = new ScormDeSerializer(tempStudentRecord);
		        tempStudentRecord = (Scorm1_2.DataModel)deserializer.Deserialize(ScormBridge);
				StudentRecord = ScormVersionConversion.DataModel.Translate(tempStudentRecord);
			}
		}catch(Exception e)
		{
			UnityEngine.Application.ExternalCall("DebugPrint", "***ERROR***" + e.Message +"<br/>" + e.StackTrace + "<br/>" + e.Source );	
		}
		Initialized = true;
		GameObject.Find(ObjectName).BroadcastMessage("Scorm_Initialize_Complete",SendMessageOptions.DontRequireReceiver);
	}
	///<summary>
	///Read the student data in from the LMS
	///</summary>
	///<remarks>
	///Launches a seperate thread. Calls the javascript layer to read in all the data. 
	///Will fire "Scorm_Initialize_Complete" when the datamodel is ready to be manipulated
	///</remarks> 
	public static void Initialize()
	{
		System.Threading.ThreadStart start = new System.Threading.ThreadStart(Initialize_imp);
        System.Threading.Thread t = new System.Threading.Thread(start);
        t.Start();
	}
	///<summary>
	///Check that the running thread is not the Unity Thread
	///</summary>
	///<remarks>
	///Running some of the ScormManager function in the Unity thread will block the unity message queue
	///resulting in deadlock. This checks to make sure that will not occur.
	///</remarks> 
	public static bool CheckThread()
	{
		if(MainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
		{
			UnityEngine.Debug.LogError("This scorm manager command must not be called from the main thread.");
			return false;	
		}
		return true;
	}
	///<summary>
	///Begin the Scorm Manager. Wait for "Scorm_Initialize_Complete" after Start();
	///</summary>
	///<remarks>
	///Triggered by the Unity Engine, this function begins reading in all the data from the LMS. Launches
	///a thread, that will fire "Scorm_Initialize_Complete" when ready.
	///</remarks> 
    void Start()
    {
        ObjectName = this.gameObject.name;
		Log = new List<string>();
		MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
		Initialize();
    }
	///<summary>
	///Stop the Scorm Manager. Will commit data.
	///</summary>
	///<remarks>
	///Triggered by the Unity Engine, this will commit data back to the LMS. 
	///DO NOT RELY on this - the shutdown of Unity may break the message loop and cause timeouts in the 
	///serilization thread. Commit your data manually and wait for the "Scorm_Commit_Complete" message
	///before shutting down the engine.
	///</remarks> 
	void Stop()
	{
		Commit();
	}
	///<summary>
	///Get the entire student record
	///</summary>
	///<returns>
	///The cocdType object that represents the entire state data for the learner.
	///</returns>
	///<remarks>
	///Wait for the initialization to complete before retreiving this object.
	///You can do just about anything you like to the SCORM with this method - not recomended for SCORM
	///newbies.
	///</remarks> 
	static Scorm2004.DataModel GetLearnerRecord()
	{
		return StudentRecord;	
	}
	///<summary>
	///Used for the Javascript layer to communicate back to the Unity layer.
	///</summary>
	///<param name="value" direction="input">
	///A string in the format "value|number" where the number is the identifier of the Set of Get operation
	///issued by the bridge, and the value is the result of that API call
	///</param>
	///<remarks>
	///The javascript layer uses the name of the scormmanager object and the name of this function to return
	///the results of an operation on the javascript API to the ScormBridge. The number in the input string
	///is used by the bridge to figure out what API call this message answers. This complexity is due to the 
	///asyncronous nature of the UNITY/Javascript interface.
	///</remarks> 
    public void ScormValueCallback(string value)
    {
		try{
        ScormBridge.SetCallbackValue(value);
		}catch(Exception e)
		{
		UnityEngine.Application.ExternalCall("DebugPrint", e.Message);
		}
		
    }
	///<summary>
	///Wait until the ScormManager is ready.
	///</summary>
	///<remarks>
	///Should not be called by users. Will block the thread and sleep, so cannot be called from
	///the main Unity thread.
	///</remarks>
	private static void WaitForInitialize()
	{
		if(!CheckThread())return;
		
		while(Initialized == false)
		{
			System.Threading.Thread.Sleep(25);	
		}
	}
	///<summary>
	///Log a message.
	///</summary>
	///<param name="text" direction="input">
	///the string value to log.
	///</param>
	///<remarks>
	///Simply sends the log data down to child objects, which should handle the logging tasks.
	///</remarks> 
	public void LogMessage(object text)
	{
		GameObject.Find(ObjectName).BroadcastMessage("Log",text,UnityEngine.SendMessageOptions.DontRequireReceiver);
	}
	///<summary>
	///Get the name of the learner as supplied by the LMS
	///</summary>
	///<returns>
	///The name of the learner as supplied by the LMS.
	///</returns>
	///<remarks>
	///Wait for the initialization to complete before retreiving this object.
	///newbies.
	///</remarks> 
	public static string GetLearnerName()
	{
		return StudentRecord.learner_name.Value;	
	}
	///<summary>
	///Get the ID of the learner as supplied by the LMS
	///</summary>
	///<returns>
	///The ID of the learner as supplied by the LMS.
	///</returns>
	///<remarks>
	///Wait for the initialization to complete before retreiving this object.
	///newbies.
	///</remarks> 
	public static string GetLearnerID()
	{
		return StudentRecord.learner_id;
	}
	///<summary>
	///Get the attempt number of this attempt on this SCO.
	///</summary>
	///<returns>
	///the attempt number of this attempt on this SCO.
	///</returns>
	///<remarks>
	///Not currently implemented by the SCORM Standard. May be used in the future.
	///</remarks> 
	public static int GetAttemptNumber()
	{
		return StudentRecord.attemptNumber;	
	}
	///<summary>
	///Get the completion status value for this SCO as a string.
	///</summary>
	///<returns>
	///the completion status value for this SCO as a string.
	///</returns>
	///<remarks>
	///Wait for the initialization to complete before retreiving this object.
	///</remarks> 
	public static string GetCompletionStatusString()
	{
		return Enum.GetName(StudentRecord.completion_status.GetType(),StudentRecord.completion_status);	
	}
	///<summary>
	///Get the completion status value for this SCO 
	///</summary>
	///<returns>
	///the completion status value for this SCO.
	///</returns>
	///<remarks>
	///Wait for the initialization to complete before retreiving this object.
	///</remarks> 
	public static completionStatusType GetCompletionStatus()
	{
		return StudentRecord.completion_status;	
	}
	///<summary>
	///Set the completion status for this SCO
	///</summary>
	///<param name="completion" direction="input">
	///the new completion value
	///</param>
	///<remarks>
	///Wait for the initialization to complete before setting this object.
	///Used to signal that this course was completed or not completed
	///</remarks> 
	public static void SetCompletionStatus(completionStatusType completion)
	{
		GetLearnerRecord().completion_status = completion;	
	}
	///<summary>
	///Get the entry value for this SCO
	///</summary>
	///<returns>
	///The string value that specifies how the user has entered the course.
	///</returns>
	///<remarks>
	///Wait for the initialization to complete before setting this object.
	///Used to signal that this course was completed or not completed
	///</remarks> 
	public static string GetEntry()
	{
		return Enum.GetName(StudentRecord.entry.GetType(),StudentRecord.entry);	
	}
	///<summary>
	///Add a comment submitted by the learner.
	///</summary>
	///<param name="comment" direction="input">
	///the new comment object. Should contain the text of the comment. Timestamp will be filled automatically.
	///</param>
	///<remarks>
	///Resizes the comment array, adds the new comment. Sets the timestamp for the comment.
	///</remarks> 
	public static void AddCommentFromLearner(commentTypeComment comment)
	{
		
		commentType newcomment = new commentType();
		newcomment.comment = comment;
		newcomment.timestamp = new Scorm2004.DateTime(System.DateTime.Now);
		GetLearnerRecord().comments_from_learner.Add(newcomment);	
	}
	///<summary>
	///Add an interaction to the course data.
	///</summary>
	///<returns>
	///A new interaction object.
	///</returns>
	///<remarks>
	///The new interaction object created is added into the array, it is not necessary to add it 
	///again manually. The identifier is generated automatically, as is the timestamp. 
	///The new interaction is the LongFillIn type. You will have to get deeper into the SCORM for 
	///other types. Note that if you wish to set the LearnerResponse values, you need to cast 
	///interaction.learner_response to learnerResponseTypeLearnerResponseLongFillIn
	///</remarks> 
	public static interactionType AddInteraction()
	{
		
		interactionType i = new interactionType();
		i.id = "urn:ADL:interaction-id-" + GetLearnerRecord().interactions.Count.ToString();
		learnerResponseTypeLearnerResponseLongFillIn lrs = new learnerResponseTypeLearnerResponseLongFillIn();
		lrs.Value = "this is the data that should be in the answer";
		lrs.lang = "en-US";
		i.learner_response = lrs;
		i.objectives = new System.Collections.Generic.List<objectiveIDType>();
		i.result = interactionTypeResult.incorrect;
		
		i.timestamp = new Scorm2004.DateTime(System.DateTime.Now);
		i.type = interactionTypeType.long_fill_in;
		i.weighting = new System.Decimal(100);
		i.description.lang = "en-US";
		i.description.Value = "this is the description for the interaction";
		
		GetLearnerRecord().interactions.Add(i);	
		return i;
	}
	///<summary>
	///Get the array of interactions.
	///</summary>
	///<returns>
	///An array of interaction objects. DO NOT ADD OR DELETE FROM THIS ARRAY
	///</returns>
	///<remarks>
	///If you wish to add an interaction, use ScormManager.addInteraction(). If you need to remove an interaction
	///or otherwise manipulate this array, create a new array, copy the stuff you want, and set that array as 
	///in ScormManager.GetStudentRecord().interaction = new InteractionType[12];
	///</remarks> 
	public static System.Collections.Generic.List<interactionType> GetInteractions()
	{
		return GetLearnerRecord().interactions;	
	}
	///<summary>
	///Get the learner preference structure.
	///</summary>
	///<returns>
	///learner preference structure.
	///</returns>
	///<remarks>
	///You can use these for setting the volume or speed of presentation, and modifiy them for future use.
	///</remarks> 
	public static learnerPreferenceType GetLearnerPreferences()
	{
		return GetLearnerRecord().learner_preference;	
	}
	///<summary>
	///Set the learner preference structure.
	///</summary>
	///<param name="Preference">
	///The new learner preference structure.
	///</param>	
	///<remarks>
	///Probably better just to use the get function and modifiy the existing structure.
	///</remarks>
	public static void SetLearnerPreferences(learnerPreferenceType Preference)
	{
		GetLearnerRecord().learner_preference = Preference;	
	}
	///<summary>
	///Get the bookmark data
	///</summary>
	///<returns>
	///The bookmark data associated with the SCO.
	///</returns>
	///<remarks>
	///This is used to store data from one session on the SCO to another. It's up to you how you interperate
	///the data.
	///</remarks>
	public static string GetBookmark()
	{
		return GetLearnerRecord().location;	
	}
	///<summary>
	///Set the bookmark data.
	///</summary>
	///<param name="bookmark">
	///The bookmark string data.
	///</param>
	///<remarks>
	///This is used to store data from one session on the SCO to another. It's up to you how you interperate
	///the data. Note that setting a bookmark implies an exit code of suspend. You should probably exit after
	///setting this, or be sure to set the Exit state to something else if they continue on then complete.
	///</remarks>
	public static void SetBookmark(string bookmark)
	{
		GetLearnerRecord().exit = exit.suspend;
		GetLearnerRecord().location = bookmark;
	}
	///<summary>
	///Get the maximum time allowed for this SCO
	///</summary>
	///<returns>
	///The maximum time allowed for this SCO
	///</returns>
	///<remarks>
	///This is supplied by the LMS from the manifest in the course. Our packaging tool does not set this
	///but you can manually add it to the imsmanifest.xml in the course package if you wish.
	///</remarks>
	public static System.TimeSpan GetMaxTimeAllowed()
	{
		if(GetLearnerRecord().max_time_allowed != null)
			return GetLearnerRecord().max_time_allowed.timespan;	
		return new System.TimeSpan();
	}
	///<summary>
	///Set the session time.
	///</summary>
	///<param name="time">
	///the time that the user has been in this session.
	///</param>
	///<remarks>
	///You can decide on what counts as the timespan. It's possible that pausing does not count - it's up
	///to you.
	///</remarks>
	public static void SetSessionTime(System.TimeSpan time)
	{
		GetLearnerRecord().session_time = new Scorm2004.TimeSpan(time);	
	}
	///<summary>
	///Get the total time tracked by the LMS for this SCO
	///</summary>
	///<returns>
	///The total time tracked by the LMS for this SCO
	///</returns>
	///<remarks>
	///Read only, set by the LMS.
	///</remarks>
	public static System.TimeSpan GetTotalTime()
	{
		if(GetLearnerRecord().total_time != null)
			return GetLearnerRecord().total_time.timespan;
		return new System.TimeSpan();
	}
	///<summary>
	///Get the time limit action supplied by the LMS.
	///</summary>
	///<returns>
	///The time limit action supplied by the LMS.
	///</returns>
	///<remarks>
	///Read only, set by the LMS. This tells you what to do if the user has exceeded to allowed time.
	///You may have to deal with stopping the course depending on this value. It can be set in the 
	///imsmanifest for the course.
	///</remarks>
	public static timeLimitAction GetTimeLimitAction()
	{
		return GetLearnerRecord().time_limit_action;	
	}
	///<summary>
	///Get the progress measure for this SCO
	///</summary>
	///<returns>
	///The progress measure for this SCO
	///</returns>
	///<remarks>
	///This is set by you, and not manipulated by the LMS. The LMS may change it's behavior based on this 
	///value.
	///</remarks>
	public static System.Nullable<Decimal> GetProgressMeasure()
	{
		return GetLearnerRecord().progress_measure;	
	}
	///<summary>
	///Set the progress measure for this SCO
	///</summary>
	///<param name="Progress">
	///The new progress measure.
	///</param>
	///<remarks>
	///This is set by you, and not manipulated by the LMS. The LMS may change it's behavior based on this 
	///value.
	///</remarks>
	public static void SetProgressMeasure(Decimal Progress)
	{
		GetLearnerRecord().progress_measure = Progress;	
	}
	///<summary>
	///Add an objective to the course data.
	///</summary>
	///<returns>
	///A new objective object.
	///</returns>
	///<remarks>
	///The new objective object created is added into the array, it is not necessary to add it 
	///again manually. The identifier is generated automatically, as is the timestamp. 
	///The new objective defaults to not-attempted, and the sucess is unknown. 
	///</remarks> 
	public static objectiveType AddObjective()
	{
		objectiveType obj = new objectiveType();
				obj.id = "urn:ADL:objective-id-" + GetLearnerRecord().objectives.Count.ToString();
				obj.completion_status = completionStatusType.not_attempted;
				
				obj.description.Value = "This is objective description";
				obj.description.lang = "en-US";
				obj.progress_measure = (decimal)0;
				
				obj.score.min = (decimal)0;
				obj.score.max = (decimal)100;
				obj.score.raw  =(decimal)0;
				obj.score.scaled = (decimal)0;
				
				obj.success_status = successStatusType.unknown;
		
		
		GetLearnerRecord().objectives.Add(obj);	
		return obj;
	}
	///<summary>
	///Get the array of objectives.
	///</summary>
	///<returns>
	///An array of objectives objects. DO NOT ADD OR DELETE FROM THIS ARRAY
	///</returns>
	///<remarks>
	///If you wish to add an objectives, use ScormManager.addObjective(). If you need to remove an objective
	///or otherwise manipulate this array, create a new array, copy the stuff you want, and set that array as 
	///in ScormManager.GetStudentRecord().objectives = new objectiveType[12];
	///</remarks> 
	public static System.Collections.Generic.List<objectiveType> GetObjectives()
	{
		return GetLearnerRecord().objectives;	
	}
	///<summary>
	///Get the score data structure.
	///</summary>
	///<returns>
	///The score data structure.
	///</returns>
	///<remarks>
	///This is something you set, and is not read from the LMS. The LMS will not supply you with the 
	///previous scores. 
	///</remarks> 
	public static scoreType GetScore()
	{
		return GetLearnerRecord().score;	
	}
	///<summary>
	///Set the score for this SCO
	///</summary>
	///<param name="Raw">
	///A raw value for the score. This can be any value you wish.
	///</param>
	///<param name="Scaled">
	///The score value as a percentage between min and max. (0-1)
	///</param>
	///<param name="Min">
	///The minimum possible raw score
	///</param>
	///<param name="Max">
	///The maximum possible raw score
	///</param>
	///<remarks>
	///This is set by you, and not manipulated by the LMS. The LMS may change it's behavior based on this 
	///value.
	///</remarks>
	public static void SetScore(float Raw, float Scaled, float Min, float Max)
	{
		scoreType score = new scoreType();
		score.raw = (decimal)Raw;
		score.scaled = (decimal)Scaled;
		score.min = (decimal)Min;
		score.max = (decimal)Max;
		
		GetLearnerRecord().score = score;
	}
	///<summary>
	///A convienience method to set the scaled score
	///</summary>
	///<param name="score">
	///The score value as a percentage between min and max. (0-1)
	///</param>
	///<remarks>
	///This is set by you, and not manipulated by the LMS. The LMS may change it's behavior based on this 
	///value.
	///</remarks>
	public static void SetNormalizedScore(float score)
	{
		GetLearnerRecord().score.scaled = (decimal)score;	
	}
	///<summary>
	///Get success status for this SCO.
	///</summary>
	///<returns>
	///The success status for this SCO.
	///</returns>
	///<remarks>
	///You set this, and it is never read from the LMS.
	///</remarks> 
	public static successStatusType GetSatisfaction()
	{
		return GetLearnerRecord().success_status;	
	}
	///<summary>
	///Set the success value for this SCO
	///</summary>
	///<param name="success">
	///</param>
	///The success value.
	///<remarks>
	///This is set by you, and not manipulated by the LMS. The LMS may change it's behavior based on this 
	///value.
	///</remarks>
	public static void SetSatisfaction(successStatusType success)
	{
		GetLearnerRecord().success_status = success;	
	}
	///<summary>
	///Mark the data model as persistant for the next execution of the SCO.
	///</summary>
	///<remarks>
	///You'll need to call Commit to send this command to the LMS. Calling this then quitting without
	///calling Commit will never instruct the LMS with the persist command.
	///</remarks>
	public static void PersistDataForNextSession()
	{
		GetLearnerRecord().exit = exit.suspend;
	}
	///<summary>
	///Mark the exit type as normal, so the data will not persist into another execution
	///</summary>
	///<remarks>
	///You'll need to call Commit to send this command to the LMS. Calling this then quitting without
	///calling Commit will never instruct the LMS with the persist command.
	///</remarks>
	public static void ClearDataForNextSession()
	{
		GetLearnerRecord().exit = exit.normal;
	}
	///<summary>
	///Get the data that is associated with the SCO as startup data.
	///</summary>
	///<returns>
	///the data that is associated with the SCO as startup data.
	///</returns>
	///<remarks>
	///This can be used to create some initial state for the SCO. It is specified in the manifest and 
	///can be set up in the SCORMExport tool. You interperte this data as you see fit.
	///</remarks>
	public static string GetLaunchData()
	{
		return GetLearnerRecord().launch_data;	
	}
	///<summary>
	///Much like PersistDataForNextSession, but allows you to add additional data as suspend_data.
	///</summary>
	///<param name="data">
	///the additional data you would like to be stored as the suspend_data.
	///</param>
	///<remarks>
	///You'll need to call Commit to send this command to the LMS. Calling this then quitting without
	///calling Commit will never instruct the LMS with the persist command.  You interperte 
	// the additional data as you see fit.
	///</remarks>
	public static void SetSessionStateData(string data)
	{
		GetLearnerRecord().suspend_data = data;
		GetLearnerRecord().exit = exit.suspend;
	}
}