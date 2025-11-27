namespace SimpleMedWayDemo.Services;

public class EmailVerificationService
{
    private readonly Dictionary<string, string> _otpStore = new();
    private readonly Dictionary<string, DateTime> _otpExpiry = new();
    private readonly Random _random = new();

    public Task<string> GenerateOtpAsync(string email)
    {
        // Generate 6-digit OTP
        var otp = _random.Next(100000, 999999).ToString();
        
        _otpStore[email] = otp;
        _otpExpiry[email] = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes
        
        return Task.FromResult(otp);
    }

    public Task<bool> SendOtpEmailAsync(string email, string otp)
    {
        // In production, this would send an actual email
        // For now, we'll simulate email sending
        Console.WriteLine($"[EMAIL SIMULATION] Sending OTP to {email}");
        Console.WriteLine($"[EMAIL SIMULATION] OTP Code: {otp}");
        Console.WriteLine($"[EMAIL SIMULATION] This code will expire in 10 minutes.");
        
        // Return true to simulate successful email sending
        return Task.FromResult(true);
    }

    public Task<bool> VerifyOtpAsync(string email, string otp)
    {
        if (!_otpStore.ContainsKey(email))
            return Task.FromResult(false);

        if (_otpExpiry[email] < DateTime.UtcNow)
        {
            // OTP expired
            _otpStore.Remove(email);
            _otpExpiry.Remove(email);
            return Task.FromResult(false);
        }

        var isValid = _otpStore[email] == otp;
        
        if (isValid)
        {
            // Remove OTP after successful verification
            _otpStore.Remove(email);
            _otpExpiry.Remove(email);
        }

        return Task.FromResult(isValid);
    }

    public Task<bool> IsEmailVerifiedAsync(string email)
    {
        // Check if OTP has been verified (not in store means either verified or expired)
        var isVerified = !_otpStore.ContainsKey(email);
        return Task.FromResult(isVerified);
    }

    public Task<TimeSpan?> GetOtpRemainingTimeAsync(string email)
    {
        if (!_otpExpiry.ContainsKey(email))
            return Task.FromResult<TimeSpan?>(null);

        var remaining = _otpExpiry[email] - DateTime.UtcNow;
        return Task.FromResult<TimeSpan?>(remaining > TimeSpan.Zero ? remaining : null);
    }
}
