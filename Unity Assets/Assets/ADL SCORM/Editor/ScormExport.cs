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

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Web;
using Ionic.Zip;
using System.IO;
 

//This class handles the editor window
public class ScormExport : EditorWindow {
	public GUISkin skin;
    
	static bool foldout1,foldout2,foldout3,foldout4;
	static ScormExport window; 
	static Vector2 scrollview;
    //Add this to the main editor gui
    [MenuItem("SCORM/Export SCORM Package",false,0)]
    static void ShowWindow()
    {
		EditorUtility.DisplayDialog("Export this scene as a WebPlayer first","Because this software is developed for Unity Basic, we cannot automatically build the web player. Please export your simulation to the web player first. Remember to select the SCORM Integration web player template.","OK");
        window = (ScormExport)EditorWindow.GetWindow (typeof (ScormExport));
		window.ShowAuxWindow();
		foldout1=foldout2=foldout3=foldout4 = true;
    }
	
	
	static void Init () {
	// Get existing open window or if none, make a new one:
	window = (ScormExport)EditorWindow.GetWindow (typeof (ScormExport));
	window.ShowAuxWindow();
		
	}
	
	[MenuItem("SCORM/Create SCORM Manager",false,0)]
    static void CreateManager()
    {
		GameObject manager = GameObject.Find("ScormManager");
		if(manager == null)
		{
        	manager = (GameObject)UnityEditor.SceneView.Instantiate(Resources.Load("ScormManager"));
			manager.name = "ScormManager";
			EditorUtility.DisplayDialog("The SCORM Manager has been added to the scene","Remember to place objects that need messages from the ScormManager under it in the scene heirarchy. It will send the message 'Scorm_Initialize_Complete' when it finishes communicating with the LMS.","OK");
		}else
		{
			EditorUtility.DisplayDialog("SCORM Manager is already present","You only need one SCORM Manager game object in your simulation. Remember to place objects that need messages from the ScormManager under it in the scene heirarchy.","OK");
		}
		
    }
	
	//Add this to the main editor gui
    [MenuItem("SCORM/About SCORM Integration",false,0)]
    static void About()
    {
		EditorUtility.DisplayDialog("Unity-SCORM Integration Toolkit Version 1.1 Beta","This software is a demonstation of integration between web deployed immersive 3D training and a Learning Managment System (LMS) using the Sharable Content Object Reference Model (SCORM) developed at the US Department of Defence Advance Distributed Learning (ADL) Inititive. This software is provided 'as-is' and is available free of charge at http://www.adlnet.gov. This software may be used under the provisions of the Apache 2.0 license. Source code is available from www.adlnet.gov.  ","OK");
    }
	//Add this to the main editor gui
    [MenuItem("SCORM/Help",false,0)]
    static void Help()
    {
		Application.OpenURL("http://www.adlnet.gov/scorm-unity-integration");
    }

	public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
	{
	    foreach (DirectoryInfo dir in source.GetDirectories())
	        CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
	    foreach (FileInfo file in source.GetFiles())
	        file.CopyTo(Path.Combine(target.FullName, file.Name));
	}
	string GetParameterString()
	{
		string parameters = "?";
		int num = 	PlayerPrefs.GetInt("Param_Count");
		
		for( int i = 0; i < num; i++)
		{
			if(PlayerPrefs.GetString("Param_"+i.ToString()+"_Name") != "")
			{
				parameters+= PlayerPrefs.GetString("Param_"+i.ToString()+"_Name") + "="+ PlayerPrefs.GetString("Param_"+i.ToString()+"_Value") + "&amp;";
			}
		}
		return parameters;
	}
    void Publish()
	{
		
		
		string webplayer = PlayerPrefs.GetString("Course_Export");
		string tempdir = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
		System.IO.Directory.CreateDirectory(tempdir);
		CopyFilesRecursively(new System.IO.DirectoryInfo(webplayer),new System.IO.DirectoryInfo(tempdir));
		string zipfile = EditorUtility.SaveFilePanel("Choose Output File",webplayer,PlayerPrefs.GetString("Course_Title"),"zip");
		if(zipfile!= "")
		{
			if(File.Exists(zipfile))
				File.Delete(zipfile);
			   
			Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipfile);
			zip.AddDirectory(tempdir); 
			
			if(PlayerPrefs.GetInt("SCORM_Version") < 3)
			{
				zip.AddItem(Application.dataPath + "/ADL SCORM/Plugins/2004");
				string manifest = 
								"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + 
								"<!-- exported by Unity-SCORM Integration Toolkit Version 1.1 Beta-->"+
								"<manifest xmlns=\"http://www.imsglobal.org/xsd/imscp_v1p1\" xmlns:imsmd=\"http://www.imsglobal.org/xsd/imsmd_v1p2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:adlcp=\"http://www.adlnet.org/xsd/adlcp_v1p3\" xmlns:imsss=\"http://www.imsglobal.org/xsd/imsss\" xmlns:adlseq=\"http://www.adlnet.org/xsd/adlseq_v1p3\" xmlns:adlnav=\"http://www.adlnet.org/xsd/adlnav_v1p3\" identifier=\"MANIFEST-AECEF15E-06B8-1FAB-5289-73A0B058E2DD\" xsi:schemaLocation=\"http://www.imsglobal.org/xsd/imscp_v1p1 imscp_v1p1.xsd http://www.imsglobal.org/xsd/imsmd_v1p2 imsmd_v1p2p2.xsd http://www.adlnet.org/xsd/adlcp_v1p3 adlcp_v1p3.xsd http://www.imsglobal.org/xsd/imsss imsss_v1p0.xsd http://www.adlnet.org/xsd/adlseq_v1p3 adlseq_v1p3.xsd http://www.adlnet.org/xsd/adlnav_v1p3 adlnav_v1p3.xsd\" version=\"1.3\">"+
								"  <metadata>"+
								"    <schema>ADL SCORM</schema>"
								+((PlayerPrefs.GetInt("SCORM_Version") == 0)?"    <schemaversion>2004 4th Edition</schemaversion>":"")
								+((PlayerPrefs.GetInt("SCORM_Version") == 1)?"    <schemaversion>2004 3rd Edition</schemaversion>":"")
								+((PlayerPrefs.GetInt("SCORM_Version") == 2)?"    <schemaversion>2004 CAM 1.3</schemaversion>":"")
								+
								"  </metadata>"+
								"  <organizations default=\"ORG-8770D9D9-AD66-06BB-9A3D-E87784C697FF\">"+
								"    <organization identifier=\"ORG-8770D9D9-AD66-06BB-9A3D-E87784C697FF\">"+
								"      <title>"+PlayerPrefs.GetString("Course_Title")+"</title>"+
								"      <item identifier=\"ITEM-79701DB3-F0AD-9426-8C43-819F3CB0EE6E\" identifierref=\"RES-4F17946A-9286-D5F0-7B6A-04E980C04F46\" parameters=\"" + GetParameterString() +"\">"+
								"			<title>"+PlayerPrefs.GetString("SCO_Title")+"</title>"+
								"			 <adlcp:dataFromLMS>"+ PlayerPrefs.GetString("Data_From_Lms") +"</adlcp:dataFromLMS>"+ 
								"			 <adlcp:completionThreshold completedByMeasure = \""+ (System.Convert.ToBoolean(PlayerPrefs.GetInt("completedByMeasure"))).ToString().ToLower() +"\" minProgressMeasure= \""+PlayerPrefs.GetFloat("minProgressMeasure") +"\" />";
				if(PlayerPrefs.GetInt("satisfiedByMeasure") == 1)
				{
								manifest += "				<imsss:sequencing>"+
								"			      <imsss:objectives>"+
								"			        <imsss:primaryObjective objectiveID = \"PRIMARYOBJ\""+
								"			               satisfiedByMeasure = \"" + (System.Convert.ToBoolean(PlayerPrefs.GetInt("satisfiedByMeasure"))).ToString().ToLower() + "\">"+
								"			          <imsss:minNormalizedMeasure>"+ PlayerPrefs.GetFloat("minNormalizedMeasure") +"</imsss:minNormalizedMeasure>"+
								"			        </imsss:primaryObjective>"+
								"			      </imsss:objectives>"+
								"			    </imsss:sequencing>";
				}
								manifest +=
								"      </item>"+
								"    </organization>"+
								"  </organizations>"+
								"  <resources>"+
								"    <resource identifier=\"RES-4F17946A-9286-D5F0-7B6A-04E980C04F46\" type=\"webcontent\" href=\"WebPlayer/WebPlayer.html\" adlcp:scormType=\"sco\">"+
								"      <file href=\"WebPlayer/WebPlayer.html\" />"+
								"      <file href=\"WebPlayer/scripts/scorm.js\" />"+
								"		<file href=\"WebPlayer/scripts/ScormSimulator.js\" />"+
								"		<file href=\"WebPlayer/UnityObject.js\" />"+
								"		<file href=\"WebPlayer/Webplayer.unity3d\" />"+
								"		<file href=\"WebPlayer/images/scorm_progres_bar.png\" />"+
								"		<file href=\"WebPlayer/images/thumbnail.png\" />"+
								"    </resource>"+
								"  </resources>"+
								"</manifest>";
				
				zip.AddEntry("imsmanifest.xml",".",System.Text.ASCIIEncoding.ASCII.GetBytes(manifest));
			}else
			{
				zip.AddItem(Application.dataPath + "/ADL SCORM/Plugins/1.2");
				string manifest = 
								"\"<?xml version=\"1.0\"?>"+
								"<!-- exported by Unity-SCORM Integration Toolkit Version 1.1 Beta-->"+
								"<manifest identifier=\"SingleCourseManifest\" version=\"1.1\""+
								"          xmlns=\"http://www.imsproject.org/xsd/imscp_rootv1p1p2\""+
								"          xmlns:adlcp=\"http://www.adlnet.org/xsd/adlcp_rootv1p2\""+
								"          xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""+
								"          xsi:schemaLocation=\"http://www.imsproject.org/xsd/imscp_rootv1p1p2 imscp_rootv1p1p2.xsd"+
								"                              http://www.imsglobal.org/xsd/imsmd_rootv1p2p1 imsmd_rootv1p2p1.xsd"+
								"                              http://www.adlnet.org/xsd/adlcp_rootv1p2 adlcp_rootv1p2.xsd\">"+
								"   <organizations default=\"B0\">"+
								"      <organization identifier=\"B0\">"+
								"         <title>"+PlayerPrefs.GetString("Course_Title")+"</title>"+
								"			   <item identifier=\"IS12\" identifierref=\"RS12\" parameters=\""+ GetParameterString() +"\">"+
								"				  <title>"+PlayerPrefs.GetString("SCO_Title")+"</title>"+
								"				  <adlcp:dataFromLMS>"+ PlayerPrefs.GetString("Data_From_Lms") +"</adlcp:dataFromLMS>"+
						 		"				  <adlcp:masteryscore>"+PlayerPrefs.GetFloat("masteryscore")+"</adlcp:masteryscore>"+
								"			   </item>"+
								"	      </organization>"+
								"	   </organizations>"+
								"	   <resources>"+
								"	      <resource identifier=\"RS12\" type=\"webcontent\""+
								"	                adlcp:scormtype=\"sco\" href=\"WebPlayer/WebPlayer.html\">"+
								"	         <metadata>"+
								"	            <schema>ADL SCORM</schema>"+
								"	            <schemaversion>1.2</schemaversion>"+
								"	            <adlcp:location>WebPlayer/WebPlayer.html</adlcp:location>"+
								"	         </metadata>"+
								"	      </resource>"+
								"	   </resources>"+
								"	</manifest>";
	
				
				zip.AddEntry("imsmanifest.xml",".",System.Text.ASCIIEncoding.ASCII.GetBytes(manifest));
				
			}
			zip.Save();
		}
	}
    //If there is no selection,show the search page and results, otherwise show the selection
    void OnGUI()
    {
		//window.position = new Rect(window.position.x,window.position.y,330,450);
		EditorStyles.miniLabel.wordWrap = true;
		EditorStyles.foldout.fontStyle = FontStyle.Bold;
		GUILayout.BeginHorizontal();                            
		foldout1 = EditorGUILayout.Foldout(foldout1,"Player Location", EditorStyles.foldout);
		bool help1 = GUILayout.Button(new GUIContent ("Help", "Help for the Player Location section"),EditorStyles.miniBoldLabel);
		if(help1)
			EditorUtility.DisplayDialog("Help","You must export this simulation as a webplayer, then tell this packaging tool the location of that exported webplayer folder. Be sure to select the SCORM webplayer template, or the necessary JavaScript components will not be included, and the system will fail to connect to the LMS.","OK");
		GUILayout.EndHorizontal();
		if(foldout1)
		{
			GUILayout.BeginVertical("TextArea");
			GUILayout.Label("Choose the location where the Webplayer was exported", EditorStyles.miniLabel);
			//GUILayout.BeginHorizontal();
			PlayerPrefs.SetString("Course_Export", EditorGUILayout.TextField("Player Folder", PlayerPrefs.GetString("Course_Export")));
			
			GUI.skin.button.fontSize = 8;
			GUILayout.BeginHorizontal();
			GUILayout.Space(window.position.width - 85);	
			bool ChooseDir = GUILayout.Button(new GUIContent("Choose Folder","Select the folder containing the webplayer"),GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			if(ChooseDir)
			{
				string export_dir = EditorUtility.OpenFolderPanel("Choose WebPlayer",PlayerPrefs.GetString("Course_Export"),"WebPlayer");
				if(export_dir != "")
				{
					
					if(export_dir.Substring(export_dir.LastIndexOf('/')+1).Equals("WebPlayer"))
					{
						export_dir = export_dir.Substring(0,export_dir.LastIndexOf('/'));	
					}
					if(Directory.Exists(export_dir + "/WebPlayer"))
						PlayerPrefs.SetString("Course_Export",export_dir);
					else
						EditorUtility.DisplayDialog("Invalid Directory","Please Choose a directory with a WebPlayer folder in it.","OK");
				}
			}
			//GUILayout.EndHorizontal();
	        //Create the label
	        GUILayout.EndVertical();
		}
		GUILayout.BeginHorizontal();                            
		foldout2 = EditorGUILayout.Foldout(foldout2,"Course Properties", EditorStyles.foldout);
		bool help2 = GUILayout.Button(new GUIContent ("Help", "Help for the Course Properties section"),EditorStyles.miniBoldLabel);
		if(help2)
			EditorUtility.DisplayDialog("Help","The properties will control how the LMS controls and displays your course content. These values will be written into the imsmanifest.xml file within the exported zip package. There are many other settings that can be specified in the manifest - for more information read the Content Aggregation Model documents at http://www.adlnet.gov/capabilities/scorm","OK");
		GUILayout.EndHorizontal();
		
		if(foldout2)
		{
			GUILayout.BeginVertical("TextArea");
			GUILayout.Label("Information about your SCORM package including the title and various configuration values.", EditorStyles.miniLabel);
	        PlayerPrefs.SetString("Course_Title", EditorGUILayout.TextField(new GUIContent("Course Title:","The title of the course, as listed in the learning management system (LMS)"), PlayerPrefs.GetString("Course_Title")));
	        PlayerPrefs.SetString("SCO_Title", EditorGUILayout.TextField(new GUIContent("Scene Title:","The title of the Unity content.  Note, the X title may show as the first item in an LMS-provided table of contents."), PlayerPrefs.GetString("SCO_Title")));
			PlayerPrefs.SetString("Launch_Data", EditorGUILayout.TextField(new GUIContent("Launch Data:","User-defined string value that can be used as initial learning experience state data."), PlayerPrefs.GetString("Launch_Data")));
			PlayerPrefs.SetString("Data_From_Lms", EditorGUILayout.TextField(new GUIContent("Data from LMS:","User-defined string value that can be used as initial learning experience state data."), PlayerPrefs.GetString("Data_From_Lms")));
			
			//2004
			if(PlayerPrefs.GetInt("SCORM_Version") < 3)
		 	{
				bool satisified = GUILayout.Toggle(System.Convert.ToBoolean(PlayerPrefs.GetInt("satisfiedByMeasure")),new GUIContent("Satisfied By Measure","If true, then this objective's satisfaction status will be determined by the score's relation to the passing score."));
				PlayerPrefs.SetInt("satisfiedByMeasure",System.Convert.ToInt16(satisified));
				if(satisified)
				{
					GUILayout.Label(new GUIContent("Passing Score: " + PlayerPrefs.GetFloat("minNormalizedMeasure").ToString(),"Defines a 'passing score' for this objective for use in conjunction with objective satisfied by measure."), EditorStyles.miniLabel);
					PlayerPrefs.SetFloat("minNormalizedMeasure",(float)System.Math.Round(GUILayout.HorizontalSlider(PlayerPrefs.GetFloat("minNormalizedMeasure"),-1.0f,1.0f)*100.0f)/100.0f);
				}
				bool progress = GUILayout.Toggle(System.Convert.ToBoolean(PlayerPrefs.GetInt("completedByMeasure")),new GUIContent("Completed By Measure","If true, then this activity's completion status will be determined by the progress measure's relation to the minimum progress measure. This derived completion status will override what it explicitly set."));
				PlayerPrefs.SetInt("completedByMeasure",System.Convert.ToInt16(progress));
				if(progress)
				{
					GUILayout.Label(new GUIContent("Minimum Progress Measure: " + PlayerPrefs.GetFloat("minProgressMeasure").ToString(),"Defines a minimum completion percentage for this activity for use in conjunction with completed by measure.") , EditorStyles.miniLabel);
					PlayerPrefs.SetFloat("minProgressMeasure",(float)System.Math.Round(GUILayout.HorizontalSlider(PlayerPrefs.GetFloat("minProgressMeasure"),0.0f,1.0f)*100.0f)/100.0f);
				}
			}
			//1.2
			else
			{
				GUILayout.Label(new GUIContent("Mastery Score: " + PlayerPrefs.GetFloat("masteryscore").ToString(),"The score required of a learner to achieve \"mastery\" or pass a given SCO"), EditorStyles.miniLabel);
				PlayerPrefs.SetFloat("masteryscore",(float)System.Math.Round(GUILayout.HorizontalSlider(PlayerPrefs.GetFloat("masteryscore"),0.0f,100.0f)));
				
			}
			
			
			foldout4 = EditorGUILayout.Foldout(foldout4,new GUIContent("Launch Parameters","Querystring parameters appended to the player URL during launch"), EditorStyles.foldout);
			GUI.skin.textArea.alignment = TextAnchor.MiddleLeft;
			if(foldout4)
			{
				int num = 	PlayerPrefs.GetInt("Param_Count");
				GUILayout.BeginVertical("TextArea");
				
				int notblank = 0;
				for( int i = 0; i < num; i++)
				{
					if(PlayerPrefs.GetString("Param_"+i.ToString()+"_Name") != "")
						notblank++;
				}
				if(notblank > 2)
				scrollview = GUILayout.BeginScrollView(scrollview,GUILayout.MinHeight(Mathf.Clamp(notblank*20,0,80)));
				int total = 0;
				for( int i = 0; i < num; i++)
				{
					if(PlayerPrefs.GetString("Param_"+i.ToString()+"_Name") != "")
					{
						GUILayout.BeginHorizontal();
						bool delete = GUILayout.Button(new GUIContent("x","Remove this entry"),GUILayout.ExpandWidth(false));
						string name = GUILayout.TextArea(PlayerPrefs.GetString("Param_"+i.ToString()+"_Name"),GUILayout.MaxWidth((window.position.width - 10)/2));
						string val = GUILayout.TextArea(PlayerPrefs.GetString("Param_"+i.ToString()+"_Value"));
						GUILayout.EndHorizontal();
						if(delete)
						{
							name = "";
							val = "";
						}
						PlayerPrefs.SetString("Param_"+i.ToString()+"_Name",name);
						PlayerPrefs.SetString("Param_"+i.ToString()+"_Value",val);
					}
					
				}
				if(notblank > 2)
					GUILayout.EndScrollView();
				GUILayout.BeginHorizontal();
				bool add = GUILayout.Button(new GUIContent("Add Entry","Add a new entry"),GUILayout.ExpandWidth(false));
				if(add)
				{		
						
					    PlayerPrefs.SetString("Param_"+num.ToString()+"_Name","name");
						PlayerPrefs.SetString("Param_"+num.ToString()+"_Value","val");
						num++;
						PlayerPrefs.SetInt("Param_Count",num);
				}
				
				bool clear = GUILayout.Button(new GUIContent("Clear Entrys","Remove All Entries"),GUILayout.ExpandWidth(false));
				if(clear)
				{
						PlayerPrefs.SetInt("Param_Count",0);
						
						
				}
				GUILayout.EndHorizontal();
				
				GUILayout.EndVertical();
			}
			GUILayout.EndVertical();
		}
		GUILayout.BeginHorizontal();                            
		foldout3 = EditorGUILayout.Foldout(foldout3,"SCORM Version", EditorStyles.foldout);
		bool help3 = GUILayout.Button(new GUIContent ("Help", "Help for the SCORM Version section"),EditorStyles.miniBoldLabel);
		if(help3)
			EditorUtility.DisplayDialog("Help","Select the version that your LMS supports. Some LMSs support multiple SCORM versions. Choose the highest level supported. The calls to the SCORM Manager within your game code will be translated into the correct SCORM version. Please note that some data cannot be mapped between the SCORM 2004 and SCORM 1.2 datamodels, and will be lost when running the course in a 1.2 LMS. Contact ADL for information on this topic.","OK");
		GUILayout.EndHorizontal();
		
		if(foldout3)
		{
			GUILayout.BeginVertical("TextArea");
			//GUILayout.EndHorizontal();
			GUILayout.Label("Select the SCORM version used by the target LMS", EditorStyles.miniLabel);
			//PlayerPrefs.SetInt("SCORM_Version",GUILayout.SelectionGrid(PlayerPrefs.GetInt("SCORM_Version"),new string[]{"SCORM 2004 v. 4","SCORM 1.2"},2));
	        
				
			PlayerPrefs.SetInt("SCORM_Version",EditorGUILayout.Popup(PlayerPrefs.GetInt("SCORM_Version"),new string[]{"SCORM 2004 v. 4","SCORM 2004 v. 3","SCORM 2004 v. 2","SCORM 1.2"},GUILayout.ExpandWidth(false)));
			
			
			
			
			GUILayout.EndVertical();
		}
		GUIStyle s =  new GUIStyle();
		GUI.skin.button.fontSize = 12;
		bool publish = GUILayout.Button(new GUIContent ("Publish", "Export this course to a SCORM package."));
		if(publish)
			Publish();

    }
}
