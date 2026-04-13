namespace BcProxy.Models;

/// <summary>
/// The clean API response returned for each Grade 10 student.
/// Combines data from Studentfees (billing lines) and customerbalance (balance).
/// </summary>
public class Grade10StudentFinancials
{
    public string StudentNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Programme { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;

    /// <summary>Current outstanding balance from customerbalance.Balance_LCY</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>Total billed this term (sum of all fee line amounts)</summary>
    public decimal TermTotal { get; set; }

    /// <summary>Individual fee lines charged this term</summary>
    public List<FeeLineItem> Fees { get; set; } = new();
}

public class FeeLineItem
{
    public string FeeCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
