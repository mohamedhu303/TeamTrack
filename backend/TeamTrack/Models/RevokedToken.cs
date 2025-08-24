public class RevokedToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime RevokeAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpirationDate { get; set; }
}
