namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// VNPay payment gateway integration service.
/// </summary>
public interface IVnPayService
{
    /// <summary>
    /// Creates a VNPay payment URL that redirects the user to VNPay's payment page.
    /// </summary>
    /// <param name="orderId">Internal order ID.</param>
    /// <param name="amount">Amount in VND.</param>
    /// <param name="orderInfo">Description shown on VNPay.</param>
    /// <param name="clientIpAddress">Client's IP address for VNPay logging.</param>
    /// <returns>Full VNPay redirect URL.</returns>
    string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string clientIpAddress);

    /// <summary>
    /// Validates the VNPay callback signature (IPN or ReturnUrl).
    /// </summary>
    /// <param name="queryParams">All query string parameters from VNPay callback.</param>
    /// <returns>True if signature is valid.</returns>
    bool ValidateCallback(IDictionary<string, string> queryParams);
}
