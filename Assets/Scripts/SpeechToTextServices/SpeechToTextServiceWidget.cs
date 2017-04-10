using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnitySpeechToText.Services;
//using UnitySpeechToText.Utilities;

namespace opal
{
    /// <summary>
    /// Widget that handles interaction with a specific speech-to-text API.
    /// </summary>
    public class SpeechToTextServiceWidget : MonoBehaviour
    {

        /// <summary>
        /// Store for SpeechToTextService property
        /// </summary>
        [SerializeField]
        SpeechToTextService m_SpeechToTextService;

 		/// <summary>
        /// Delegate for recording timeout
        /// </summary>
        Action m_OnRecordingTimeout;
        /// <summary>
        /// Delegate for receiving the last text result
        /// </summary>
        Action<SpeechToTextServiceWidget> m_OnReceivedLastResponse;

        /// <summary>
        /// The specific speech-to-text service to use
        /// </summary>
        public SpeechToTextService SpeechToTextService
        {
            set
            {
                m_SpeechToTextService = value;
                RegisterSpeechToTextServiceCallbacks();
            }
        }
			
        /// <summary>
        /// Adds a function to the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnRecordingTimeout(Action action)
        {
            Logger.Log( SpeechToTextServiceString() + " register timeout");
            m_OnRecordingTimeout += action;
        }

        /// <summary>
        /// Removes a function from the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnRecordingTimeout(Action action)
        {
			Logger.Log( SpeechToTextServiceString() + " unregister timeout");
            m_OnRecordingTimeout -= action;
        }

        /// <summary>
        /// Adds a function to the received last response delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnReceivedLastResponse(Action<SpeechToTextServiceWidget> action)
        {
            m_OnReceivedLastResponse += action;
        }

        /// <summary>
        /// Removes a function from the received last response delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnReceivedLastResponse(Action<SpeechToTextServiceWidget> action)
        {
            m_OnReceivedLastResponse -= action;
        }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        void Start()
        {
            RegisterSpeechToTextServiceCallbacks();
            
        }

        /// <summary>
        /// Function that is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
		{
        }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        void OnDestroy()
        {
            UnregisterSpeechToTextServiceCallbacks();
        }
			
        /// <summary>
        /// Registers callbacks with the SpeechToTextService.
        /// </summary>
        void RegisterSpeechToTextServiceCallbacks()
        {
            if (m_SpeechToTextService != null)
            {
                m_SpeechToTextService.RegisterOnError(OnSpeechToTextError);
                m_SpeechToTextService.RegisterOnTextResult(OnTextResult);
                m_SpeechToTextService.RegisterOnRecordingTimeout(OnSpeechToTextRecordingTimeout);
            }
        }

        /// <summary>
        /// Unregisters callbacks with the SpeechToTextService.
        /// </summary>
        void UnregisterSpeechToTextServiceCallbacks()
        {
            if (m_SpeechToTextService != null)
            {
                m_SpeechToTextService.UnregisterOnError(OnSpeechToTextError);
                m_SpeechToTextService.UnregisterOnTextResult(OnTextResult);
                m_SpeechToTextService.UnregisterOnRecordingTimeout(OnSpeechToTextRecordingTimeout);
            }
        }

        /// <summary>
        /// Returns a string representation of the type of speech-to-text service used by this object.
        /// </summary>
        /// <returns>String representation of the type of speech-to-text service used by this object</returns>
        public string SpeechToTextServiceString()
        {
            return m_SpeechToTextService.GetType().ToString();
        }

        

        /// <summary>
        /// Clears the current results text and tells the speech-to-text service to start recording.
        /// </summary>
        public void StartRecording()
        {
			m_SpeechToTextService.StartRecording ();
        }

        /// <summary>
        /// Starts waiting for the last text result and tells the speech-to-text service to stop recording.
        /// If a streaming speech-to-text service stops recording and the last result sent by it was not already final,
        /// the service is guaranteed to send a final result or error after or before some defined amount of time has passed.
        /// </summary>
        /// <param name="comparisonPhrase">Optional text to compare the speech-to-text result against</param>
        public void StopRecording(string comparisonPhrase = null)
        {

            m_SpeechToTextService.StopRecording();
        }

       
        void OnTextResult(SpeechToTextResult result)
        {
			
        }

        /// <summary>
        /// Does any final processing necessary for the results of the last started session and then
        /// stops the widget from displaying results until the start of the next session.
        /// </summary>
        void ProcessEndResults()
        {
            //LogFileManager.Instance.WriteTextToFileIfShouldLog(SpeechToTextServiceString() + ": " + m_ResultsTextUI.text);
            if (m_OnReceivedLastResponse != null)
            {
                m_OnReceivedLastResponse(this);
            }
           // m_WillDisplayReceivedResults = false;
        }


        /// <summary>
        /// Function that is called when an error occurs. If this object is waiting for
        /// a last response, then this error is treated as the last "result" of the current session.
        /// </summary>
        /// <param name="text">The error text</param>
        void OnSpeechToTextError(string text)
        {

        }

        /// <summary>
        /// Function that is called when the recording times out.
        /// </summary>
        void OnSpeechToTextRecordingTimeout()
        {
           // SmartLogger.Log(DebugFlags.SpeechToTextWidgets, SpeechToTextServiceString() + " call timeout");
            if (m_OnRecordingTimeout != null)
            {
                m_OnRecordingTimeout();
            }
        }
    }
}
