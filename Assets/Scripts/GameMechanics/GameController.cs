using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using System.Diagnostics; 
using UnityEngine.UI;
using System.Threading;
using System;

namespace opal{


	public class GameController : MonoBehaviour {

		public GameObject animal;
		public Camera cam;
		public AnimalHandler animalHandler;
		public SpeechHandler speechHandler;

		public GameObject coins; // bonuses 

		//game score
		public Text GameScoreText;
		public int gameScore;
		private float maxWidth;

		public string speechText;



		//word visualization
		[SerializeField]
		WordVizManager wordViz;

		public WordVizManager WordVizManager
		{
			set
			{
				wordViz = value;
			}
		}

		//static instance to be accessed by other scripts/objects
		public static GameController instance = null;  

		// time elapsed since last time of getting coins
		private float timeSinceLastCoins;
		private float spawnRate = 5f;
		private bool animalPresent = false;


		// ROS
		// rosbridge websocket client
		private RosbridgeWebSocketClient clientSocket = null;
		// config
		private GameConfig gameConfig;

		// actions for main thread, because the network messages from the
		// websocket can come in on another thread
		readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

		private bool test=false;

		//Awake is always called before any Start functions
		void Awake()
		{
			string path = "";

			//Check if instance already exists
			if (instance == null)

				//if not, set instance to this
				instance = this;

			//If instance already exists and it's not this:
			else if (instance != this)

				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
				Destroy (gameObject);   

			Logger.Log ("check game controller instance: "+instance);
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);


			// find the config file
			#if UNITY_ANDROID
			path = Constants.CONFIG_PATH_ANDROID + Constants.OPAL_CONFIG;
			Logger.Log("trying android path: " + path);
			#endif

			#if UNITY_EDITOR
			path = Application.dataPath + Constants.CONFIG_PATH_OSX + Constants.OPAL_CONFIG;
			Logger.Log("trying os x path: " + path);
			#endif

			#if UNITY_STANDALONE_LINUX
			path = Application.dataPath + Constants.CONFIG_PATH_LINUX + Constants.OPAL_CONFIG;
			Logger.Log("trying linux path: " + path);
			#endif

			// read config file
			if(!Utilities.ParseConfig(path, out gameConfig)) {
				Logger.LogWarning("Could not read config file! Will try default "
					+ "values of toucan=true, server IP=192.168.1.103, port=9090, "
					+ "opal_action_topic=" + Constants.ACTION_ROSTOPIC
					+ ", opal_audio_topic=" + Constants.AUDIO_ROSTOPIC
					+ ", opal_command_topic=" + Constants.CMD_ROSTOPIC
					+ ", opal_log_topic=" + Constants.LOG_ROSTOPIC
					+ ", opal_scene_topic=" + Constants.SCENE_ROSTOPIC
					+ ".");
			}
			else {
				Logger.Log("Got game config!");
			}
		}

		// Use this for initialization
		void Start () {
			if (cam == null) {
				cam = Camera.main;
			}

			speechHandler = new SpeechHandler ();
			animalHandler = new AnimalHandler ();

			//word visualization
			//wordViz=GameObject.Find("WordVisualization");

			//intialize game score
			gameScore = 0;
			SetGameScoreText();


			speechText="chicken";
			TagHelper.AddTag(Constants.TAG_ANIMAL_OBJECT);



		}
			
		void setupROS(){
			// set up rosbridge websocket client
			// note: does not attempt to reconnect if connection fails!
			// demo mode does not use ROS!
			if(this.clientSocket == null)
			{	
				Logger.Log("!!!!!!!!!!!!!!!!!!! socket setup!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				// load file

				if (this.gameConfig.server.Equals("") || this.gameConfig.port.Equals("")) {
					Logger.LogWarning("Do not have opal configuration... trying "
						+ "hardcoded IP 192.168.1.103 and port 9090");
					this.clientSocket = new RosbridgeWebSocketClient(
						"192.168.1.103",// server, // can pass hostname or IP address
						"9090"); //port);   
				} else {
					this.clientSocket = new RosbridgeWebSocketClient(
						this.gameConfig.server, // can pass hostname or IP address
						this.gameConfig.port);  
					Logger.Log("!!!!!!!!!!!!!!!!!!! ros bridge web ..!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				}

				if (this.clientSocket.SetupSocket())
				{
					Logger.Log("!!!!!!!!!!!!!!!!!!! socket setup successful!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
//					this.clientSocket.receivedMsgEvent += 
//						new ReceivedMessageEventHandler(HandleClientSocketReceivedMsgEvent);

					Logger.Log("!!!!!!!!!!!!!!!!!!! after received meessage event handler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
						Constants.SPEECH_RESULT_ROSTOPIC2, Constants.SPEECH_RESULT_ROSMSG_TYPE));

					// advertise that we will publish opal action messages                
					if (this.gameConfig.opalActionTopic == "")
					{
						Logger.LogWarning("Do not have opal configuration... trying "
							+ "default topic " + Constants.DEFAULT_ACTION_ROSTOPIC);
						Constants.ACTION_ROSTOPIC = Constants.DEFAULT_ACTION_ROSTOPIC;
					}
					else 
					{   
						Constants.ACTION_ROSTOPIC = this.gameConfig.opalActionTopic;
					}
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
						Constants.ACTION_ROSTOPIC, Constants.ACTION_ROSMSG_TYPE));

					// advertise that we will publish opal audio messages
					if (this.gameConfig.opalAudioTopic == "")
					{
						Logger.LogWarning("Do not have opal configuration... trying "
							+ "default topic " + Constants.DEFAULT_AUDIO_ROSTOPIC);
						Constants.AUDIO_ROSTOPIC = Constants.DEFAULT_AUDIO_ROSTOPIC;
					}
					else 
					{
						Constants.AUDIO_ROSTOPIC = this.gameConfig.opalAudioTopic;
					}
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
						Constants.AUDIO_ROSTOPIC, Constants.AUDIO_ROSMSG_TYPE));

					// advertise that we will subscribe to opal command messages
					if (this.gameConfig.opalCommandTopic == "")
					{
						Logger.LogWarning("Do not have opal configuration... trying "
							+ "default topic " + Constants.DEFAULT_CMD_ROSTOPIC);

						Constants.CMD_ROSTOPIC = Constants.DEFAULT_CMD_ROSTOPIC;
					}
					else 
					{
						Constants.CMD_ROSTOPIC = this.gameConfig.opalCommandTopic;
					}
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
						Constants.CMD_ROSTOPIC, Constants.CMD_ROSMSG_TYPE));

					// advertise that we will publish opal log messages
					if (this.gameConfig.opalLogTopic == "")
					{
						Logger.LogWarning("Do not have opal configuration... trying "
							+ "default topic " + Constants.DEFAULT_LOG_ROSTOPIC);
						Constants.LOG_ROSTOPIC = Constants.DEFAULT_LOG_ROSTOPIC;
					}
					else 
					{
						Constants.LOG_ROSTOPIC = this.gameConfig.opalLogTopic;
					}
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
						Constants.LOG_ROSTOPIC, Constants.LOG_ROSMSG_TYPE));



					// advertise that we will publish opal scene messages
					if (this.gameConfig.opalSceneTopic == "")
					{
						Logger.LogWarning("Do not have opal configuration... trying "
							+ "default topic " + Constants.DEFAULT_SCENE_ROSTOPIC);
						Constants.SCENE_ROSTOPIC = Constants.DEFAULT_SCENE_ROSTOPIC;
					}
					else 
					{
						Constants.SCENE_ROSTOPIC = this.gameConfig.opalSceneTopic;
					}
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
						Constants.SCENE_ROSTOPIC, Constants.SCENE_ROSMSG_TYPE));

					// publish log message to opal log topic
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
						Constants.LOG_ROSTOPIC, "Opal game checking in!"));

					Logger.Log("!!!!!!!!!!!!!!!!!!! setting up all ros topics !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

				}
				else {
					Logger.LogError("Could not set up websocket!");
				}            
				// register log callback for Logger.Log calls
				//Application.logMessageReceivedThreaded += HandleApplicationLogMessageReceived;

			}

		}

		/** On disable, disable some stuff */
		void OnDestroy ()
		{
			
			// unsubscribe from Unity Logger.Log events
			//Application.logMessageReceivedThreaded -= HandleApplicationLogMessageReceived;

			// close websocket
			if(this.clientSocket != null) {
				this.clientSocket.CloseSocket();

				// unsubscribe from received message events
				//this.clientSocket.receivedMsgEvent -= HandleClientSocketReceivedMsgEvent;
			}
			Logger.Log("destroyed main game controller");
		}

		public void HandleSpeechResultReceived(Dictionary<string,double> syllables)
		{
			Logger.Log ("handle speech result received");
			if (this.clientSocket != null )
			{
				Logger.Log ("about to send message");
				// send log string over ROS
				this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishSpeechResultMsg(
					Constants.SPEECH_RESULT_ROSTOPIC2, 
					syllables,speechText
				));
//				this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
//					Constants.LOG_ROSTOPIC, Constants.LOG_ROSMSG_TYPE));
			}
		}

		void checkDestoryAnimalObject(){
			GameObject tmp = GameObject.FindGameObjectWithTag (Constants.TAG_ANIMAL_OBJECT);
			if (tmp != null) {
				if (tmp.transform.position.x < cam.ScreenToWorldPoint (new Vector3 (0.0f, 0.0f, 0.0f)).x) {
					Logger.Log ("destory current animal");
					Destroy (tmp);
					animalPresent = false;
				}
			}
		}


		// Update is called once per frame
		void Update () {
			SetGameScoreText ();
			checkDestoryAnimalObject ();
			if (animalPresent == false) {
				Vector3 initPosition = cam.ScreenToWorldPoint (new Vector3 (1.2f * Screen.width, Screen.height * 0.2f, 50.0f));
				loadAnimal (initPosition);
			}
		}

		void SetGameScoreText(){
			GameScoreText.text = "Coins: " + gameScore.ToString();
		}

		public void updateGameScore(){
			Logger.Log ("current game score: "+gameScore);
			gameScore += 1;
		}


		public void visualizeWords(){
			Logger.Log ("visualize words in game controller");
			wordViz.LoadGame(speechText);
		}

		public void destoryWords(){
			wordViz.destoryWords ();
		}

		public void updateSpeechaceResult(string word, Dictionary<string,double> syllables){
			Logger.Log ("update speechace result runs...");
			HandleSpeechResultReceived (syllables);
			bool allPass = true;
			foreach (KeyValuePair<string,double> i in syllables) {
				if (i.Value > 60) {
					Logger.Log (i.Key+" has a score above 60. Score is: "+i.Value.ToString());
					wordViz.onSyllableReceived (i.Key);
				} else {
					allPass = false;
					Logger.Log (i.Key+" has a score below 60. Score is: "+i.Value.ToString());
					//wordViz.onBadSyllableReceived (i.Key);
				}
			}
			if (allPass == true) {
				displayCoins ();
				destoryWords ();
				destoryAnimal ();
				animalPresent = false;
			}
		}

		void destoryAnimal(){
			GameObject tmp = GameObject.FindGameObjectWithTag (Constants.TAG_ANIMAL_OBJECT);
			Destroy (tmp);
		}

		void displayCoins(){
			
			for (int i = 0; i < speechText.ToCharArray ().Length; i++) {
				Vector3 tmp_pos=cam.ScreenToWorldPoint(new Vector3(((Screen.width / 2)) - (i * 38 ), 300, 20));
				Instantiate (coins, tmp_pos, Quaternion.identity);
			}
		}
			
		void loadAnimal(Vector3 initPosition){
			string animalName = animalHandler.GetNewAnimal ();
			speechText = animalName.Split ("-".ToCharArray()) [0].ToLower();
			Logger.Log ("animal name: "+speechText);
			SceneObjectProperties pops = new SceneObjectProperties ();
			pops.setAll(animalName,Constants.TAG_ANIMAL_OBJECT,initPosition,true);
			ObjectHandler.InstantiateSceneObject(pops,null,new GameObject());
			animalPresent = true;
		}
			
		/// <summary>
		/// "Speak" Button's clicked. Trigger action.
		/// </summary>
		public void onWordButtonClicked()
		{
			Logger.Log ("on word button clicked..");
			//Logger.Log ("word button is clicked...");
//			Logger.Log(cam.transform.position);
//			//Logger.Log ( cam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0.0f)).x);
//			Vector3 initPosition=cam.ScreenToWorldPoint(new Vector3(1.5f*Screen.width,Screen.height*0.2f,50.0f));
//			loadAnimal(initPosition);
			//setupROS();
			if (test == false) {
				Logger.Log ("runs setup ROS");
				test = true;
				setupROS ();
			} else {
				Logger.Log ("handles message received");
				//HandleSpeechResultReceived ();
			}
		}
	}
}
