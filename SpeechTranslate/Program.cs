using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
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

            var translationConfig = SpeechTranslationConfig.FromSubscription(aiSvcKey, aiSvcRegion);
            translationConfig.SpeechRecognitionLanguage = "en-US"; // You speak English
            translationConfig.AddTargetLanguage("fr"); // Translate to Spanish

            Console.WriteLine("🎤 Say something in English. I will translate and speak it in French!");

            string translatedText = await RecognizeAndTranslateSpeechAsync(translationConfig);

            if (!string.IsNullOrWhiteSpace(translatedText))
            {
                await SynthesizeSpeechAsync(aiSvcKey, aiSvcRegion, translatedText);
            }
            else
            {
                Console.WriteLine("⚠️ No recognizable speech detected.");
            }
        }

        static async Task<string> RecognizeAndTranslateSpeechAsync(SpeechTranslationConfig config)
        {
            using var recognizer = new TranslationRecognizer(config);

            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.TranslatedSpeech)
            {
                Console.WriteLine($"✅ Recognized: {result.Text}");
                Console.WriteLine($"🌎 Translated (French): {result.Translations["fr"]}");
                return result.Translations["fr"];
            }
            else if (result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"✅ Recognized but no translation: {result.Text}");
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("⚠️ No speech recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"❌ Canceled: {cancellation.Reason}");
                Console.WriteLine($"🔎 Details: {cancellation.ErrorDetails}");
            }
            return null;
        }

        static async Task SynthesizeSpeechAsync(string key, string region, string text)
        {
            var speechConfig = SpeechConfig.FromSubscription(key, region);
            speechConfig.SpeechSynthesisLanguage = "fr-FR"; // Speak in French
            speechConfig.SpeechSynthesisVoiceName = "fr-FR-VivienneMultilingualNeural"; // Choose a pleasant Spanish voice

            using var synthesizer = new SpeechSynthesizer(speechConfig);

            var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine($"❗ Speech synthesis failed: {result.Reason}");
            }
        }
    }
}

