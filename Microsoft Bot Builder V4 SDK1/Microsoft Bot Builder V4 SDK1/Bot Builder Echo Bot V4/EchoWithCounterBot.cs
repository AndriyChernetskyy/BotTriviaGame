// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Bot_Builder_Echo_Bot_V4
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EchoWithCounterBot : IBot
    {
        private UserData _dataAboutUser;
        private LuckyTrivia _luckyTrivia;
        private readonly EchoBotAccessors _accessors;
        private readonly ILogger _logger;
        private bool check = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoWithCounterBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public EchoWithCounterBot(EchoBotAccessors accessors, ILoggerFactory loggerFactory, UserData userData, LuckyTrivia luckyTrivia)
        {
             if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

             _logger = loggerFactory.CreateLogger<EchoWithCounterBot>();
             _logger.LogTrace("EchoBot turn start.");
             _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
             _dataAboutUser = userData;
            _luckyTrivia = luckyTrivia;

        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context.
                var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count for this conversation.

               
                state.TurnCount++;
                if (!state.SaidHello)
                {
                    string message = "Hi, My Name is Bot, What is your name?";
                    await turnContext.SendActivityAsync(message);
                    state.SaidHello = true;
                }
                else if (!state.GreetedName)
                {
                    string message = "Hi, " + turnContext.Activity.Text + ", it's is nice to meet you!";
                    _dataAboutUser.Name = turnContext.Activity.Text;
                    await turnContext.SendActivityAsync(message);
                    state.GreetedName = true;
                }
                else if (!state.OfferGame)
                {
                    var message = $"{_dataAboutUser.Name} , do you want to play a lucky .NET Developer game?";
                    await turnContext.SendActivityAsync(message);
                    state.PlayAGame = true;
                    state.OfferGame = true;
                }
                else if (state.PlayAGame)
                {
                    bool gameOn = turnContext.Activity.Text.ToLower().Equals("yes") ? true : false;
                    var message = string.Empty;
                    if (gameOn)
                    {
                        message = $"That's Great! {System.Environment.NewLine} So the first question is {System.Environment.NewLine} Which of the following converts a type to a signed byte type in C#?";
                        message += $"{System.Environment.NewLine} A : ToInt64 {System.Environment.NewLine} B : ToSbyte {System.Environment.NewLine} C : ToSingle {System.Environment.NewLine} D : ToInt32 ?";
                        state.PlayAGame = false;
                    }
                    else
                    {
                        message = "Unfortunately there is no more functional! Goodbye!";
                        state.PlayAGame = false;
                    }

                    await turnContext.SendActivityAsync(message);
                }
                else if (!_luckyTrivia.AnswerToFirstAnswer)
                {
                    var answer = turnContext.Activity.Text.ToLower();
                    if (answer.Equals("b"))
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync("right answer!!!");
                        ++_luckyTrivia.Points;
                        _luckyTrivia.AnswerToFirstAnswer = true;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync("wrong (((, but do not be sad it is only first question");
                        _luckyTrivia.AnswerToFirstAnswer = true;
                    }

                    await turnContext.SendActivityAsync($"Your current number of points is {_luckyTrivia.Points.ToString()}");
                    Thread.Sleep(1000);
                    await turnContext.SendActivityAsync($"And now it is time for the second question {System.Environment.NewLine}  Which of the following access specifier in C# allows a child class to access the member variables and member functions of its base class?");
                    await turnContext.SendActivityAsync($"A : Public {System.Environment.NewLine} B :  Private {System.Environment.NewLine} C : Protected{System.Environment.NewLine} OR {System.Environment.NewLine} D : Internal");

                }
                else if (_luckyTrivia.AnswerToSecondAnswer)
                {
                    var answer = turnContext.Activity.Text.ToLower();
                    if (answer.Equals("c"))
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync("right answer!!!");
                        ++_luckyTrivia.Points;
                        _luckyTrivia.AnswerToSecondAnswer = true;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync($"wrong (((, the right answer is {System.Environment.NewLine} Protected {System.Environment.NewLine} but do not be sad it is only first question");
                        _luckyTrivia.AnswerToSecondAnswer = true;
                    }

                    await turnContext.SendActivityAsync($"Your current number of points is {_luckyTrivia.Points.ToString()}");
                    Thread.Sleep(1000);
                    await turnContext.SendActivityAsync($"And now it is time for the 3d question {System.Environment.NewLine} Which of the following preprocessor directive specifies the end of a conditional directive in C#?");
                    await turnContext.SendActivityAsync($"A : elif {System.Environment.NewLine} B : endif {System.Environment.NewLine} C : if {System.Environment.NewLine} D : else");
                }
                else if (!_luckyTrivia.AnswerToThirdAnswer)
                {
                    var answer = turnContext.Activity.Text.ToLower();
                    if (answer.Equals("b"))
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync("right answer!!!");
                        ++_luckyTrivia.Points;
                        _luckyTrivia.AnswerToThirdAnswer = true;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync($"wrong (((, the right answer is {System.Environment.NewLine} endif {System.Environment.NewLine} but do not be sad it is only first question");
                        _luckyTrivia.AnswerToThirdAnswer = true;
                    }

                    await turnContext.SendActivityAsync($"Your current number of points is {_luckyTrivia.Points.ToString()}");
                    Thread.Sleep(1000);
                    await turnContext.SendActivityAsync($"And now it is time for the 4th question {System.Environment.NewLine} Which of the following is the default access specifier of a class?");
                    await turnContext.SendActivityAsync($"A : Private {System.Environment.NewLine} B : Public {System.Environment.NewLine} C : Protected {System.Environment.NewLine} D : Internal");
                }
                else if (!_luckyTrivia.AnswerToFourthAnswer)
                {
                    var answer = turnContext.Activity.Text.ToLower();
                    if (answer.Equals("d"))
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync("right answer!!!");
                        ++_luckyTrivia.Points;
                        _luckyTrivia.AnswerToFourthAnswer = true;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync($"wrong (((, the right answer is {System.Environment.NewLine} Internal {System.Environment.NewLine} but do not be sad it is only first question");
                        _luckyTrivia.AnswerToFourthAnswer = true;
                    }

                    await turnContext.SendActivityAsync($"Your current number of points is {_luckyTrivia.Points.ToString()}");
                    Thread.Sleep(1000);
                    await turnContext.SendActivityAsync($"And now it is time for the 5th question {System.Environment.NewLine} Which of the following converts a type to a Boolean value, where possible in C#?");
                    await turnContext.SendActivityAsync($"A : ToBoolean {System.Environment.NewLine} B : ToSingle {System.Environment.NewLine} C : ToChar {System.Environment.NewLine} D : ToDateTime");
                }
                else if (!_luckyTrivia.AnswerToFifthAnswer)
                {
                    var answer = turnContext.Activity.Text.ToLower();
                    if (answer.Equals("a"))
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync("right answer!!!");
                        ++_luckyTrivia.Points;
                        _luckyTrivia.AnswerToFifthAnswer = true;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("And this is ");
                        Thread.Sleep(2000);
                        await turnContext.SendActivityAsync($"wrong (((, the right answer is {System.Environment.NewLine} ToBoolean {System.Environment.NewLine}  but do not be sad it is only first question");
                        _luckyTrivia.AnswerToFifthAnswer = true;
                    }

                    await turnContext.SendActivityAsync($"Your current number of points is {_luckyTrivia.Points.ToString()}");
                    Thread.Sleep(1000);
                    switch (_luckyTrivia.Points)
                    {
                        case 0:
                        case 1:
                        case 2:
                            await turnContext.SendActivityAsync($"Unfortunately, you are a piece of shit not a programmer");
                            break;
                        case 3:
                        case 4:
                            await turnContext.SendActivityAsync($"Man, you are in good relationship with C#!");
                            break;
                        case 5:
                            await turnContext.SendActivityAsync($"You are you, object?");
                            break;
                    }
                }
                else
                {
                    var message = $"{_dataAboutUser.Name} , do you want to play a lucky trivial game?";
                    await turnContext.SendActivityAsync(message);
                }

                await _accessors.CounterState.SetAsync(turnContext, state);
                await _accessors.ConversationState.SaveChangesAsync(turnContext);

            }
            else
            {
                if (_dataAboutUser.Name != null)
                {
                    await turnContext.SendActivityAsync($"We hope you are still here, {_dataAboutUser.Name}");
                }
                //await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
