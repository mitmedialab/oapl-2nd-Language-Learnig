using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opal
{
	// log message event -- fire when you want to log something
	// so others who do logging can listen for the messages
	public delegate void LogEventHandler(object sender,LogEvent logme);

	// configuration
	public struct GameConfig
	{
		public string server;
		public string port;
		public bool sidekick;
		public bool logDebugToROS;
		public string opalCommandTopic;
		public string opalActionTopic;
		public string opalLogTopic;
		public string opalSceneTopic;
		public string opalAudioTopic;


	}
		
	public static class Constants
	{
		/** where images to load as sprites are located in Resources */
		public const string ANIMALS_IMAGE_PATH = "2D-Animals/Colored/";

		/// <summary>
		/// tags applied to game objects 
		/// </summary>
		public const string TAG_ANIMAL_OBJECT = "AnimalObject";
		public const string TAG_PLAY_OBJECT = "PlayObject";
		public const string TAG_PLAYER = "Player";

		/// <summary>
		/// sorting layer name 
		/// </summary>
		public const string LAYER_BACKGROUND ="Layer Background";
		public const string LAYER_WORDS = "Layer Words";
		public const string LAYER_OBSTACLE = "Layer Obstacle";

		/// <summary>
		/// animals names 
		/// </summary>
		public const string ANIMAL_TIGER= "Tiger-c";
		public const string ANIMAL_CHICKEN= "Chicken-c";
		public const string ANIMAL_LADYBIRD= "Ladybird-c";
		public const string ANIMAL_SHEEP= "Sheep-c";
		public const string ANIMAL_SNAKE= "Snake-c";
		public const string ANIMAL_RABBIT= "Rabbit-c";
		public const string ANIMAL_BEE= "Bee-c";
		public const string ANIMAL_DINOSAUR = "Dinosaur-c";


		///<summary>
		/// animation for main character
		/// </summary>
		public const string ANIM_WALK="02_walk";
		public const string ANIM_IDLE="01_idle";
		public const string ANIM_FIGHT="03_fight";
		public const string ANIM_HIT="04_hit";
		public const string ANIM_DIE="05_die";


		public const string SPEECHACE_URL="https://api.speechace.co/api/scoring/text/v0.1/json";
		public const string SPEECHACE_USERNAME = "002";
		public const string SPEECHACE_KEY="po%2Fc4gm%2Bp4KIrcoofC5QoiFHR2BTrgfUdkozmpzHFuP%2BEuoCI1sSoDFoYOCtaxj8N6Y%2BXxYpVqtvj1EeYqmXYSp%2BfgNfgoSr5urt6%2FPQzAQwieDDzlqZhWO2qFqYKslE";
	
		public const string SpeechToTextFolderName="SpeechToText";
		public const string TempFolderName="Temp";
		//public const string 

		///<summary>
		/// word visualization tags
		/// </summary>
		public const string WORDVIZ_LETTERHOLDER="WordLetterHolder";
		public const string WORDVIZ_LETTERTILE="WordLetterTile";


		/** config file path */
		// if playing in unity on desktop:
		public const string OPAL_CONFIG = "opal_config.txt";
		public const string CONFIG_PATH_OSX = @"/Resources/";
		// if playing on tablet:
		public const string CONFIG_PATH_ANDROID = "mnt/sdcard/edu.mit.media.prg.sar.opal.base/";
		// if a linux game:
		public const string CONFIG_PATH_LINUX = "/Resources/";

		///<summary>
		/// ROS-related constants
		/// </summary>
		/** Default ROS-related constants: topics and message types */
		// general string log messages (e.g., "started up", "error", whatever)
		public static string LOG_ROSTOPIC = "/opal_tablet";
		public const string DEFAULT_LOG_ROSTOPIC = "/opal_tablet";
		public const string LOG_ROSMSG_TYPE = "std_msgs/String";
		public const string SPEECH_RESULT_ROSTOPIC = "/opal_speech_result";
		public const string SPEECH_RESULT_ROSTOPIC2 = "/opal_speech_result2";
		public const string SPEECH_RESULT_ROSMSG_TYPE = "/sar_opal_msgs/OpalSpeechResult";
		// messages about actions taken on tablet (e.g., tap occurred on object x at xyz)
		// contains: 
		//  string object: name
		//  string action_type: tap
		//  float[] position: xyz
		public static string ACTION_ROSTOPIC = "/opal_tablet_action";
		public const string DEFAULT_ACTION_ROSTOPIC = "/opal_tablet_action";
		public const string ACTION_ROSMSG_TYPE = "/sar_opal_msgs/OpalAction";
		// messages logging the entire current scene
		// contains:
		//  string background
		//  objects[] { name posn tag }
		public static string SCENE_ROSTOPIC = "/opal_tablet_scene";
		public const string DEFAULT_SCENE_ROSTOPIC = "/opal_tablet_scene";
		public const string SCENE_ROSMSG_TYPE = "/sar_opal_msgs/OpalScene";
		// commands from elsewhere that we should deal with
		public static string CMD_ROSTOPIC = "/opal_tablet_command";
		public const string DEFAULT_CMD_ROSTOPIC = "/opal_tablet_command";
		public const string CMD_ROSMSG_TYPE = "/sar_opal_msgs/OpalCommand";   
		// messages to tell the game node when we're done playing audio
		// contains:
		//   bool done playing
		public static string AUDIO_ROSTOPIC = "/opal_tablet_audio";
		public const string DEFAULT_AUDIO_ROSTOPIC = "/opal_tablet_audio";
		public const string AUDIO_ROSMSG_TYPE = "/std_msgs/Bool";     
	}


}