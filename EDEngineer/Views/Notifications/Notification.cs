namespace EDEngineer.Views.Notifications
{
    public class Notification
    {
        public Notification(NotificationContentKind contentKind, string header, string content)
        {
            ContentKind = contentKind;
            Header = header;
            Content = content;
        }

        public string Content { get; }
        public string Header { get; }
        public NotificationContentKind ContentKind { get; }
    }
}