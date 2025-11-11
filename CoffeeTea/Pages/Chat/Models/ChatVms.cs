namespace CoffeeTea.Pages.Chat.Models
{
    public class ChatThreadVm
    {
        public int ThreadId { get; set; }
        public List<ChatMessageVm> Messages { get; set; } = new();
    }

    public class ChatMessageVm
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
