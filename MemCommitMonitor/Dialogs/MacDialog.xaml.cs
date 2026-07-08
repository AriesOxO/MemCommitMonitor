using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MemCommitMonitor.Dialogs;

public partial class MacDialog : Window
{
    public enum DialogIcon
    {
        None,
        Info,
        Warning,
        Error,
        Question,
        Success
    }

    public enum DialogButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public new bool? DialogResult { get; private set; }

    public MacDialog(string title, string content, DialogIcon icon = DialogIcon.Info, DialogButton buttons = DialogButton.OK)
    {
        InitializeComponent();

        TitleText.Text = title;
        ContentText.Text = content;
        SetIcon(icon);
        CreateButtons(buttons);
    }

    private void SetIcon(DialogIcon icon)
    {
        IconText.Text = icon switch
        {
            DialogIcon.Info => "ℹ️",
            DialogIcon.Warning => "⚠️",
            DialogIcon.Error => "❌",
            DialogIcon.Question => "❓",
            DialogIcon.Success => "✅",
            _ => ""
        };

        if (icon == DialogIcon.None)
        {
            IconText.Visibility = Visibility.Collapsed;
        }
    }

    private void CreateButtons(DialogButton buttons)
    {
        var buttonPanel = (StackPanel)((Grid)Content).Children[2];

        switch (buttons)
        {
            case DialogButton.OK:
                AddButton(buttonPanel, "确定", true, () => { DialogResult = true; Close(); });
                break;

            case DialogButton.OKCancel:
                AddButton(buttonPanel, "取消", false, () => { DialogResult = false; Close(); });
                AddButton(buttonPanel, "确定", true, () => { DialogResult = true; Close(); });
                break;

            case DialogButton.YesNo:
                AddButton(buttonPanel, "否", false, () => { DialogResult = false; Close(); });
                AddButton(buttonPanel, "是", true, () => { DialogResult = true; Close(); });
                break;

            case DialogButton.YesNoCancel:
                AddButton(buttonPanel, "取消", false, () => { DialogResult = null; Close(); });
                AddButton(buttonPanel, "否", false, () => { DialogResult = false; Close(); });
                AddButton(buttonPanel, "是", true, () => { DialogResult = true; Close(); });
                break;
        }
    }

    private void AddButton(StackPanel panel, string text, bool isPrimary, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 80,
            Height = 32,
            Margin = new Thickness(8, 0, 0, 0),
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        // 应用样式
        if (isPrimary)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(0, 122, 255)); // MacOS Blue
            button.Foreground = Brushes.White;
        }
        else
        {
            button.Background = new SolidColorBrush(Color.FromRgb(242, 242, 247)); // MacOS LightGray
            button.Foreground = Brushes.Black;
        }

        button.BorderThickness = new Thickness(0);

        // 设置圆角和阴影
        var template = new ControlTemplate(typeof(Button));
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        factory.SetValue(Border.PaddingProperty, new Thickness(16, 0, 16, 0));

        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        factory.AppendChild(contentPresenter);

        template.VisualTree = factory;
        button.Template = template;

        button.Click += (s, e) => onClick();

        panel.Children.Add(button);
    }

    public static bool? Show(string title, string content, DialogIcon icon = DialogIcon.Info, DialogButton buttons = DialogButton.OK, Window? owner = null)
    {
        var dialog = new MacDialog(title, content, icon, buttons);
        if (owner != null)
        {
            dialog.Owner = owner;
        }
        dialog.ShowDialog();
        return dialog.DialogResult;
    }
}
