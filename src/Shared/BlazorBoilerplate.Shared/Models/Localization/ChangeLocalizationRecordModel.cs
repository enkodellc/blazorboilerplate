namespace BlazorBoilerplate.Shared.Models.Localization
{
    public class ChangeLocalizationRecordModel
    {
        public string ContextId { get; set; }
        public string MsgId { get; set; }

        public string NewContextId { get; set; }
        public string NewMsgId { get; set; }
    }
}
