using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Otp
{
    public int Id { get; set; }

    public string? IpClient { get; set; }

    public string Email { get; set; } = null!;

    public byte[]? Otphash { get; set; }

    public byte[]? OtphashSalt { get; set; }

    public int OtpSpamNumber { get; set; }

    public DateTime OtpSpam { get; set; }

    public DateTime LimitTimeToSendOtp { get; set; }
}
