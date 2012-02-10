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

namespace ScormSerialization
{

	///<summary>
	///Implement the ScormWrapper interface to bridge the Unity system with the Javascript layer
	///</summary>
	///<remarks>
	///This calls several blocking thread operations, and should be run in a thread that is not the
	///Unity messaging thread. It also needs the name of the ScormManager object and the ScormMAnager
	///function that handles messages, inorder to instruct the javascript layer how to communicate back.
	///</remarks>
	public class Unity_ScormBridge : ScormWrapper
	{
        int TimeToWaitForReply = 500;
        int TimePerPoll = 1;
        string CallbackObjectName;
        string CallbackFunctionName;
        Dictionary<int, APICallResult> CallbackValues;
        Random random;
        Queue<string> ResponseQueue;
		
		public bool IsScorm2004;
        public Unity_ScormBridge(string obj, string callback)
        {
            CallbackObjectName = obj;
            CallbackFunctionName = callback;
            CallbackValues = new Dictionary<int, APICallResult>();
            random = new Random();
            ResponseQueue = new Queue<string>();
			
			
        }
		public class APICallResult
		{
			public string Result;
			public string Key;
			public string ErrorCode;
			public string ErrorDescription;
		}
		///<summary>
		///Start up the bridge.
		///</summary>
		///<remarks>
		///This will check with the Javascript layer to see if it's executing in a 1.2 or 2004 LMS
		///</remarks>
		public void Initialize()
		{
			IsScorm2004 = true;
			
			int key = SetupCallback();
			TimeToWaitForReply = 10000;
            UnityEngine.Application.ExternalCall("doIsScorm2004",new object[] {CallbackObjectName, CallbackFunctionName, key });
			
            string result = WaitForReturn(key).Result;
			TimeToWaitForReply = 500;
			IsScorm2004 = System.Convert.ToBoolean(result);
			//UnityEngine.Application.ExternalCall("DebugPrint", result);
			if(IsScorm2004)
            	Log("ScormVersion is 2004");
			else
				Log("ScormVersion is 1.2");
			
		}
		///<summary>
		///Get a random key value not currently used by the list of waiting commands.
		///</summary>
		///<remarks>
		///the key is used to associate a javascript command with the message that responds to it.
		///</remarks>
		///<returns>
		///A key value that is not used in the map previously.
		///</returns>
        int GetRandomKey()
        {

                int key = random.Next(65536);
                while (CallbackValues.ContainsKey(key))
                    key = random.Next(65536);
                return key;
            
        }
		///<summary>
		///Create a callback with a unique key and add it to the map.
		///</summary>
		///<remarks>
		///the key is used to associate a javascript command with the message that responds to it.
		///</remarks>
		///<returns>
		///A key value that is not used in the map previously.
		///</returns>
        int SetupCallback()
        {
            lock (CallbackValues)
            {
                int key = GetRandomKey();
                CallbackValues[key] = null;
                return key;
            }
        }
		///<summary>
		///Wait until the ScormManager calls the object and inserts the results of a javascript message
		///</summary>
		///<remarks>
		///the key is used to associate a javascript command with the message that responds to it. This key is
		///sent into the javascript layer, and sent back to the scormmanager as part of the response. The 
		///Scorm manager inserts the message value into the queue. After this thread sleeps a bit, it checks
		///the queue to see if it got an answer to this request. If so, you get back the value of that message.
		///This all happens to allow the ScormSerializer to pretend that the set functions it calls on the Bridge
		///are synchronous.
		///</remarks>
		///<returns>
		///The return of the javascript commands associated with this key.
		///</returns>
        APICallResult WaitForReturn(int key)
        {
            int timeout = 0;
            bool wait = true;
            lock (CallbackValues)
            {
                wait = CallbackValues[key] == null && timeout < TimeToWaitForReply;
            }
            while (wait)
            {
                processQueue();
                lock (CallbackValues)
                {
                    wait = CallbackValues[key] == null && timeout < TimeToWaitForReply;
                }                
                if(wait)
                	System.Threading.Thread.Sleep(TimePerPoll);
                timeout += TimePerPoll;
                
            }
            if (timeout >= TimeToWaitForReply)
               Log("timeout");
            lock (CallbackValues)
            {
                APICallResult ret = CallbackValues[key];
				//if(CallbackValues[key].ErrorCode != "")
				//	Log(CallbackValues[key].ErrorDescription);
                CallbackValues.Remove(key);
                return ret;
            }
        }
		///<summary>
		///Check the queue of incomming message.
		///</summary>
		///<remarks>
		///When an incomming message is in the queue, add it to the response map using the key
		///this is part of the message string.
		///</remarks>
        void processQueue()
        {
            while (ResponseQueue.Count > 0)
            {
                try
                {
                    string input;
                        lock (ResponseQueue)
                    {
                        input = ResponseQueue.Dequeue();
                    }
                   
                    string[] tokens = input.Split('|');
                    int key = System.Convert.ToInt32(tokens[tokens.Length-1]);

                    lock (CallbackValues)
                    {
                        CallbackValues[key] = new Unity_ScormBridge.APICallResult();
						CallbackValues[key].Result = tokens[0];
						CallbackValues[key].ErrorCode = tokens[1];
						CallbackValues[key].ErrorDescription = tokens[2];
						CallbackValues[key].Key = tokens[tokens.Length-1];
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Application.ExternalCall("DebugPrint", "error " + e.Message);
                }
            }
        }
		///<summary>
		///Add a response to a javascript call to the queue.
		///</summary>
		///<remarks>
		///The SCORMManager will add messages from the javascript layer to the queue, which will be
		///processed by the other thread.
		///</remarks>
        public void SetCallbackValue(string input)
        {
            lock (ResponseQueue)
            {
                ResponseQueue.Enqueue(input);
            }
        }
		///<summary>
		///Get a value from the javascript API
		///</summary>
		///<remarks>
		///Implements the interface in ScormWrapper, and looks syncronous to the caller.
		///does the 2004/1.2 conversion.
		///</remarks>
		///<returns>
		///A scormget structure with the identifier and the return value of the get command
		///</returns>
		///<param name="identifier">
		///the dot notation identifier of the data model element to get
		///</param>
        public ScormGet Get(string identifier, System.Type EDT)
        {
			ScormGet get = new ScormGet(identifier, "",EDT);

			
            int key = SetupCallback();
			Log("Get " + get.GetIdentifier());
            UnityEngine.Application.ExternalCall("doGetValue", new object[] { get.GetIdentifier() , CallbackObjectName, CallbackFunctionName, key });
            
             APICallResult returnval = WaitForReturn(key);
            get.SetValue(returnval.Result);
			
			
			if(returnval.ErrorCode == "")
				Log("Got  " + get.GetValue());
			else
				Log("Error:" + returnval.ErrorCode.ToString() + " " + returnval.ErrorDescription);
				
			
            return get;
        }
		///<summary>
		///Set a value on the javascript API
		///</summary>
		///<remarks> 
		///Implements the interface in ScormWrapper, and looks syncronous to the caller.
		///</remarks>
		///<returns>
		///A SetResult enum with the return value of the set command
		///</returns>
		///<param name="identifier">
		///the dot notation identifier of the data model element to get
		///does the 2004/1.2 conversion.
		///</param>
        public SetResult Set(ScormSet set)
        {
			if(set.GetValue() != "not_set")
			{
	            int key = SetupCallback();
				Log( "Set  " + set.GetIdentifier() + " to " + set.GetValue());
	            UnityEngine.Application.ExternalCall("doSetValue", new object[] { set.GetIdentifier(), set.GetValue(), CallbackObjectName, CallbackFunctionName, key });
	            APICallResult returnval = WaitForReturn(key);
	            if(returnval.ErrorCode == "")
				{
					Log("Got " + returnval.Result);
					return SetResult.Ok;
				}
				else
				{
					Log("Error:" + returnval.ErrorCode.ToString() + " " + returnval.ErrorDescription);
					return SetResult.Error;
				}
			}
			return SetResult.Ok;
            
        }
		///<summary>
		///Send a log command up to the parent GameObject
		///</summary>
		///<param name="text">
		///the text to log
		///</param>
		public void Log(string text)
		{
			UnityEngine.GameObject.Find(CallbackObjectName).SendMessage("LogMessage",text,UnityEngine.SendMessageOptions.DontRequireReceiver);
		}
		///<summary>
		///Call the commit function in the javascript layer
		///</summary>
		public void Commit()
		{
			UnityEngine.Application.ExternalCall("doCommit");	
		}
		///<summary>
		///Call the terminate function in the javascript layer
		///</summary>
		public void Terminate()
		{
			UnityEngine.Application.ExternalCall("doTerminate");	
		}
	}
}
