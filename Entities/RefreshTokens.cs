namespace MyApi.Entities
{
    public class RefreshTokens : BaseEntity<Guid>
    {
        public string Token { get; set; } = null!;
        public DateTime? ExpiresAt { get; set;  }
        public Guid UserId { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsUsed { get; set; }
        public User User { get; set; }

    }
}
