namespace PBMS.Application.Payment.Services;



///< Summary>
/// Lớp hỗ trợ làm tròn tiền mặt tại tầng Apllication
/// </summary>
public class CashRoundingPolicy
{
    ///<summary>
    /// Thực hiện làm tròn số tiền dựa trên đơn vị và ngưỡng làm tròn 
    /// </summary>
    /// <param name ="amount"> Số tiền gốc ban đầu cần thanh toán </param>
    /// <param name ="unit"> Đơn vị làm tròn ( Ví dụ :500 VND). </param>
    ///<param name = "threshold"> Ngưỡng làm tròn ( Ví dụ : 250 VND). </param>
    ///<returns> Số tiền sau khi đã làm tròn </returns>

    public decimal Round(decimal amount, decimal unit, decimal threshold)
    {
        if (unit <= 0)
        {
            return amount;
        }

        decimal remainder = amount % unit;
        if (remainder >= threshold)
        {
            return amount - remainder + unit;
        }
        else
        {
            return amount - remainder;
        }
    }
}
