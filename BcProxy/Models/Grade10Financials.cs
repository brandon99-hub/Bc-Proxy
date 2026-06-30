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
    public decimal TermTotalBilled { get; set; }

    /// <summary>Total reversed via credit notes</summary>
    public decimal TermTotalCredited { get; set; }

    /// <summary>Total paid via receipts</summary>
    public decimal TermTotalPaid { get; set; }

    /// <summary>Individual fee lines charged this term</summary>
    public List<FeeLineItem> Fees { get; set; } = new();

    /// <summary>Credit notes for reversals</summary>
    public List<CreditNoteLineItem> CreditNotes { get; set; } = new();

    /// <summary>Receipts for payments</summary>
    public List<ReceiptLineItem> Receipts { get; set; } = new();
}

public class FeeLineItem
{
    public string FeeCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CreditNoteLineItem
{
    public string No { get; set; } = string.Empty;
    public string AppliesToDocNo { get; set; } = string.Empty;
    public string FeeItem { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PostingDate { get; set; } = string.Empty;
}

public class ReceiptLineItem
{
    public string No { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PostingDate { get; set; } = string.Empty;
}


