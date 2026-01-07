# H? Th?ng Thông Báo (Notification System)

## ? Hoàn t?t

### Backend (C#/.NET)

#### 1. Database
- ? B?ng `Notifications` ?ã ???c t?o th? công
- Foreign Keys: UserId (CASCADE), LoanId (SET NULL)
- Fields: Id, CreatedAt, UpdatedAt, UserId, Title, Message, Type, LoanId, IsRead

#### 2. Entities
- ? `Notification.cs` - Entity class
- ? `NotificationType` enum - LoanApproved, PaymentApproved, LoanOverdue, PaymentRequested

#### 3. Models
- ? `NotificationResponse.cs` - Response model

#### 4. Services
- ? `INotificationService.cs` - Interface
- ? `NotificationService.cs` - Implementation
  - `CreateNotificationAsync()` - T?o thông báo m?i
  - `GetUserNotificationsAsync()` - L?y t?t c? thông báo c?a user
  - `GetUnreadNotificationsAsync()` - L?y thông báo ch?a ??c
  - `MarkAsReadAsync()` - ?ánh d?u m?t thông báo là ?ã ??c
  - `MarkAllAsReadAsync()` - ?ánh d?u t?t c? thông báo là ?ã ??c
  - `DeleteNotificationAsync()` - Xóa thông báo

#### 5. Controllers
- ? `NotificationController.cs` - API endpoints
  - `GET /api/Notification` - Danh sách thông báo
  - `GET /api/Notification/unread` - Thông báo ch?a ??c + s? l??ng
  - `PUT /api/Notification/read/{id}` - ?ánh d?u ?ã ??c
  - `PUT /api/Notification/read-all` - ?ánh d?u t?t c? ?ã ??c
  - `DELETE /api/Notification/{id}` - Xóa thông báo

#### 6. Integration
- ? `LoanService.ApproveLoanAsync()` - T? ??ng t?o notification khi admin duy?t m??n sách
- ? `LoanService.ApprovePaymentAsync()` - T? ??ng t?o notification khi admin duy?t thanh toán ph?t
- ? `Program.cs` - ??ng ký NotificationService
- ? `AppDbContext.cs` - Thêm DbSet<Notification>

---

### Frontend (TypeScript/React)

#### 1. Services
- ? `notificationService.ts` - API calls
  - `getNotifications()` - L?y danh sách thông báo
  - `getUnreadNotifications()` - L?y thông báo ch?a ??c
  - `markNotificationAsRead()` - ?ánh d?u ?ã ??c
  - `markAllNotificationsAsRead()` - ?ánh d?u t?t c? ?ã ??c
  - `deleteNotification()` - Xóa thông báo

#### 2. Components
- ? `NotificationDropdown.tsx` - Component chuông thông báo
  - Hi?n th? s? thông báo ch?a ??c
  - Danh sách thông báo v?i icons theo lo?i
  - T? ??ng refresh m?i 30 giây
  - ?ánh d?u ?ã ??c / xóa t?ng thông báo

---

## ?? H??ng D?n S? D?ng

### 1. Tích h?p Component vào Navbar/Header

```tsx
import NotificationDropdown from './components/NotificationDropdown';

export const Header = () => {
  return (
    <header className="flex justify-between items-center p-4 bg-white shadow">
      <h1>Library Manager</h1>
      <nav className="flex items-center gap-4">
        {/* Thêm component ? ?ây */}
        <NotificationDropdown onNotificationRead={() => {
          // Refresh d? li?u n?u c?n
        }} />
        <UserProfile />
      </nav>
    </header>
  );
};
```

### 2. API Endpoints

#### L?y t?t c? thông báo
```bash
GET /api/Notification
Authorization: Bearer <token>
```

Response:
```json
[
  {
    "id": 1,
    "userId": "7c54be15-8679-43bf-b238-3260be5182a6",
    "title": "Phê duy?t m??n sách",
    "message": "Yêu c?u m??n sách \"Sapiens\" ?ã ???c phê duy?t.",
    "type": "LoanApproved",
    "loanId": 28,
    "isRead": false,
    "createdAt": "2026-01-07T10:30:00Z"
  }
]
```

#### L?y thông báo ch?a ??c
```bash
GET /api/Notification/unread
Authorization: Bearer <token>
```

Response:
```json
{
  "unreadCount": 2,
  "notifications": [
    {
      "id": 1,
      "userId": "7c54be15-8679-43bf-b238-3260be5182a6",
      "title": "Phê duy?t m??n sách",
      "message": "Yêu c?u m??n sách \"Sapiens\" ?ã ???c phê duy?t.",
      "type": "LoanApproved",
      "loanId": 28,
      "isRead": false,
      "createdAt": "2026-01-07T10:30:00Z"
    }
  ]
}
```

#### ?ánh d?u thông báo là ?ã ??c
```bash
PUT /api/Notification/read/1
Authorization: Bearer <token>
```

#### ?ánh d?u t?t c? thông báo là ?ã ??c
```bash
PUT /api/Notification/read-all
Authorization: Bearer <token>
```

#### Xóa thông báo
```bash
DELETE /api/Notification/1
Authorization: Bearer <token>
```

---

## ?? Flow Ho?t ??ng

### Khi Admin Duy?t M??n Sách
```
1. Admin ?n "Phê duy?t" yêu c?u m??n sách
   ?
2. ApproveLoanAsync() ???c g?i
   ?? Status: Pending ? Approved
   ?? ? T?o Notification: "Phê duy?t m??n sách"
   ?
3. User nh?n thông báo (chuông + dropdown)
   ?
4. User click thông báo ?? xem chi ti?t
   ?
5. Thông báo ???c ?ánh d?u là "?ã ??c"
```

### Khi Admin Duy?t Thanh Toán Ph?t
```
1. User ?n "Thanh toán ph?t"
   ?
2. Status: Overdue ? Paid (ch? admin)
   ?
3. Admin ?n "Duy?t thanh toán"
   ?
4. ApprovePaymentAsync() ???c g?i
   ?? Status: Paid ? Returned
   ?? FineAmount: 40000 ? 0
   ?? ? T?o Notification: "Phê duy?t thanh toán ph?t"
   ?
5. User nh?n thông báo
```

---

## ?? Lo?i Thông Báo

| Type | Icon | Màu | Ý Ngh?a |
|------|------|------|---------|
| LoanApproved | ? | Green | Admin phê duy?t m??n sách |
| PaymentApproved | ?? | Blue | Admin phê duy?t thanh toán |
| LoanOverdue | ? | Red | Sách quá h?n |
| PaymentRequested | ?? | Yellow | Có yêu c?u thanh toán ph?t |

---

## ?? Tùy Ch?nh

### Thay ??i style c?a NotificationDropdown
- Ch?nh s?a Tailwind classes trong `NotificationDropdown.tsx`
- Ho?c thay th? b?ng th? vi?n UI khác (Material-UI, Chakra UI, etc.)

### Thêm lo?i thông báo m?i
1. Thêm enum vào `NotificationType`
2. T?o notification t?i các ch? c?n thi?t
3. C?p nh?t icons và colors trong component

---

## ? Hoàn t?t! 

H? th?ng thông báo ?ã s?n sàng s? d?ng. B?n có th?:
- ? L?y thông báo t? API
- ? Hi?n th? dropdown chuông
- ? ?ánh d?u ?ã ??c
- ? Xóa thông báo
- ? T? ??ng refresh m?i 30 giây
