namespace DoDo.Open.ChatGPT
{
    public class ChatGPTOutput
    {
        public List<ChatGPTChoice> Choices { get; set; }
    }

    public class ChatGPTChoice
    {
        public string Text { get; set; }
    }
}
