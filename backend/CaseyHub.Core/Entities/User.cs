namespace CaseyHub.Core.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private readonly List<Permit> _savedPermits = new ();
    public IReadOnlyCollection<Permit> SavedPermits => _savedPermits.AsReadOnly();

    private User()
    {
        Name = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AddSavedPermit (Permit permit)
    {
        if (permit == null) throw new ArgumentNullException(nameof(permit));
        if (!_savedPermits.Any(p=>p.ApplicationNumber == permit.ApplicationNumber))
        {
            _savedPermits.Add(permit);
        }
    }
}
