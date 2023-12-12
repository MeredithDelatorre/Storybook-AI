using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MadLibStoryGenerator : MonoBehaviour
{
    private const string apiKey = "YOUR_API_KEY"; // Replace with your OpenAI API key
    private const string baseUrl = "https://api.openai.com/v1/engines/davinci/completions"; // Example URL, adjust as per OpenAI API documentation

    private string currentStory = "Once upon a time there was a "; // Initial story prompt
    private string currentPrompt = "(noun)"; // Example prompt, can change based on your story structure

    // Function to send a prompt to OpenAI API
    private IEnumerator SendPromptToAI(string prompt)
    {
        string requestBody = "{\"prompt\": \"" + prompt + "\", \"max_tokens\": 100}"; // Adjust the prompt and max_tokens as needed

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(baseUrl, requestBody))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                // Parse the response and update the story
                string response = request.downloadHandler.text;
                string generatedText = ParseResponse(response); // Implement a function to extract the generated text from the response
                UpdateStory(generatedText);
            }
        }
    }

    // Function to update the story based on the generated text
    private void UpdateStory(string generatedText)
    {
        currentStory += generatedText + " "; // Append the generated text to the story
        Debug.Log("Current Story: " + currentStory); // Display the current story in the console or UI
    }

    // Example function to handle user input (called when user provides a noun)
    public void OnUserInput(string userInput)
    {
        string newPrompt = currentPrompt.Replace("(noun)", userInput); // Replace placeholder with user's noun input
        StartCoroutine(SendPromptToAI(newPrompt)); // Send the updated prompt to the AI
    }

    // Example function to parse the response from the OpenAI API
    private string ParseResponse(string response)
    {
        // Implement logic to extract the generated text from the API response
        // This could involve JSON parsing or other methods depending on the API response format
        // Return the generated text
        return response;
    }
}
