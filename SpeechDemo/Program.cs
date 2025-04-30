using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;

namespace TimeKeeperAI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string aiSvcKey = configuration["SpeechKey"];
            string aiSvcRegion = configuration["SpeechRegion"];

            var speechConfig = SpeechConfig.FromSubscription(aiSvcKey, aiSvcRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-GB-LibbyNeural"; // Change to your preferred neural voice

            Console.WriteLine("🎤 Andrew and Ava are in a work session on -Azure AI Speech- presentation. you want to Hi?! '");

            //Hi Andrew, Hi Ava, I see you guys are working hard in here...

            string command = await RecognizeSpeechAsync(speechConfig);

            if (!string.IsNullOrWhiteSpace(command))
            {
                // Check if the command contains words that trigger the Microsoft story
                if (command.ToLower().Contains("hard") || command.ToLower().Contains("working"))
                {
                    string storyResponse = GenerateMicrosoftStorySSML(speechConfig.SpeechSynthesisVoiceName);
                    await SynthesizeSpeechAsync(speechConfig, storyResponse, isSsml: true);
                }
                else
                {
                    Console.WriteLine("⏱ No relevant command detected.");
                    await SynthesizeSpeechAsync(speechConfig, "I didn't hear a story command. Please try again.");
                }
            }
        }
        static async Task<string> RecognizeSpeechAsync(SpeechConfig config)
        {
            using var recognizer = new SpeechRecognizer(config);

            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"✅ You said: {result.Text}");
                return result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("⚠️ Speech could not be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var details = CancellationDetails.FromResult(result);
                Console.WriteLine($"❌ Canceled: {details.Reason}");
                Console.WriteLine($"🔎 Error Details: {details.ErrorDetails}");
            }
            return null;
        }
        static async Task SynthesizeSpeechAsync(SpeechConfig config, string responseText, bool isSsml = false)
        {
            using var synthesizer = new SpeechSynthesizer(config);
            SpeechSynthesisResult result;

            if (isSsml)
            {
                result = await synthesizer.SpeakSsmlAsync(responseText);
            }
            else
            {
                result = await synthesizer.SpeakTextAsync(responseText);
            }

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine("✅ Speech synthesis completed successfully.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Console.WriteLine($"❌ Speech synthesis canceled: {cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"🔎 ErrorCode: {cancellation.ErrorCode}");
                    Console.WriteLine($"🧾 ErrorDetails: {cancellation.ErrorDetails}");
                    Console.WriteLine($"💡 Did you set the speech resource key and region correctly?");
                }
            }
        }
        static string GenerateMicrosoftStorySSML(string voiceName)
        {
            // Generating a 50th-anniversary story in SSML format
            return $@"
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='https://www.w3.org/2001/mstts' xml:lang='en-US'>
              <voice name='en-US-MultiTalker-Ava-Andrew:DragonHDLatestNeural'>
                <mstts:dialog>
                  <mstts:turn speaker='ava'>Hi Michel! Andrew and I are working on a new presentation about AI speech technology.</mstts:turn>
                  <mstts:turn speaker='andrew'>Hello Michel! We’re really excited to show how AI is transforming communication.</mstts:turn>
                  <mstts:turn speaker='ava'>Exactly! We've been focusing on how text-to-speech can create natural, dynamic conversations.</mstts:turn>
                  <mstts:turn speaker='andrew'>And even make digital assistants sound more human and expressive!</mstts:turn>
                  <mstts:turn speaker='ava'>We think AI voices like ours could make education, accessibility, and entertainment more engaging.</mstts:turn>
                  <mstts:turn speaker='andrew'>Thanks for letting us share a sneak peek with you, Michel. We can't wait to hear what you think!</mstts:turn>
                </mstts:dialog>
              </voice>
            </speak>";
        }
    }
}

