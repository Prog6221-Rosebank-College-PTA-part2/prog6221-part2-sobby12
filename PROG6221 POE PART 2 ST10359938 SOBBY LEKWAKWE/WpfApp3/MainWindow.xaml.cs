using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

namespace WpfApp3
{
    public partial class MainWindow : Window
    {
        // Memory variables
        private string userName = "";
        private string favoriteTopic = "";
        private string lastTopic = "";
        private List<string> chatHistory = new List<string>();

        // Bot components
        private Random random = new Random();
        private SpeechSynthesizer speechSynthesizer;

        // Response database
        private Dictionary<string, List<string>> keywordResponses;

        // Conversation flow
        private int followUpCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSpeech();
            InitializeResponses();
            ShowWelcomeMessage();
        }

        private void InitializeSpeech()
        {
            try
            {
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SetOutputToDefaultAudioDevice();
            }
            catch (Exception)
            {
                // Speech not available - app continues working
            }
        }

        private void InitializeResponses()
        {
            keywordResponses = new Dictionary<string, List<string>>()
            {
                ["password"] = new List<string>
                {
                    "Make sure to use strong, unique passwords for each account. Avoid using personal details like birthdays in your passwords.",
                    "A strong password should be at least 12 characters with uppercase, lowercase, numbers, and symbols. Consider using a password manager!",
                    "Enable two-factor authentication wherever possible. Even if someone steals your password, they can't access your account without the second factor."
                },

                ["scam"] = new List<string>
                {
                    "Never share OTPs or personal information with anyone. Scammers often pretend to be from your bank or tech support.",
                    "If you receive a call claiming urgent action is needed, hang up immediately. Call back using the official number from the company's website.",
                    "Report phishing attempts to your IT team or the real organization. Forward suspicious emails to report@phishing.gov.uk"
                },

                ["privacy"] = new List<string>
                {
                    "Review your app permissions regularly. Remove access for apps you don't use anymore.",
                    "Use a VPN when on public Wi-Fi. Public networks are easy targets for hackers.",
                    "Check your privacy settings on social media. You might be sharing more than you think!"
                },

                ["phishing"] = new List<string>
                {
                    "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                    "Hover over links before clicking. Check for misspelled domains like amaz0n.com instead of amazon.com.",
                    "Look for generic greetings like 'Dear Customer' instead of your real name - that's a common scam tactic!"
                },

                ["ransomware"] = new List<string>
                {
                    "Always maintain offline backups of important files. This is your best defense against ransomware!",
                    "Never pay the ransom. Only 8 percent of companies get their data back after paying.",
                    "Keep your software updated and be careful with email attachments to prevent ransomware infections."
                },

                ["2fa"] = new List<string>
                {
                    "Two-Factor Authentication adds an extra layer of security. Enable it on your email, banking, and social media accounts.",
                    "Use authenticator apps like Google Authenticator instead of SMS when possible. SMS can be intercepted.",
                    "Save your backup codes in a secure location when setting up Two-Factor Authentication. You'll need them if you lose your phone."
                }
            };
        }

        private void ShowWelcomeMessage()
        {
            AppendToChat("================================================");
            AppendToChat("     CYBERSECURITY AWARENESS CHATBOT");
            AppendToChat("         Your Personal Security Assistant");
            AppendToChat("================================================");
            AppendToChat("");
            AppendToChat("Hello! I'm your cybersecurity assistant.");
            AppendToChat("");
            AppendToChat("You can ask me about:");
            AppendToChat("   - Password security");
            AppendToChat("   - Scam protection");
            AppendToChat("   - Privacy tips");
            AppendToChat("   - Phishing detection");
            AppendToChat("   - Ransomware prevention");
            AppendToChat("   - Two-Factor Authentication (2FA)");
            AppendToChat("");
            AppendToChat("Example questions:");
            AppendToChat("   - 'Tell me about password safety'");
            AppendToChat("   - 'How to spot a scam?'");
            AppendToChat("   - 'What is phishing?'");
            AppendToChat("   - 'Give me another tip'");
            AppendToChat("");
            AppendToChat("What's your name?");
        }

        private void AppendToChat(string message)
        {
            Dispatcher.Invoke(() =>
            {
                Paragraph paragraph = new Paragraph(new Run(message));
                paragraph.Margin = new Thickness(0, 3, 0, 3);
                ChatRichTextBox.Document.Blocks.Add(paragraph);

                // Auto-scroll to bottom
                ChatScrollViewer.ScrollToBottom();

                chatHistory.Add(message);
            });
        }

        private void SpeakText(string text)
        {
            try
            {
                if (speechSynthesizer != null)
                {
                    // Remove special characters for cleaner speech
                    string cleanText = System.Text.RegularExpressions.Regex.Replace(text, @"[^\u0000-\u007F]+", "");
                    cleanText = cleanText.Replace("*", "").Replace("-", "").Replace("\n", ". ");

                    if (!string.IsNullOrWhiteSpace(cleanText))
                    {
                        speechSynthesizer.SpeakAsync(cleanText);
                    }
                }
            }
            catch (Exception)
            {
                // Speech failed - continue
            }
        }

        private void ProcessInput()
        {
            string input = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            AppendToChat("You: " + input);

            // If name not set yet, store it
            if (string.IsNullOrEmpty(userName))
            {
                userName = input;
                AppendToChat("Bot: " + "Nice to meet you, " + userName + "! I'll remember your name. Ask me about cybersecurity to learn how to stay safe online!");
                SpeakText("Nice to meet you, " + userName + "! I'll remember your name. Ask me about cybersecurity to learn how to stay safe online!");
                InputTextBox.Clear();
                return;
            }

            // Process the question
            string response = GetBotResponse(input.ToLower());
            AppendToChat("Bot: " + response);
            SpeakText(response);

            InputTextBox.Clear();
        }

        private string GetBotResponse(string input)
        {
            // Sentiment detection
            string sentiment = DetectSentiment(input);
            if (sentiment == "worried")
            {
                AppendToChat("Bot: I understand it can be scary to think about cyber threats. Don't worry - let me help you take small, manageable steps to stay safe.");
                SpeakText("I understand it can be scary to think about cyber threats. Don't worry, let me help you.");
            }
            else if (sentiment == "frustrated")
            {
                AppendToChat("Bot: I know security can feel frustrating sometimes. Think of it like locking your front door - it takes a second but keeps you safe!");
                SpeakText("I know security can feel frustrating. Think of it like locking your front door - it takes a second but keeps you safe.");
            }
            else if (sentiment == "curious")
            {
                AppendToChat("Bot: That's a great question! Curiosity is your best defense against cyber threats.");
                SpeakText("That's a great question. Curiosity is your best defense against cyber threats.");
            }

            // Conversation flow - follow ups
            if (input.Contains("tell me more") || input.Contains("explain more") ||
                input.Contains("another tip") || input.Contains("more details"))
            {
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    followUpCount++;
                    if (keywordResponses.ContainsKey(lastTopic) && followUpCount <= 3)
                    {
                        int randomIndex = random.Next(keywordResponses[lastTopic].Count);
                        return "Here's more about " + lastTopic + ": " + keywordResponses[lastTopic][randomIndex];
                    }
                    else
                    {
                        return "We've covered " + lastTopic + " quite a bit! Would you like to learn about passwords, scams, or privacy instead?";
                    }
                }
                return "What topic would you like to know more about? Try asking about passwords, scams, or privacy.";
            }

            // Random tip
            if (input.Contains("random tip") || input.Contains("give me a tip"))
            {
                string[] topics = keywordResponses.Keys.ToArray();
                string randomTopic = topics[random.Next(topics.Length)];
                string tip = keywordResponses[randomTopic][random.Next(keywordResponses[randomTopic].Count)];
                return "Here's a random tip about " + randomTopic + ": " + tip;
            }

            // Memory - store favorite topic
            if (input.Contains("interested in"))
            {
                if (input.Contains("password")) favoriteTopic = "passwords";
                else if (input.Contains("scam")) favoriteTopic = "scams";
                else if (input.Contains("privacy")) favoriteTopic = "privacy";
                else if (input.Contains("phishing")) favoriteTopic = "phishing";

                if (!string.IsNullOrEmpty(favoriteTopic))
                {
                    return "Great! I'll remember that you're interested in " + favoriteTopic + ". Staying educated is the best defense!";
                }
            }

            // Recall memory in responses
            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                string topic = favoriteTopic;
                if (topic.EndsWith("s")) topic = topic.Substring(0, topic.Length - 1);

                if (keywordResponses.ContainsKey(topic) && (input.Contains("advice") || input.Contains("help me")))
                {
                    int randomIndex = random.Next(keywordResponses[topic].Count);
                    return "Since you're interested in " + favoriteTopic + ", here's relevant advice: " + keywordResponses[topic][randomIndex];
                }
            }

            // Keyword recognition
            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    lastTopic = keyword;
                    followUpCount = 0;
                    int randomIndex = random.Next(keywordResponses[keyword].Count);
                    string response = keywordResponses[keyword][randomIndex];

                    // Personalize with user's name
                    if (!string.IsNullOrEmpty(userName))
                    {
                        response = userName + ", " + response.ToLower();
                        response = char.ToUpper(response[0]) + response.Substring(1);
                    }

                    return response;
                }
            }

            // Help command
            if (input.Contains("help") || input.Contains("what can i ask"))
            {
                return "You can ask me about: passwords, scams, privacy, phishing, ransomware, or 2FA. Try 'tell me about password safety' or 'how to spot a scam?'";
            }

            // Greetings
            if (input.Contains("hello") || input.Contains("hi"))
            {
                return "Hello " + userName + "! How can I help you with cybersecurity today?";
            }

            if (input.Contains("how are you"))
            {
                return "I'm doing great! Ready to help you stay safe online.";
            }

            if (input.Contains("thank"))
            {
                return "You're welcome! Stay safe online!";
            }

            if (input.Contains("bye") || input.Contains("goodbye"))
            {
                return "Goodbye " + userName + "! Remember to use strong passwords and enable 2FA. Stay safe!";
            }

            // Default response for unrecognized input
            return "I'm not sure I understand. Can you try rephrasing? You can ask me about passwords, scams, privacy, or phishing. Type 'help' to see all options.";
        }

        private string DetectSentiment(string input)
        {
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious") || input.Contains("nervous"))
                return "worried";
            if (input.Contains("frustrated") || input.Contains("annoyed") || input.Contains("angry") || input.Contains("confused"))
                return "frustrated";
            if (input.Contains("curious") || input.Contains("interesting"))
                return "curious";
            return "neutral";
        }

        // Button event handlers
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessInput();
            }
        }

        private void SpeakButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the last bot message and speak it
            string lastBotMsg = "";
            for (int i = chatHistory.Count - 1; i >= 0; i--)
            {
                if (chatHistory[i].StartsWith("Bot:"))
                {
                    lastBotMsg = chatHistory[i].Replace("Bot:", "").Trim();
                    break;
                }
            }

            if (!string.IsNullOrEmpty(lastBotMsg))
            {
                SpeakText(lastBotMsg);
            }
            else
            {
                SpeakText("No response to speak.");
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Text Files (*.txt)|*.txt";
                saveDialog.DefaultExt = ".txt";
                saveDialog.FileName = "ChatLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllLines(saveDialog.FileName, chatHistory);
                    AppendToChat("Chat history exported successfully!");
                }
            }
            catch (Exception ex)
            {
                AppendToChat("Error exporting: " + ex.Message);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatRichTextBox.Document.Blocks.Clear();
            chatHistory.Clear();
            ShowWelcomeMessage();
        }
    }
}