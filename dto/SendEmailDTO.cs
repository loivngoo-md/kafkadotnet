namespace kafkadotnet.dto;

public class SendEmailDTO
{
    public string Subject { get; init; } = string.Empty;
    public List<string> To { get; init; } = new List<string>();
    public List<string> CC { get; init; } = new List<string>();
    public string Body { get; init; } = string.Empty;
}