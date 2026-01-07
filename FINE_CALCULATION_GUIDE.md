# H? Th?ng Tính Ti?n Ph?t (Fine Calculation System)

## ?? T?ng Quan

Ti?n ph?t ???c **tính hoàn toàn ? Backend** và **l?u vào database** t? ??ng khi sách quá h?n.

## ?? Công Th?c Tính Ti?n Ph?t

```
Ti?n Ph?t = S? Ngày Tr? × 20.000?
```

**Ví d?:**
- Tr? 1 ngày = 20.000?
- Tr? 5 ngày = 100.000?
- Tr? 10 ngày = 200.000?

## ?? Quy Trình X? Lý

### 1?? Khi User Tr? Sách Mu?n
```
User tr? sách mu?n
  ?
ReturnBookAsync()
  ?? Ki?m tra: returnDate > DueDate?
  ?? N?u có ? Tính ti?n ph?t
  ?   ?? overdueDays = (returnDate - DueDate).Days
  ?   ?? FineAmount = overdueDays × 20.000
  ?? Status = Overdue
  ?? L?u vào DB
```

### 2?? Khi GET Loans (Status Ki?m Tra Overdue)
```
GET /api/Loan hay /api/Loan/my
  ?
UpdateOverdueStatusAsync()
  ?? Ki?m tra: Status = Approved & DateTime.UtcNow > DueDate?
  ?? N?u có ? Chuy?n Status = Overdue
  ?   ?? overdueDays = (UtcNow - DueDate).Days
  ?   ?? FineAmount = overdueDays × 20.000
  ?? L?u vào DB
```

### 3?? Khi Loan ?ã Overdue (C?p Nh?t Ti?n Ph?t)
```
GET /api/Loan hay /api/Loan/my
  ?
UpdateOverdueStatusAsync()
  ?? Ki?m tra: Status = Overdue & FineAmount != Tính toán m?i?
  ?? N?u khác ? C?p nh?t FineAmount
  ?   ?? overdueDays = (UtcNow - DueDate).Days
  ?   ?? FineAmount = overdueDays × 20.000 (c?p nh?t)
  ?? L?u vào DB
```

### 4?? Khi User Thanh Toán
```
User ?n "Thanh toán ph?t"
  ?
PayFineAsync()
  ?? L?y FineAmount t? DB
  ?? Ki?m tra FineAmount > 0?
  ?? Status = Paid (ch? admin duy?t)
  ?? L?u vào DB
```

### 5?? Khi Admin Duy?t Thanh Toán
```
Admin ?n "Duy?t thanh toán"
  ?
ApprovePaymentAsync()
  ?? T?o record trong FinePayments
  ?   ?? Amount = loan.FineAmount
  ?   ?? PaymentDate = DateTime.UtcNow
  ?? Status = Returned
  ?? FineAmount = 0 (xóa kh?i Loans)
  ?? T?o Notification
  ?? L?u vào DB
```

## ?? Database

### Loans Table
```
Id, UserId, BookId, LoanDate, DueDate, ReturnDate, Status, FineAmount
```

### FinePayments Table (L?ch S? Doanh Thu)
```
Id, UserId, LoanId, Amount, PaymentDate, PaymentMethod, Description
```

## ?? Tính N?ng

- ? **Tính t? ??ng:** Ti?n ph?t ???c tính khi status thành Overdue
- ? **C?p nh?t liên t?c:** Ti?n ph?t c?p nh?t m?i l?n truy v?n (ngày tr? t?ng)
- ? **L?u vào DB:** FineAmount l?u trong b?ng Loans
- ? **L?ch s?:** Khi admin duy?t, ti?n ph?t ???c l?u vào FinePayments
- ? **Báo cáo:** Có th? th?ng kê doanh thu t? FinePayments

## ?? API Endpoints

### L?y danh sách loans (auto tính ph?t)
```
GET /api/Loan
GET /api/Loan/{id}
GET /api/Loan/my
```
Response s? bao g?m `FineAmount` ???c tính t? DB

### User thanh toán
```
PUT /api/Loan/send-request-pay-fine/{id}
```
Ch? c?n ID loan, backend s? l?y FineAmount t? DB

### Admin duy?t thanh toán
```
PUT /api/Loan/approve-payment/{id}
```
T? ??ng t?o FinePayments record và xóa FineAmount t? Loans

## ?? Thay ??i Ti?n Ph?t

N?u mu?n thay ??i ti?n ph?t t? 20.000? thành giá tr? khác:

**File:** `Services/Loans/LoanService.cs`

Tìm các dòng:
```csharp
int fineAmount = overdueDays * 20000;
```

Thay ??i `20000` thành giá tr? m?i, ví d? `30000`:
```csharp
int fineAmount = overdueDays * 30000; // 30.000?/ngày
```

## ?? Ví D? Scenario

**Ngày 1:** User m??n sách
- DueDate: 2026-01-10

**Ngày 11:** Sách quá h?n 1 ngày
- GET /api/Loan ? Status = Overdue, FineAmount = 20.000?

**Ngày 15:** Sách quá h?n 5 ngày
- GET /api/Loan ? Status = Overdue, FineAmount = 100.000?

**Ngày 20:** User tr? sách (quá h?n 10 ngày)
- PUT /api/Loan/return/1 ? Status = Overdue, FineAmount = 200.000?

**Ngày 20:** User ?n thanh toán
- PUT /api/Loan/send-request-pay-fine/1 ? Status = Paid, FineAmount = 200.000?

**Ngày 20:** Admin duy?t
- PUT /api/Loan/approve-payment/1
- ? T?o FinePayments record (Amount = 200.000?)
- ? Loans: Status = Returned, FineAmount = 0
- ? Notification ???c t?o

## ? Hoàn T?t

H? th?ng tính ti?n ph?t **100% ? Backend**, không c?n x? lý ? Frontend n?a! ??
