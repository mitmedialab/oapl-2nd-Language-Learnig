using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using MiniJSON;

namespace opal
{

	public static class Utilities
	{
		/// <summary>
		/// Decodes the JSON config file
		/// </summary>
		/// <param name="path">Path to config file</param>
		/// <param name="server">Server IP address for websocket connection</param>
		/// <param name="port">Port for websocket connection </param>
		/// <param name="toucan">Whether the Toucan should be present or not</param> 
		public static bool ParseConfig (string path, 
			out GameConfig gameConfig)
		{
			gameConfig.server = "";
			gameConfig.port = "";
			gameConfig.sidekick = false;
			gameConfig.logDebugToROS = false;
			gameConfig.opalActionTopic = "";
			gameConfig.opalAudioTopic = "";
			gameConfig.opalCommandTopic = "";
			gameConfig.opalLogTopic = "";
			gameConfig.opalSceneTopic = "";

			//if (!File.Exists(path))
			//{
			//    Logger.LogError("ERROR: can't find websocket config file at " +
			//              path);
			//    return;
			//}
			string config = "";
			try {
				config = File.ReadAllText(path);
				Logger.Log("got config: " + config);
				config.Replace("\n", "");

				Dictionary<string, object> data = null;
				data = Json.Deserialize(config) as Dictionary<string, object>;
				if(data == null) {   
					Logger.LogError("Could not parse JSON config file!");
					return false;
				}
				Logger.Log("deserialized " + data.Count + " objects from JSON!");

				// if the config file doesn't have all parts, consider it invalid
				if(!(data.ContainsKey("server")
					&& data.ContainsKey("port")
					&& data.ContainsKey("toucan")
					&& data.ContainsKey("log_debug_to_ros")
					&& data.ContainsKey("opal_action_topic")
					&& data.ContainsKey("opal_scene_topic")
					&& data.ContainsKey("opal_command_topic")
					&& data.ContainsKey("opal_audio_topic")
					&& data.ContainsKey("opal_log_topic")))
				{
					Logger.LogError("Did not get a valid config file!");
					return false;
				}

				// get configuration options
				gameConfig.server = (string)data["server"];
				gameConfig.port = (string)data["port"];
				gameConfig.sidekick = (bool)data["toucan"];
				gameConfig.logDebugToROS = (bool)data["log_debug_to_ros"];
				gameConfig.opalActionTopic = (string)data["opal_action_topic"];
				gameConfig.opalAudioTopic = (string)data["opal_audio_topic"];
				gameConfig.opalCommandTopic = (string)data["opal_command_topic"];
				gameConfig.opalSceneTopic = (string)data["opal_scene_topic"];
				gameConfig.opalLogTopic = (string)data["opal_log_topic"];
				gameConfig.opalLogTopic = (string)data["opal_speech_result_topic"];

				Logger.Log("server: " + gameConfig.server + "\nport: " + gameConfig.port 
					+ "\nsidekick: " + gameConfig.sidekick + "\nlog_debug_to_ros: " 
					+ gameConfig.logDebugToROS + "\nopal_action_topic: "
					+ gameConfig.opalActionTopic + "\nopal_audio_topic: "
					+ gameConfig.opalAudioTopic + "\nopal_command_topic: "
					+ gameConfig.opalCommandTopic + "\nopal_log_topic: "
					+ gameConfig.opalLogTopic + "\nopal_scene_topic: "
					+ gameConfig.opalSceneTopic);
				return true;

			} catch(Exception e) {
				Logger.LogError("Could not read config file! File path given was " 
					+ path + "\nError: " + e);
				return false;
			}

		}
	}

	/// <summary>
	/// check whether a tag exists. If not, add the tag
	/// </summary>
	public static class TagHelper
	{
		public static void AddTag(string tag)
		{
			UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
			if ((asset != null) && (asset.Length > 0))
			{
				SerializedObject so = new SerializedObject(asset[0]);
				SerializedProperty tags = so.FindProperty("tags");

				for (int i = 0; i < tags.arraySize; ++i)
				{
					if (tags.GetArrayElementAtIndex(i).stringValue == tag)
					{
						return;     // Tag already present, nothing to do.
					}
				}

				tags.InsertArrayElementAtIndex(0);
				tags.GetArrayElementAtIndex(0).stringValue = tag;
				so.ApplyModifiedProperties();
				so.Update();
			}
		}
	}

	/// <summary>
	/// assigning sorting layers according to game object's tag
	/// </summary>
//	public static class SortingLayerHelper
//	{
//		public static void AssignSortingLayers()
//		{
//			//assig background sorting layer to objects with tag "BackGround"
//
//			AssignLayerToObjects (GameObject.FindGameObjectsWithTag ("BackGround"), "Layer_Background");
//
////			GameObject[] objList = GameObject.FindGameObjectsWithTag ("BackGround");
////			AssignLayerToObjects (objList, "Layer_Background");
//		}
//
//		void AssignLayerToObjects(GameObject[] objList,string sortingLayerID){
//			foreach (GameObject obj in objList) {
//				SpriteRenderer iRenderer = obj.GetComponent<SpriteRenderer>;
//				if (iRenderer != null) {
//					iRenderer.sortingLayerID = sortingLayerID;
//				}
//			}
//		}
//	}

	public static class ObjectHandler
	{
		/// <summary>
		/// Instantiate a new game object with the specified properties
		/// </summary>
		/// <param name="pops">properties of the play object.</param>
		public static void InstantiateSceneObject (SceneObjectProperties pops, Sprite spri,GameObject go)
		{
			//GameObject go = new GameObject();

			// set object name
			go.name = pops.Name();

			Logger.Log("Creating new play object: " + pops.Name() + ", with properties: "
				+ pops.ToString());

			// load sprite/image for object
			SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
			// if we were not given a sprite for this object, try loading one
			if (spri == null)
			{
				// don't need file extension to load from resources folder -- strip if it exists
				Sprite sprite = Resources.Load<Sprite>(Constants.ANIMALS_IMAGE_PATH+pops.Name());

				if(sprite == null)
				{
					Logger.LogWarning("Could not load sprite from Resources: " 
						+ Constants.ANIMALS_IMAGE_PATH+pops.Name()
						+ "\nGoing to try file path...");

					// TODO add filepath to pops! don't use Name
					//sprite = Utilities.LoadSpriteFromFile(pops.Name());
				}

				// got sprite!
				spriteRenderer.sprite = sprite; 
			}
			// otherwise, we were given a sprite, try using that one
			else
			{
				spriteRenderer.sprite = spri;
			}

			// move object to specified initial position 
			go.transform.position = pops.InitPosition();

			go.transform.localScale = pops.Scale ();
			// set the scale of the sprite to the specified scale
			//go.transform.localScale = pops.Scale();

			// set tag
			go.tag = pops.Tag();


			// if tag is ANIMAL, keep reference and set as invisible
			if (go.tag.Equals(Constants.TAG_ANIMAL_OBJECT)
				&& go.GetComponent<Renderer>() != null)
			{
				go.GetComponent<Renderer>().enabled = true;
			}

			if (pops.RigidBody2D())
			{
				// add rigidbody if this is a rigidbody object
				Rigidbody2D rb2d = go.AddComponent<Rigidbody2D>();
				// remove object from physics engine's control, because we don't want
				// the object to move with gravity, forces, etc - we do the moving
				rb2d.isKinematic = false; 

				// true: don't want gravity, otherwise objects will fall
				// though with the isKinematic flag set this may not matter
				//rb2d.gravityScale = 0; 
				// set collision detection to 'continuous' instead of discrete
				//rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

				// add collision manager so we get trigger enter/exit events
				CollisionManager cm = go.AddComponent<CollisionManager>();
				// subscribe to log events from the collision manager
				//cm.logEvent += new LogEventHandler(HandleLogEvent);

			}

			// add polygon collider that matches shape of object and set as a 
			// trigger so enter/exit events fire when this collider is hit
			PolygonCollider2D pc = go.AddComponent<PolygonCollider2D>();
			pc.isTrigger = false;

			// add and subscribe to gestures
			//			if(this.gestureManager == null) {
			//				Logger.Log("ERROR no gesture manager");
			//				FindGestureManager();
			//			}
			//
			//			try {
			//				// add gestures and register to get event notifications
			//				this.gestureManager.AddAndSubscribeToGestures(go, pops.draggable, false);
			//			}
			//			catch (Exception e)
			//			{
			//				Logger.LogError("Tried to subscribe to gestures but failed! " + e);
			//			}

			// add pulsing behavior (draws attention to actionable objects)
			// go.AddComponent<GrowShrinkBehavior>();
			// Removing this because it messes with collision detection when
			// objects are close to each other (continuously colliding/uncolliding)
			// go.GetComponent<GrowShrinkBehavior>().StartPulsing();
		}
	}

	public class SpeechHandler
	{
		AudioClip myAudioClip;

		public string speechFileName="speechData2";
		public string WorkingDirectory="/Users/huilichen/Downloads/speechace/";
		public string speechText="apple";


		public void Post2SpeechAce()
		{
			// run sh command to send http request to Speechace API

			ProcessStartInfo proc = new ProcessStartInfo();
			proc.FileName = "/bin/bash";
			//proc.FileName = "/Library/Frameworks/Python.framework/Versions/2.7/Resources/Python.app/Contents/MacOS/Python";
			string WorkingDirectory="/Users/huilichen/speechace/";
			proc.WorkingDirectory = WorkingDirectory;
			proc.Arguments = "speechace_test.sh";
			//proc.Arguments = "speechace_evaluation.py";
			proc.WindowStyle = ProcessWindowStyle.Minimized;
			proc.CreateNoWindow = true;
			proc.RedirectStandardOutput = true;
			proc.UseShellExecute = false;
			Process process = new Process();
			process.StartInfo = proc;
			process.Start();

			string q = "";
			while ( ! process.HasExited ) {
				q += process.StandardOutput.ReadToEnd();
				Logger.Log (q);

			}


			Logger.Log (q);
			Logger.Log (" !!!!!!processor stops....!!!!!");
		}
	}





}

