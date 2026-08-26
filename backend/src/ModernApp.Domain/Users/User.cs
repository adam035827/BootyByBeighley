using ModernApp.Domain.Common;

namespace ModernApp.Domain.Users;

public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public SubscriptionStatus SubscriptionStatus { get; private set; }
    public string? ProfilePhotoUrl { get; private set; }
    public string? Bio { get; private set; }
    public string? SocialLinks { get; private set; }
    
    // v2 smartwatch integration fields (added for schema compatibility)
    public double? HeartRateAvgBpm { get; private set; }
    public double? HeartRateMaxBpm { get; private set; }
    public double? CaloriesBurned { get; private set; }

    public bool IsCoach => Role == UserRole.Coach;

    private User() { }

    public static User CreateStudent(
        string email,
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required", nameof(lastName));

        return new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.Student,
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateCoach(
        string email,
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required", nameof(lastName));

        return new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.Coach,
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User Reconstitute(
        Guid id,
        string email,
        UserRole role,
        string firstName,
        string lastName,
        DateTime createdAt,
        SubscriptionStatus subscriptionStatus,
        string? profilePhotoUrl = null,
        string? bio = null,
        string? socialLinks = null,
        double? heartRateAvgBpm = null,
        double? heartRateMaxBpm = null,
        double? caloriesBurned = null)
    {
        return new User
        {
            Id = id,
            Email = email,
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = createdAt,
            SubscriptionStatus = subscriptionStatus,
            ProfilePhotoUrl = profilePhotoUrl,
            Bio = bio,
            SocialLinks = socialLinks,
            HeartRateAvgBpm = heartRateAvgBpm,
            HeartRateMaxBpm = heartRateMaxBpm,
            CaloriesBurned = caloriesBurned
        };
    }

    public void UpdateProfile(
        string? profilePhotoUrl,
        string? bio,
        string? socialLinks)
    {
        ProfilePhotoUrl = profilePhotoUrl;
        Bio = bio;
        SocialLinks = socialLinks;
    }
}
