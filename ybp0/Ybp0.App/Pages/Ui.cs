namespace Ybp0.App.Pages;

#pragma warning disable CS0618
internal static class Ui
{
    public static readonly Color Mint = Color.FromArgb("#EDF8F5");
    public static readonly Color Card = Color.FromArgb("#FFFFFF");
    public static readonly Color Ink = Color.FromArgb("#13231F");
    public static readonly Color Muted = Color.FromArgb("#63756F");
    public static readonly Color Teal = Color.FromArgb("#009688");
    public static readonly Color Dark = Color.FromArgb("#101418");
    public static readonly Color Error = Color.FromArgb("#D64545");

    public static Label Text(string text, double size, Color color, FontAttributes attributes = FontAttributes.None)
    {
        return new Label
        {
            Text = text,
            FontSize = size,
            TextColor = color,
            FontAttributes = attributes,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    public static Frame CardFrame(View content, double radius = 28)
    {
        return new Frame
        {
            Content = content,
            BackgroundColor = Card,
            BorderColor = Color.FromArgb("#22FFFFFF"),
            CornerRadius = (float)radius,
            HasShadow = false,
            Padding = 22
        };
    }

    public static Frame Field(View content)
    {
        return new Frame
        {
            Content = content,
            BackgroundColor = Color.FromArgb("#F7FFFC"),
            BorderColor = Color.FromArgb("#33009688"),
            CornerRadius = 18,
            HasShadow = false,
            Padding = new Thickness(14, 2)
        };
    }

    public static Button Primary(string text)
    {
        return new Button
        {
            Text = text,
            TextColor = Colors.White,
            BackgroundColor = Teal,
            CornerRadius = 20,
            HeightRequest = 54,
            FontAttributes = FontAttributes.Bold,
            FontSize = 16
        };
    }

    public static Button Link(string text)
    {
        return new Button
        {
            Text = text,
            TextColor = Teal,
            BackgroundColor = Colors.Transparent,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(4)
        };
    }

    public static Button Nav(string text, bool active = false)
    {
        return new Button
        {
            Text = text,
            TextColor = active ? Dark : Colors.White,
            BackgroundColor = active ? Colors.White : Colors.Transparent,
            CornerRadius = 24,
            FontSize = 22,
            Padding = 0,
            HeightRequest = 48,
            WidthRequest = 58
        };
    }
}
#pragma warning restore CS0618
