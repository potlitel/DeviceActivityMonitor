namespace DAM.Core.Entities;

public enum UserRole { Manager, Worker }

public class ApplicationUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; } // El secreto permanente
    public string? TempTwoFactorSecret { get; set; } // El secreto temporal durante el setup
    public string? BackupCodesHash { get; set; } // Códigos de emergencia (Hasheados)
}