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


/*******************************************************************************
** Usage: Creates javacript object to simulate SCORM LMS. Uncomment lines at bottom for proper LMS version.
**      functions as follows:
**      Will not actually save data anywhere or do error checking - just use this when testing locally to ensure that the 
**		system will sucessfully initialize
**
*******************************************************************************/
function ScormSimulator()
{
	
	
	this.Initialize = function(s)
	{
			this.errorcode = 0;
			this.data = {};
	        this.data["cmi.comments_from_lms._count"] = "1";
	        this.data["cmi.comments_from_learner._count"] = "0";
            this.data["cmi.comments_from_lms.0.comment"] = "{lang=en-US}test comment from LMS";
            
            this.data["cmi.comments_from_lms.0.id"] = "test identifier string";
            this.data["cmi.comments_from_lms.0.location"] = "test location string";
            this.data["cmi.comments_from_lms.0.timestamp"] = "";
            
             
            this.data["cmi.attemptNumber"] = "1";
            
            this.data["cmi.completion_status"] = "completed";
            
            this.data["cmi.completion_threshold"] = "90.0";
            

            this.data["cmi.credit"] = "credit";
            

            this.data["cmi.dataModelVersion"] = "1.0";
            this.data["cmi.entry"] = "ab-initio";
            
            
            this.data["cmi.exit"] = "logout";
            
            
            this.data["cmi.interactions._count"] = "1";
            this.data["cmi.interactions.0.attemptNumber"] = "4";
           
            this.data["cmi.interactions.0.description"] = "test description";
            
            this.data["cmi.interactions.0.id"] = "test identifier";
            this.data["cmi.interactions.0.latency"] = "0";
            
            this.data["cmi.interactions.0.objectiveIds._count"] = "2";
            this.data["cmi.interactions.0.objectiveIds.0"] = "Objective1";
            this.data["cmi.interactions.0.objectiveIds.1"] = "Objective2";
            this.data["cmi.interactions.0.result"] = "incorrect";
            this.data["cmi.interactions.0.resultNumeric"] = "0.0";
            
            
            this.data["cmi.interactions.0.timeStamp"] = "";
            
            this.data["cmi.interactions.0.type"] = "matching";
            this.data["cmi.interactions.0.weighting"] = "1.0";
            
            
            this.data["cmi.launch_data"] = "launch data would be hwere";
            
            this.data["cmi.learner_id"] = "learner id would be here";
            
            this.data["cmi.learner_name"] = "Rob Chadwick";
            

            
            this.data["cmi.learner_preference.audio_captioning"] = "off";
            
            this.data["cmi.learner_preference.audio_level"] = "80.0";
            
            this.data["cmi.learner_preference.delivery_speed"] = "1.0";
            
            this.data["cmi.learner_preference.language"] = "en-US";
                       
            this.data["cmi.mode"] = "browse";
            
            
            this.data["cmi.objectives.0.completionStatus"] = "completed";
            
            this.data["cmi.objectives.0.description"] = "{lang=en-US}this is a description";
            
            this.data["cmi.objectives._count"] = "1";
            this.data["cmi.objectives.0.id"] = "Objective1";
            this.data["cmi.objectives.0.progressMeasure"] = "90.0";
            

           
            this.data["cmi.objectives.0.score.max"] = "99.0";
            
            this.data["cmi.objectives.0.score.min"] = "1.0";
            
            this.data["cmi.objectives.0.score.raw"] = "45.0";
            
            this.data["cmi.objectives.0.score.scaled"] = "0.50";
            

            this.data["cmi.objectives.0.successStatus"] = "failed";
            
            
            this.data["cmi.progress_measure"] = "90.0";
            
            
            this.data["cmi.scaled_passing_score"] = "80.0";
            

            
            this.data["cmi.score.max"] = "100.0";
            
            this.data["cmi.score.min"] = "0.0";
            
            this.data["cmi.score.raw"] = "75.5";
            
            this.data["cmi.score.scaled"] = "0.90";
            
            
            this.data["cmi.core.student_id"] = "11011883";
            this.data["cmi.success_status"] = "failed";
            
            
            this.data["cmi.suspend_data"] = "suspend data";
            
            this.data["cmi.time_limit_action"] = "continue_no_message";
            
            this.data["cmi.total_time"] = "10h10m";
	
			this.data["cmi.interactions.0.correctResponses.Items._count"] = "0";
			this.data["cmi.interactions.0.correctResponses.ItemsElementName._count"] = "0";
			this.data["cmi.interactions.0.learnerResponse.Items._count"] = "0";
			
	
			this.data["cmi.interactions.0.learnerResponse.ItemsElementName._count"] = "0";
	
		return "true";
	}
	this.LMSInitialize = this.Initialize;
	
	this.GetValue = function(identifier)
	{
		this.errorcode = 0;
		var result = this.data[identifier];
		if(result)
			return result
		
		this.errorcode = 401;
		return "Error";
	}
	this.LMSGetValue = this.GetValue;
	
	this.SetValue = function(identifier,value)
	{
		this.data[identifier] = value;
		return "true";
	}
	this.LMSSetValue = this.SetValue;
	
	this.Terminate = function(s)
	{
	    return "true";
	}
	this.LMSFinish = this.Terminate;
	
	this.GetDiagnostic = function(errorCode)
	{
		return "Not Implemented";
	}
	this.LMSGetDiagnostic = this.GetDiagnostic;
	
	this.GetErrorString = function(errorCode)
	{
		return "Not Implemented";
	}
	this.LMSGetErrorString = this.GetErrorString;
	
	this.GetLastError = function()
	{
		return this.errorcode;
	}
	this.LMSGetLastError = this.GetLastError;
	
	this.Commit = function()
	{
		return "true";
	}
	this.LMSCommit = this.Commit;
}
 	
	//Uncomment this line to simulate a SCORM 2004 LMS
    //window.API_1484_11= new ScormSimulator();
	
	//Uncomment this line to simulate a SCORM 1.2 LMS
    //window.API= new ScormSimulator();
      
