using Microsoft.EntityFrameworkCore;
using MyApi.Entities;

namespace MyApi.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Payment> CreatePaymentAsync(Guid userId, int? loanId, decimal amount, string orderInfo, DateTime? expiredAt)
        {
            try
            {
                var payment = new Payment
                {
                    OrderId = $"ORD_{userId.ToString().Substring(0, 8)}_{DateTime.UtcNow.Ticks}",
                    UserId = userId,
                    LoanId = loanId,
                    Amount = amount,
                    Status = "Pending",
                    Description = orderInfo,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = expiredAt
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"CreatePaymentAsync: Created payment OrderId={payment.OrderId}, Amount={amount}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreatePaymentAsync Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> UpdatePaymentPendingAsync(string orderId)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment == null)
                {
                    _logger.LogWarning($"UpdatePaymentPendingAsync: Payment not found OrderId={orderId}");
                    throw new Exception($"Payment not found: {orderId}");
                }

                payment.Status = "Pending";
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"UpdatePaymentPendingAsync: Updated OrderId={orderId}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdatePaymentPendingAsync Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> UpdatePaymentSuccessAsync(string orderId, string transactionId, string vnPayResponseCode)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Loan)
                        .ThenInclude(l => l.Book)
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);
                
                if (payment == null)
                {
                    _logger.LogWarning($"UpdatePaymentSuccessAsync: Payment not found OrderId={orderId}");
                    throw new Exception($"Payment not found: {orderId}");
                }

                payment.Status = "Success";
                payment.TransactionId = transactionId;
                payment.VnPayResponseCode = vnPayResponseCode;
                payment.PaidAt = DateTime.UtcNow;

                // Update Loan status if linked
                if (payment.LoanId.HasValue)
                {
                    var loan = await _context.Loans
                        .Include(l => l.User)
                        .Include(l => l.Book)
                        .FirstOrDefaultAsync(l => l.Id == payment.LoanId);
                    
                    if (loan != null)
                    {
                        var fineAmountBeforeUpdate = loan.FineAmount;
                        
                        // ? C?p nh?t Loan status thành "Paid" - ch? Admin duy?t tr? sách
                        loan.Status = LoanStatus.Paid.ToString();
                        loan.FineAmount = 0; // Reset ti?n ph?t v? 0
                        // KHÔNG set ReturnDate ? ?ây - ch? Admin duy?t
                        loan.UpdatedAt = DateTime.UtcNow;
                        _context.Loans.Update(loan);

                        // ? T?o record trong FinePayments table
                        if (fineAmountBeforeUpdate > 0)
                        {
                            var finePayment = new FinePayment
                            {
                                UserId = payment.UserId,
                                LoanId = payment.LoanId.Value,
                                Amount = fineAmountBeforeUpdate,
                                PaymentDate = DateTime.UtcNow,
                                PaymentMethod = "VNPay",
                                Description = $"Thanh toán ti?n ph?t qua VNPay - OrderId: {payment.OrderId}",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.FinePayments.Add(finePayment);
                        }

                        // ? T?o Notification thông báo thanh toán thành công
                        var bookTitle = loan.Book?.Title ?? "sách";
                        var notification = new Notification
                        {
                            UserId = payment.UserId,
                            Title = "Thanh toán thành công",
                            Message = $"B?n ?ã thanh toán thành công ti?n ph?t {fineAmountBeforeUpdate:N0}? cho sách \"{bookTitle}\". Vui lòng ch? Admin duy?t tr? sách.",
                            Type = NotificationType.PaymentSuccess.ToString(),
                            LoanId = loan.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);

                        _logger.LogInformation(
                            $"UpdatePaymentSuccessAsync: Loan {loan.Id} - " +
                            $"Status='Paid', FineAmount reset from {fineAmountBeforeUpdate} to 0, " +
                            $"Waiting for Admin approval to change to 'Returned'");
                    }
                }

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"UpdatePaymentSuccessAsync: Updated OrderId={orderId}, TransactionId={transactionId}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdatePaymentSuccessAsync Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> UpdatePaymentFailedAsync(string orderId, string errorMessage)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment == null)
                {
                    _logger.LogWarning($"UpdatePaymentFailedAsync: Payment not found OrderId={orderId}");
                    throw new Exception($"Payment not found: {orderId}");
                }

                payment.Status = "Failed";
                payment.ErrorMessage = errorMessage;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                _logger.LogWarning($"UpdatePaymentFailedAsync: Updated OrderId={orderId}, Error={errorMessage}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdatePaymentFailedAsync Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(string orderId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Loan)
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPaymentByOrderIdAsync Error: {ex.Message}");
                return null;
            }
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Loan)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPaymentByIdAsync Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(Guid userId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.Loan)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetUserPaymentsAsync Error: {ex.Message}");
                return new List<Payment>();
            }
        }

        public async Task<List<Payment>> GetLoanPaymentsAsync(int loanId)
        {
            try
            {
                return await _context.Payments
                    .Include(p => p.User)
                    .Where(p => p.LoanId == loanId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLoanPaymentsAsync Error: {ex.Message}");
                return new List<Payment>();
            }
        }

        public async Task<bool> IsPaymentSuccessAsync(string orderId)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                return payment != null && payment.Status == "Success";
            }
            catch (Exception ex)
            {
                _logger.LogError($"IsPaymentSuccessAsync Error: {ex.Message}");
                return false;
            }
        }

        // ? Hàm m?i: Validate và l?y FineAmount t? Loan
        public async Task<decimal> GetLoanFineAmountAsync(int loanId)
        {
            try
            {
                var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
                if (loan == null)
                {
                    _logger.LogWarning($"GetLoanFineAmountAsync: Loan not found LoanId={loanId}");
                    return 0;
                }

                _logger.LogInformation($"GetLoanFineAmountAsync: LoanId={loanId}, FineAmount={loan.FineAmount}");
                return loan.FineAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLoanFineAmountAsync Error: {ex.Message}");
                return 0;
            }
        }

        // ? Hàm m?i: Check n?u Loan có ti?n ph?t ch?a thanh toán
        public async Task<bool> HasUnpaidFineAsync(int loanId)
        {
            try
            {
                var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
                return loan != null && loan.FineAmount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"HasUnpaidFineAsync Error: {ex.Message}");
                return false;
            }
        }

        // ? Hàm m?i: Validate Payment Amount vs Loan FineAmount
        public async Task<(bool IsValid, string Message)> ValidatePaymentAmountAsync(int loanId, decimal paymentAmount)
        {
            try
            {
                var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
                
                if (loan == null)
                {
                    return (false, $"Loan not found: LoanId={loanId}");
                }

                if (loan.Status == "Returned")
                {
                    return (false, "Loan already returned. No fine to pay.");
                }

                if (loan.FineAmount <= 0)
                {
                    return (false, "No fine amount to pay for this loan.");
                }

                if (paymentAmount < loan.FineAmount)
                {
                    return (false, $"Payment amount ({paymentAmount}) is less than fine amount ({loan.FineAmount})");
                }

                if (paymentAmount > loan.FineAmount)
                {
                    return (false, $"Payment amount ({paymentAmount}) exceeds fine amount ({loan.FineAmount}). Please pay exact amount.");
                }

                return (true, "Payment amount is valid");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ValidatePaymentAmountAsync Error: {ex.Message}");
                return (false, $"Validation error: {ex.Message}");
            }
        }
    }
}
