using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CxFlatUI;
using CxFlatUI.Controls;
using CxFlatUI.CustomTypes;
using FirstAidQuiz.Properties;
using MoreLinq;
using WinFormAnimation;

namespace FirstAidQuiz.Controllers {

    public class ListViewController {
        private readonly Color DarkStuff = Color.FromArgb(60, 60, 60);
        private static double ReachedPercentage { get; set; }
        private static readonly List<Fragen> ChosenQuestions = new List<Fragen>();
        private static readonly List<Antworten> AllAnswersToCurrentQuestions = new List<Antworten>();
        private static readonly IDictionary<int, Antworten> ChosenAnswers = new Dictionary<int, Antworten>();

        // TODO: Make those responsive https://www.codeproject.com/Articles/1140717/A-Responsive-Design-Technique-for-WinForms
        public void RenderInitialView(Form view) {
            var groupBoxWidth = view.ClientSize.Width / 2;
            var groupBoxHeight = view.ClientSize.Height / 2;

            var randomTake = new Random();
            var dbConnection = new Questions();
            var questions = dbConnection.Fragens.ToList();
            for (var i = 0; i < 15; i++) {
                Fragen currentQuestion;
                do {
                    currentQuestion = questions.ElementAt(randomTake.Next(0, 99));
                } while (currentQuestion == null || ChosenQuestions.Any(q => q == currentQuestion));
                ChosenQuestions.Add(currentQuestion);
                AllAnswersToCurrentQuestions.AddRange(currentQuestion.Antwortens);

                var groupBox = new CxFlatGroupBox(new Font("Segoe UI", 1)) {
                    Name = $"groupox{i}",
                    Text = currentQuestion.Frage,
                    Size = new Size(groupBoxWidth, groupBoxHeight),
                    Left = (view.ClientSize.Width - groupBoxWidth) / 2,
                    Top = ((view.ClientSize.Height - groupBoxHeight) / 2),
                    ShowText = true,
                    ThemeColor = ThemeColors.DarkPrimary, // This isn't working but I'll extend it in my free time probably
                    BackColor = this.DarkStuff,
                    ////Font = new Font("Segoe UI", 6),
                    HeaderStyle = new HeaderStyle(new CustomDrawLine(new Pen(ThemeColors.OneLevelBorder, 1), 0, 70, groupBoxWidth, 70), new CustomRectangleF(15, 0, groupBoxWidth, 50), new Font("arial", 9F), new SolidBrush(Color.White), StringAlign.Center)
                };

                // Sets up the initial objects in the CheckedListBox.
                var answers = currentQuestion.Antwortens.ToArray();
                for (var j = 1; j <= answers.Length; j++) {
                    var questionItem = new CxFlatButton {
                        Name = $"button{j + i - 1}",
                        Text = answers[j - 1].Antwort,
                        Size = new Size(250, 100),
                        Font = new Font("Arial", 10),
                        ButtonType = ButtonType.Success
                    };

                    // TODO: Make this more reasonable
                    if (j == 1) {
                        questionItem.Location = new Point(groupBox.Width / 2 - 265, groupBox.Height / 2 + 50);
                    } else if (j == 2) {
                        questionItem.Location = new Point(groupBox.Width / 2 + 15, groupBox.Height / 2 + 50);
                    } else if (j == 3) {
                        questionItem.Location = new Point(groupBox.Width / 2 - 265, groupBox.Height / 2 - 60);
                    } else if (j == 4) {
                        questionItem.Location = new Point(groupBox.Width / 2 + 15, groupBox.Height / 2 - 60);
                    }

                    questionItem.Click += this.AddSelectedOptionForAnswer;
                    groupBox.Controls.Add(questionItem);
                }

                view.Controls.Add(groupBox);
            }
        }

        public void AddSelectedOptionForAnswer(object sender, EventArgs e) {
            var selectedItem = (CxFlatButton)sender;
            var buttonGroup = (CxFlatGroupBox)selectedItem.Parent;
            var listView = (Form)buttonGroup.Parent;
            var boxIndex = listView.Controls.IndexOf(buttonGroup);
            var selectedAnswer = AllAnswersToCurrentQuestions.FirstOrDefault(a => a.Antwort == selectedItem.Text);
            var rightwrong = new PictureBox();
            buttonGroup.Enabled = false;
            rightwrong.Width = 600;
            rightwrong.Height = 600;
            rightwrong.Left = listView.Width / 2;
            rightwrong.Top = listView.Height / 30;
            rightwrong.Image = selectedAnswer != null && selectedAnswer.Richtig ? Image.FromFile("Resources\\check.jpg") : Image.FromFile("Resources\\x.png");
            listView.Controls.Add(rightwrong);
            new Animator2D(new Path2D(buttonGroup.Location.ToFloat2D(), new Point(-listView.Width, buttonGroup.Location.Y).ToFloat2D(), 300)).Play(buttonGroup, Animator2D.KnownProperties.Location, new SafeInvoker(() => {
                // TODO: Work around re-render by invoking new Render and setting visibility to false
                buttonGroup.Invoke(new Action(() => {
                    listView.Controls.Remove(rightwrong);
                }));
            }));

            if (ChosenAnswers.ContainsKey(boxIndex)) {
                ChosenAnswers[boxIndex] = selectedAnswer;
            } else {
                ChosenAnswers.Add(new KeyValuePair<int, Antworten>(boxIndex, selectedAnswer));
            }

            this.CheckShouldEnableSubmitButton(listView);
        }

        public void CheckShouldEnableSubmitButton(Form view) {
            if (ChosenAnswers.Count == 15) {
                ReachedPercentage = ChosenAnswers.Values.Where(answer => answer.Richtig).Select(answer => answer).Count();
                ReachedPercentage = Math.Round((ReachedPercentage / 15 * 100), 2);

                if (ReachedPercentage >= 75) {
                    var winLabel = new Label {
                        Text = string.Format(CultureInfo.InvariantCulture, Resources.WinningTextPercent, ReachedPercentage.ToString(CultureInfo.InvariantCulture)),
                        Size = new Size(400, 400)
                    };

                    view.Controls.Add(winLabel);
                } else {
                    var loseLabel = new Label {
                        Text = string.Format(CultureInfo.InvariantCulture, Resources.LosingTextPercent, ReachedPercentage.ToString(CultureInfo.InvariantCulture)),
                        Size = new Size(400, 400),
                        Left = view.ClientSize.Width / 2,
                        Top = view.ClientSize.Height / 2
                    };
                    var restartButton = new CxFlatButton {
                        Text = Resources.Restart,
                        Font = new Font("arial", 10),
                        Size = new Size(200, 50),
                        Left = 400,
                        Top = 400
                    };
                    restartButton.Click += this.TriggerRestart;

                    var reachedPercentageSplit = ReachedPercentage.ToString(CultureInfo.InvariantCulture).Split('.');

                    var circularBar = new CircularProgressBar.CircularProgressBar {
                        AnimationFunction = KnownAnimationFunctions.Liner,
                        Anchor = AnchorStyles.None,
                        AnimationSpeed = 500,
                        BackColor = Color.Transparent,
                        Font = new Font("Microsoft Sans Serif", 27.75F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(240, 240, 240),
                        InnerColor = Color.FromArgb(52, 73, 94),
                        InnerMargin = 0,
                        InnerWidth = -1,
                        Left = (int)(view.ClientSize.Width / 1.5),
                        Top = view.ClientSize.Height / 3,
                        MarqueeAnimationSpeed = 2000,
                        OuterColor = Color.White,
                        OuterMargin = 0,
                        OuterWidth = 0,
                        ProgressColor = Color.FromArgb(52, 151, 218),
                        ProgressWidth = 17,
                        RightToLeft = RightToLeft.No,
                        SecondaryFont = new Font("Microsoft Sans Serif", 15.75F),
                        Size = new Size(180, 180),
                        StartAngle = 270,
                        SubscriptColor = Color.FromArgb(200, 200, 200),
                        SubscriptMargin = new Padding(5, -20, 0, 0),
                        SubscriptText = $".{ (reachedPercentageSplit.Length > 1 ? reachedPercentageSplit[1] : "00")}",
                        SuperscriptColor = Color.FromArgb(200, 200, 200),
                        SuperscriptMargin = new Padding(8, 25, 0, 0),
                        SuperscriptText = "%",
                        Text = reachedPercentageSplit[0],
                        TextMargin = new Padding(0, 5, 0, 0),
                        Value = 75
                    };

                    view.Controls.Add(circularBar);
                    view.Controls.Add(loseLabel);
                    view.Controls.Add(restartButton);
                }
            }
        }

        public void TriggerRestart(object sender, EventArgs e) {
            var selectedItem = (CxFlatButton)sender;
            var listView = (Form)selectedItem.Parent;
            var addedControls = listView.Controls.Cast<Control>().Where(c => !c.Name.Contains("Header")).ToList();
            addedControls.ForEach(c => listView.Controls.Remove(c));

            ChosenAnswers.Clear();
            ChosenQuestions.Clear();
            AllAnswersToCurrentQuestions.Clear();
            ReachedPercentage = 0;
            this.RenderInitialView(listView);
        }
    }
}