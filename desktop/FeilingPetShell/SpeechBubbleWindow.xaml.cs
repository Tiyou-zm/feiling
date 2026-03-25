using System.Windows;

namespace FeilingPetShell;

public partial class SpeechBubbleWindow : Window
{
    public SpeechBubbleWindow()
    {
        InitializeComponent();
    }

    public void SetText(string text)
    {
        SpeechText.Text = text;
        InvalidateMeasure();
        UpdateLayout();
    }
}
