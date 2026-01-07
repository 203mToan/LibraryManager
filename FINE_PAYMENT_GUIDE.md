# H? Th?ng Qu?n Lý Doanh Thu Ph?t (Fine Payment System)

## ?? T?ng Quan

H? th?ng này giúp theo dõi và báo cáo t?t c? các kho?n thanh toán ph?t tr? h?n sách, v?i kh? n?ng l?c theo ngày, tu?n, tháng, n?m.

## ??? B?ng D? Li?u

### FinePayments
```
Id (PK)          - Mã thanh toán
UserId (FK)      - Mã ng??i dùng
LoanId (FK)      - Mã phi?u m??n
Amount           - S? ti?n ph?t
PaymentDate      - Ngày thanh toán
PaymentMethod    - Ph??ng th?c thanh toán (Cash, Card, Online, etc.)
Description      - Mô t?
CreatedAt        - Ngày t?o
UpdatedAt        - Ngày c?p nh?t
```

## ?? API Endpoints

### 1. L?y t?t c? thanh toán ph?t
```
GET /api/FinePayment
Authorization: Bearer <admin_token>
```

**Response:**
```json
{
  "totalPayments": 5,
  "totalAmount": 200000,
  "payments": [
    {
      "id": 1,
      "userId": "7c54be15-8679-43bf-b238-3260be5182a6",
      "userName": "Toan",
      "loanId": 28,
      "bookName": "Sapiens",
      "amount": 40000,
      "paymentDate": "2026-01-07T10:30:00Z",
      "paymentMethod": "Cash",
      "description": "Fine payment for book: Sapiens"
    }
  ]
}
```

### 2. L?y thanh toán ph?t theo kho?ng ngày
```
GET /api/FinePayment/by-date?startDate=2026-01-01&endDate=2026-01-31
Authorization: Bearer <admin_token>
```

### 3. L?y thanh toán ph?t theo tu?n
```
GET /api/FinePayment/by-week?week=1&year=2026
Authorization: Bearer <admin_token>
```

### 4. L?y thanh toán ph?t theo tháng
```
GET /api/FinePayment/by-month?month=1&year=2026
Authorization: Bearer <admin_token>
```

### 5. L?y thanh toán ph?t theo n?m
```
GET /api/FinePayment/by-year?year=2026
Authorization: Bearer <admin_token>
```

### 6. L?y th?ng kê doanh thu
```
GET /api/FinePayment/statistics
Authorization: Bearer <admin_token>
```

**Response:**
```json
{
  "dailyTotal": 40000,
  "weeklyTotal": 80000,
  "monthlyTotal": 200000,
  "yearlyTotal": 500000,
  "byDate": [
    {
      "date": "2026-01-07",
      "total": 40000,
      "count": 1
    },
    {
      "date": "2026-01-08",
      "total": 40000,
      "count": 1
    }
  ]
}
```

## ?? Frontend Usage

### Component báo cáo doanh thu
```typescript
import { 
  getFinePaymentStatistics,
  getFinePaymentsByMonth,
  getFinePaymentsByYear
} from './services/finePaymentService';

export const FinePaymentReport = () => {
  const [statistics, setStatistics] = useState(null);
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());

  useEffect(() => {
    const fetchData = async () => {
      const stats = await getFinePaymentStatistics();
      setStatistics(stats);
    };
    fetchData();
  }, []);

  return (
    <div className="p-6">
      <h1>Báo Cáo Doanh Thu Ph?t</h1>
      
      {/* Th?ng kê nhanh */}
      <div className="grid grid-cols-4 gap-4 mb-6">
        <div className="bg-blue-50 p-4 rounded-lg">
          <h3>Hôm Nay</h3>
          <p className="text-2xl font-bold">?{statistics?.dailyTotal?.toLocaleString()}</p>
        </div>
        <div className="bg-green-50 p-4 rounded-lg">
          <h3>Tu?n Này</h3>
          <p className="text-2xl font-bold">?{statistics?.weeklyTotal?.toLocaleString()}</p>
        </div>
        <div className="bg-yellow-50 p-4 rounded-lg">
          <h3>Tháng Này</h3>
          <p className="text-2xl font-bold">?{statistics?.monthlyTotal?.toLocaleString()}</p>
        </div>
        <div className="bg-purple-50 p-4 rounded-lg">
          <h3>N?m Nay</h3>
          <p className="text-2xl font-bold">?{statistics?.yearlyTotal?.toLocaleString()}</p>
        </div>
      </div>

      {/* Bi?u ?? theo ngày */}
      <div className="bg-white p-4 rounded-lg shadow">
        <h2 className="text-xl font-semibold mb-4">Doanh Thu Theo Ngày (Tháng {month}/{year})</h2>
        <table className="w-full">
          <thead>
            <tr className="border-b">
              <th className="text-left p-2">Ngày</th>
              <th className="text-right p-2">S? Ti?n</th>
              <th className="text-right p-2">S? L?n</th>
            </tr>
          </thead>
          <tbody>
            {statistics?.byDate?.map(item => (
              <tr key={item.date} className="border-b hover:bg-gray-50">
                <td className="p-2">{item.date}</td>
                <td className="text-right p-2">?{item.total?.toLocaleString()}</td>
                <td className="text-right p-2">{item.count}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
```

## ?? Flow Ho?t ??ng

### Khi Admin Duy?t Thanh Toán
```
1. User ?n "Thanh toán ph?t"
   ? Status: Overdue ? Paid
   
2. Admin ?n "Duy?t thanh toán"
   ? ApprovePaymentAsync()
   ?? T?o FinePayment record
   ?? Status: Paid ? Returned
   ?? FineAmount: 40000 ? 0
   ?? T?o Notification
   ?? Save

3. Doanh thu ph?t ???c ghi l?i
   ? Có th? truy v?n qua API báo cáo
```

## ?? L?c Báo Cáo

### Theo ngày
```typescript
const report = await getFinePaymentsByDate('2026-01-01', '2026-01-31');
```

### Theo tu?n
```typescript
const report = await getFinePaymentsByWeek(1, 2026); // Tu?n 1 n?m 2026
```

### Theo tháng
```typescript
const report = await getFinePaymentsByMonth(1, 2026); // Tháng 1 n?m 2026
```

### Theo n?m
```typescript
const report = await getFinePaymentsByYear(2026);
```

## ? Tính N?ng

- ? T? ??ng ghi l?i thanh toán ph?t
- ? Báo cáo theo ngày, tu?n, tháng, n?m
- ? Th?ng kê t?ng doanh thu
- ? L?c theo ng??i dùng
- ? Hi?n th? ph??ng th?c thanh toán
- ? Ghi chú thanh toán

## ??? Tài Li?u

- `Entities/FinePayment.cs` - Entity
- `Services/FinePayments/IFinePaymentService.cs` - Interface
- `Services/FinePayments/FinePaymentService.cs` - Implementation
- `Controllers/FinePaymentController.cs` - API Endpoints
- `frontend/services/finePaymentService.ts` - Frontend Service
