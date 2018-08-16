#region Namespaces
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WinForms = System.Windows.Forms;
#endregion

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
     public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    [TemplatePart(Name = "PART_HourEditor", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_MinuteEditor", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_SecondEditor", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_UpButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DownButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_Time", Type = typeof(ComboBox))]
    public class TimePicker : Control
    {
        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }
        static bool HourValidateValue(object value)
        {
            int t = (int)value;
            return !(t < 0 || t > 12);
        }
        static bool MinuteValidateValue(object value)
        {
            int t = (int)value;
            return !(t < 0 || t > 60);
        }
        static bool SecondValidateValue(object value)
        {
            int t = (int)value;
            return !(t < 0 || t > 60);
        }
        public int Hour
        {
            get { return (int)GetValue(HourProperty); }
            set { SetValue(HourProperty, value); }
        }
        public int Minute
        {
            get { return (int)GetValue(MinuteProperty); }
            set { SetValue(MinuteProperty, value); }
        }
        public int Second
        {
            get { return (int)GetValue(SecondProperty); }
            set { SetValue(SecondProperty, value); }
        }
        public TimeType TimeType
        {
            get { return (TimeType)GetValue(TimeTypeProperty); }
            set { SetValue(TimeTypeProperty, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.upButton = this.Template.FindName("PART_UpButton", this) as RepeatButton;
            this.downButton = this.Template.FindName("PART_DownButton", this) as RepeatButton;
            this.hourEditor = this.Template.FindName("PART_HourEditor", this) as TextBox;
            this.minuteEditor = this.Template.FindName("PART_MinuteEditor", this) as TextBox;
            this.secondEditor = this.Template.FindName("PART_SecondEditor", this) as TextBox;
            this.upButton.Click += RepeatButton_Click;
            this.downButton.Click += RepeatButton_Click;
        }
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyProperty dp = null;
            int maxValue = 60;
            if (this.hourEditor.IsFocused)
            {
                dp = HourProperty;
                maxValue = 12;
            }
            if (this.minuteEditor.IsFocused) dp = MinuteProperty;
            if (this.secondEditor.IsFocused) dp = SecondProperty;
            if (dp == null) return;
            int value = (int)this.GetValue(dp);
            if (e.Source == this.upButton)
                ++value;
            else
                --value;
            if (value < 0 || value > maxValue) return;
            this.SetValue(dp, value);
        }
        public static readonly DependencyProperty TimeTypeProperty =
        DependencyProperty.Register("TimeType", typeof(TimeType), typeof(TimePicker), new FrameworkPropertyMetadata(TimeType.AM));
        public static readonly DependencyProperty HourProperty =
        DependencyProperty.Register("Hour", typeof(int), typeof(TimePicker), new FrameworkPropertyMetadata(), HourValidateValue);
        public static readonly DependencyProperty MinuteProperty =
        DependencyProperty.Register("Minute", typeof(int), typeof(TimePicker), new FrameworkPropertyMetadata(), MinuteValidateValue);
        public static readonly DependencyProperty SecondProperty =
        DependencyProperty.Register("Second", typeof(int), typeof(TimePicker), new FrameworkPropertyMetadata(), SecondValidateValue);
        private RepeatButton upButton, downButton;
        private TextBox hourEditor, minuteEditor, secondEditor;
    }
    [Serializable]
    public enum TimeType { AM, PM }
}
