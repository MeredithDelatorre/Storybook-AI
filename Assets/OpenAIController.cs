//*********************************************************/
//This program was created by Meredith Delatorre for a
//Storybook AI application using Unity 2D and v1.6 of OkGoDoit's 
//OpenAI wrapper library. It was created for CIM 563 at the 
//University of Miami. Created on 12/11/2023. 
//*********************************************************/


using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Images;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OpenAIController : MonoBehaviour
{

    public TMP_Text textField;
    public TMP_InputField inputField;
    public Button okButton;
    public RawImage rawImage;

    private OpenAIAPI api;              //integrating api
    private List<ChatMessage> messages; //running list of all msgs from both user and chatbot 



    // Start is called before the first frame update
    void Start()
    {

        //In order for the API to work, you need to set up your OpenAI API key in your Environment Variables. You can find a tutorial on how to do this in OpenAI's website.
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User)); 
        StartConversation();
        okButton.onClick.AddListener(() => GetResponse());

    }

    private void StartConversation()
    {
        messages = new List<ChatMessage> {

            //instructions for the AI
            new ChatMessage(ChatMessageRole.System, "You are an AI storybook that gives the user a fairy tale introduction (such as \"Once upon a time there was a ...\") and lets the user decide what comes next in the story, madlib style by asking for a noun, verb, or adjective. You take in the user's input and add on to it to create a cohesive story. Keep your messages to 3 sentences max and frequently ask for user input.")   
        };

        inputField.text = "";
        string startString = " Once upon a time there was a . . . "; //starting message 
        textField.text = startString;

        Debug.Log(startString);
    }

    private async void GetResponse()
    {
        if (inputField.text.Length < 1)
        {
            return;
        }

        // Disable the OK button
        okButton.enabled = false;

        //Fill the user message from the input field
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;
        if (userMessage.Content.Length > 100)
        {
            //Limit messages to 100 characters 
            userMessage.Content = userMessage.Content.Substring(0, 100);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        // Add the message to the list
        messages.Add(userMessage);

        //Update text field with the user message
        textField.text = string.Format("You: {0}", userMessage.Content);

        //Clear the input field
        inputField.text = "";

        //Send entire chat to OpenAI to get the next message
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,     //model we're using for ChatGPT
            Temperature = 0.1,              
            MaxTokens = 200,
            Messages = messages
        });

        //Get response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

        //Add the response to the list of messages 
        messages.Add(responseMessage);

        //Update the text field with the response
        textField.text = string.Format("You: {0}\n\n AI: {1}", userMessage.Content, responseMessage.Content);

        string AI_Response = responseMessage.Content;

        //generate image based on response
        var imageResult = await api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest(AI_Response, 1, ImageSize._512));
        StartCoroutine(LoadImage(imageResult));

        Console.WriteLine(imageResult.Data[0].Url);

        //Re-enable ok button
        okButton.enabled = true;
    }


    private IEnumerator LoadImage(ImageResult imageResult)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageResult.Data[0].Url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                rawImage.texture = texture; // Assign the downloaded texture to the RawImage component
            }
            else
            {
                Debug.Log("Image download failed");
            }
        }
    }
}
