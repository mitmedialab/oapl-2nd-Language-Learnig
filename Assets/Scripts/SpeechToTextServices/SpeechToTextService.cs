using System;
using UnityEngine;
//using UnitySpeechToText.Utilities;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MiniJSON;

namespace opal
{
    /// <summary>
    /// Base class for speech-to-text SDK.
    /// </summary>
    public class SpeechToTextService : MonoBehaviour
    {
        /// <summary>
        /// Store for the IsRecording property
        /// </summary>
        protected bool m_IsRecording;
        /// <summary>
        /// Delegate for text result
        /// </summary>
        protected Action<SpeechToTextResult> m_OnTextResult;
        /// <summary>
        /// Delegate for error
        /// </summary>
        protected Action<string> m_OnError;
        /// <summary>
        /// Delegate for recording timeout
        /// </summary>
        protected Action m_OnRecordingTimeout;


		/// <summary>
		/// Component used to manage temporary audio files
		/// </summary>
		protected TempAudioFileSavingComponent m_TempAudioComponent = new TempAudioFileSavingComponent("NonStreamingAudio");
		/// <summary>

        /// <summary>
        /// Whether the service is recording audio
        /// </summary>
        public bool IsRecording { get { return m_IsRecording; } }

		/// <summary>
		/// Store accuracy scores from Speechace
		/// </summary>
		double qualityScore=0;
		Dictionary<string,Dictionary<string,double>> wordsSyllableScoreList = new Dictionary<string,Dictionary<string,double>>();

		/// <summary>
		///  script for word result visualization
		/// </summary>
		[SerializeField]
		WordVizManager wordViz;

		/// <summary>
		/// The specific speech-to-text service to use
		/// </summary>
		public WordVizManager WordVizManager
		{
			set
			{
				wordViz = value;
			}
		}


		/// <summary>
		/// Instance for GameController
		/// </summary>
		public GameController gameController;

		void Awake(){
			//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
			if (GameController.instance == null) {
				//Instantiate gameManager prefab
				Instantiate (gameController);
			}
			gameController = GameController.instance;
		}

			

//		public void CreateVisualizationWord(string iword){
//			wordViz.LoadGame (iword);
//
//		}

		public void UpdateVisualizationWord(){
			Logger.Log ("update visualization word");
			//iterate through the syllablle dictionary
			if (wordsSyllableScoreList.Count > 1) {
				Logger.LogWarning ("More than one words will be visualized! ");
			} else if (wordsSyllableScoreList.Count == 1) {
				foreach(KeyValuePair<string, Dictionary<string,double>> pair in wordsSyllableScoreList){
					//SyllableAccuracy(pair.Value);
					gameController.updateSpeechaceResult(pair.Key,pair.Value);
				}
			}
			wordsSyllableScoreList.Clear ();
		}

		//for syllables passing the accuracy threshold, play them into the letter holders
		//for syllables below the accuracy rate, send to ROS
//		public void SyllableAccuracy(Dictionary<string,double> syllables){
//			foreach (KeyValuePair<string,double> i in syllables) {
//				//wordViz.onSyllableReceived ();
//				gameController.updateSyllable(i);
//			}
//		}



        /// <summary>
        /// Adds a function to the text result delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnTextResult(Action<SpeechToTextResult> action)
        {
            m_OnTextResult += action;
        }

        /// <summary>
        /// Removes a function from the text result delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnTextResult(Action<SpeechToTextResult> action)
        {
            m_OnTextResult -= action;
        }

        /// <summary>
        /// Adds a function to the error delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnError(Action<string> action)
        {
            m_OnError += action;
        }

        /// <summary>
        /// Removes a function from the error delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnError(Action<string> action)
        {
            m_OnError -= action;
        }

        /// <summary>
        /// Adds a function to the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnRecordingTimeout(Action action)
        {
            m_OnRecordingTimeout += action;
        }

        /// <summary>
        /// Removes a function from the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnRecordingTimeout(Action action)
        {
            m_OnRecordingTimeout -= action;
        }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        protected virtual void Start()
        {
            AudioRecordingManager.Instance.RegisterOnTimeout(OnRecordingTimeout);
        }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (AudioRecordingManager.Instance != null)
            {
                AudioRecordingManager.Instance.UnregisterOnTimeout(OnRecordingTimeout);
            }
			m_TempAudioComponent.ClearTempAudioFiles();
        }

        /// <summary>
        /// Function that is called when the recording times out.
        /// </summary>
        protected void OnRecordingTimeout()
        {
            m_IsRecording = false;
            if (m_OnRecordingTimeout != null)
            {
                m_OnRecordingTimeout();
            }
        }

        /// <summary>
        /// Starts recording audio if the service is not already recording.
        /// </summary>
        /// <returns>Whether the service successfully started recording</returns>
        public bool StartRecording()
        {
			wordsSyllableScoreList.Clear ();
			Logger.Log ("speech to text serivce start recording");
            if (!m_IsRecording)
            {
                m_IsRecording = true;
                StopAllCoroutines();
				Logger.Log ("non streaming start recording");
				StartCoroutine(RecordAndTranslateToText());
				return true;
            }
            return false;
        }

        /// <summary>
        /// Stops recording audio if the service is already recording.
        /// </summary>
        /// <returns>Whether the service successfully stopped recording</returns>
        public bool StopRecording()
        {
            if (m_IsRecording)
            {
                m_IsRecording = false;
				AudioRecordingManager.Instance.StopRecording();
				return true;
            }
            return false;
        }



		/// <summary>
		/// Records audio and translates any speech to text.
		/// </summary>
		IEnumerator RecordAndTranslateToText()
		{
			yield return AudioRecordingManager.Instance.RecordAndWaitUntilDone();

			gameController.visualizeWords ();

			StartCoroutine(TranslateRecordingToText());
		}

		/// <summary>
		/// Translates speech to text by making a request to the speech-to-text API.
		/// </summary>
		protected IEnumerator TranslateRecordingToText(){
			Logger.Log (" !!!!!!translate recording text!!!!!");
			m_TempAudioComponent.ClearTempAudioFiles();
			string wavAudioFilePath = SavWav.Save(m_TempAudioComponent.TempAudioRelativePath(), AudioRecordingManager.Instance.RecordedAudio);

		
			// run sh command to send http request to

			string speechText = "\""+gameController.speechText+"\"";
			Logger.Log ("speech text for sh: "+speechText);
			ProcessStartInfo proc = new ProcessStartInfo();
			proc.FileName = "/bin/bash";
			//proc.FileName = "/Library/Frameworks/Python.framework/Versions/2.7/Resources/Python.app/Contents/MacOS/Python";
			string WorkingDirectory="/Users/huilichen/speechace/";
			proc.WorkingDirectory = WorkingDirectory;
			proc.Arguments = "speechace_test.sh "+speechText;
			//proc.Arguments = "speechace_evaluation.py";
			proc.WindowStyle = ProcessWindowStyle.Minimized;
			proc.CreateNoWindow = true;
			proc.RedirectStandardOutput = true;
			proc.UseShellExecute = false;
			Process process = new Process();
			process.StartInfo = proc;
			process.Start();
			yield return null;
			string q = "";
			while ( ! process.HasExited ) {
				q += process.StandardOutput.ReadToEnd();
				//Logger.Log (q);
				yield return null;
			}
			Logger.Log (q);
			DecodeSpeechAceJson(q);


			Logger.Log (" !!!!!!processor stops....!!!!!");
			UpdateVisualizationWord ();

			//clear audio file
			m_TempAudioComponent.ClearTempAudioFiles();
		}

		void DecodeSpeechAceJson (string q){
			

			Dictionary<string, object> data = null;
			data = Json.Deserialize(q) as Dictionary<string, object>;
			if(data == null) {   
				Logger.LogWarning("[decode SpeechAce Json] Could not parse JSON message!");
				return;
			}
			Logger.Log("[decode SpeechAce msg] deserialized " + data.Count + " objects from JSON!");

			//check whether the message is valid
			if(!data.ContainsKey("status") && !data.ContainsKey("quota_remaining")) 
			{
				Logger.LogWarning("[decode Speechace msg] Did not get a valid message!");
				return;
			}

			//check whether the message status is success
			if (!data["status"].Equals("success")) {
				Logger.LogWarning ((string)data ["status"]);
				Logger.LogWarning ("[decode Speechace msg] Failed to encode speech");
				return;
			}


			if (data.ContainsKey ("text_score")) {
				//parse actual text score
				//get overal quality score
				Dictionary<string, object> testScore = data ["text_score"] as Dictionary<string, object>;
				qualityScore = (double)testScore ["quality_score"];
				Logger.Log ("Overal quality sore: " + qualityScore);

				if (!testScore.ContainsKey ("word_score_list")) {
					Logger.LogWarning ("[decode Speechace msg] Failed to encode word score list");
					return;
				}
				//keys: letters, phone_count, pitch_range, quality_score,stress_level, stress_score
				//Dictionary<string, object> tmp = testScore["word_score_list"] as Dictionary<string, object>;

				// get syllable score list (for each word)
				foreach (Dictionary<string,object> el in testScore["word_score_list"] as IEnumerable) {
					Logger.Log ("for each....");
					//Logger.Log(Json.Serialize (el)); 
					Dictionary<string,double> syllables = new Dictionary<string,double> ();
					foreach(Dictionary<string,object> syllable in el["syllable_score_list"] as IEnumerable){
						syllables.Add ((string)syllable["letters"], (double)syllable ["quality_score"]);
						Logger.Log (syllable ["letters"]);
					}
					Logger.Log ((string)el["word"]);
					wordsSyllableScoreList.Add((string)el["word"],syllables);

				}

			}


		}

	
	}
}
