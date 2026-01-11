# API Báo Cáo (Report APIs)

## ?? T?ng Quan

4 API endpoint ?? hi?n th? d? li?u báo cáo v?i kh? n?ng l?c theo tu?n/tháng/n?m.

---

## ?? API Endpoints

### **1. Xu H??ng M??n Sách (Loan Trend)**
```
GET /api/Report/loan-trend
```

**Parameters:**
- `period` (required): "week", "month", "year"
- `year` (required): 2026
- `month` (optional): 1-12 (khi period = "month")
- `week` (optional): 1-53 (khi period = "week")

**Example Requests:**
```bash
# Xu h??ng tháng 1/2026
GET /api/Report/loan-trend?period=month&year=2026&month=1

# Xu h??ng n?m 2026
GET /api/Report/loan-trend?period=year&year=2026

# Xu h??ng tu?n 1/2026
GET /api/Report/loan-trend?period=week&year=2026&week=1
```

**Response:**
```json
{
  "period": "month",
  "data": [
    {
      "label": "Tu?n 1",
      "loanCount": 10,
      "returnCount": 5
    },
    {
      "label": "Tu?n 2",
      "loanCount": 8,
      "returnCount": 7
    },
    {
      "label": "Tu?n 3",
      "loanCount": 12,
      "returnCount": 10
    },
    {
      "label": "Tu?n 4",
      "loanCount": 6,
      "returnCount": 3
    }
  ]
}
```

---

### **2. Phân B? Th? Lo?i (Category Distribution)**
```
GET /api/Report/category-distribution
```

**Parameters:**
- `year` (required): 2026
- `month` (required): 1-12

**Example Request:**
```bash
GET /api/Report/category-distribution?year=2026&month=1
```

**Response:**
```json
{
  "categories": [
    {
      "categoryId": 1,
      "categoryName": "Science / Cosmology",
      "loanCount": 25,
      "percentage": 35.21
    },
    {
      "categoryId": 2,
      "categoryName": "Self-help / Personal Development",
      "loanCount": 18,
      "percentage": 25.35
    },
    {
      "categoryId": 3,
      "categoryName": "Historical Fiction",
      "loanCount": 12,
      "percentage": 16.90
    },
    {
      "categoryId": 4,
      "categoryName": "Fantasy",
      "loanCount": 10,
      "percentage": 14.08
    },
    {
      "categoryId": 5,
      "categoryName": "Biography / Diary",
      "loanCount": 6,
      "percentage": 8.45
    }
  ],
  "totalLoans": 71
}
```

---

### **3. Top 5 Ng??i Dùng Tích C?c (Top Users)**
```
GET /api/Report/top-users
```

**Parameters:**
- `period` (required): "month", "year"
- `year` (required): 2026
- `month` (optional): 1-12 (khi period = "month")

**Example Requests:**
```bash
# Top 5 ng??i dùng tháng 1/2026
GET /api/Report/top-users?period=month&year=2026&month=1

# Top 5 ng??i dùng n?m 2026
GET /api/Report/top-users?period=year&year=2026
```

**Response:**
```json
{
  "users": [
    {
      "userId": "7c54be15-8679-43bf-b238-3260be5182a6",
      "userName": "Toan Doi 1",
      "loanCount": 21,
      "returnedCount": 15,
      "overdueCount": 2
    },
    {
      "userId": "8d65cf26-9790-54cg-c349-4371cf62930b",
      "userName": "Hoang Van B",
      "loanCount": 18,
      "returnedCount": 16,
      "overdueCount": 1
    },
    {
      "userId": "9e76dg37-a8a1-65dh-d45a-5482dg73a41c",
      "userName": "Linh Thi C",
      "loanCount": 15,
      "returnedCount": 13,
      "overdueCount": 0
    },
    {
      "userId": "af87eh48-b9b2-76ei-e56b-6593eh84b52d",
      "userName": "Minh Van D",
      "loanCount": 12,
      "returnedCount": 10,
      "overdueCount": 1
    },
    {
      "userId": "bg98fi59-cac3-87fj-f67c-76a4fi95c63e",
      "userName": "Thu Thi E",
      "loanCount": 10,
      "returnedCount": 9,
      "overdueCount": 0
    }
  ]
}
```

---

### **4. Top 5 Sách Hot (Top Books)**
```
GET /api/Report/top-books
```

**Parameters:**
- `period` (required): "month", "year"
- `year` (required): 2026
- `month` (optional): 1-12 (khi period = "month")

**Example Requests:**
```bash
# Top 5 sách tháng 1/2026
GET /api/Report/top-books?period=month&year=2026&month=1

# Top 5 sách n?m 2026
GET /api/Report/top-books?period=year&year=2026
```

**Response:**
```json
{
  "books": [
    {
      "bookId": 1,
      "bookTitle": "Sapiens: A Brief History of Humankind",
      "categoryName": "Science / Cosmology",
      "loanCount": 8,
      "returnedCount": 6
    },
    {
      "bookId": 2,
      "bookTitle": "Atomic Habits",
      "categoryName": "Self-help / Personal Development",
      "loanCount": 6,
      "returnedCount": 5
    },
    {
      "bookId": 3,
      "bookTitle": "The Name of the Wind",
      "categoryName": "Fantasy",
      "loanCount": 5,
      "returnedCount": 4
    },
    {
      "bookId": 4,
      "bookTitle": "Sapiens: A Brief History of Humankind",
      "categoryName": "Science / Cosmology",
      "loanCount": 4,
      "returnedCount": 3
    },
    {
      "bookId": 5,
      "bookTitle": "The Nightingale",
      "categoryName": "Historical Fiction",
      "loanCount": 3,
      "returnedCount": 2
    }
  ]
}
```

---

## ?? Frontend Usage

### **React Component Example**
```typescript
import { 
  getLoanTrend,
  getCategoryDistribution,
  getTopUsers,
  getTopBooks 
} from './services/reportService';

export const ReportPage = () => {
  const [month, setMonth] = useState(1);
  const [year, setYear] = useState(2026);
  const [trendData, setTrendData] = useState(null);
  const [categoryData, setCategoryData] = useState(null);
  const [topUsers, setTopUsers] = useState(null);
  const [topBooks, setTopBooks] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const trend = await getLoanTrend('month', year, month);
        const category = await getCategoryDistribution(year, month);
        const users = await getTopUsers('month', year, month);
        const books = await getTopBooks('month', year, month);

        setTrendData(trend);
        setCategoryData(category);
        setTopUsers(users);
        setTopBooks(books);
      } catch (error) {
        console.error('Error loading report:', error);
      }
    };

    fetchData();
  }, [month, year]);

  const handleFilterChange = (newMonth: number, newYear: number) => {
    setMonth(newMonth);
    setYear(newYear);
  };

  return (
    <div className="p-6">
      {/* Filter Controls */}
      <div className="mb-6 flex gap-4">
        <select value={month} onChange={(e) => handleFilterChange(parseInt(e.target.value), year)}>
          {Array.from({length: 12}, (_, i) => i + 1).map(m => (
            <option key={m} value={m}>Tháng {m}</option>
          ))}
        </select>
        <select value={year} onChange={(e) => handleFilterChange(month, parseInt(e.target.value))}>
          {[2025, 2026, 2027].map(y => (
            <option key={y} value={y}>N?m {y}</option>
          ))}
        </select>
      </div>

      {/* Xu H??ng Chart */}
      {trendData && <LineChart data={trendData.data} />}

      {/* Category Distribution Pie */}
      {categoryData && <PieChart data={categoryData.categories} />}

      {/* Top Users Table */}
      {topUsers && <TopUsersTable users={topUsers.users} />}

      {/* Top Books Table */}
      {topBooks && <TopBooksTable books={topBooks.books} />}
    </div>
  );
};
```

---

## ?? Authorization

T?t c? endpoints yêu c?u:
- **Authentication**: Bearer token
- **Authorization**: AdminOnly role

---

## ? Tóm T?t

| Endpoint | Ch?c N?ng | Period |
|----------|----------|---------|
| `/api/Report/loan-trend` | Xu h??ng m??n sách | week, month, year |
| `/api/Report/category-distribution` | Phân b? th? lo?i | month |
| `/api/Report/top-users` | Top 5 ng??i dùng | month, year |
| `/api/Report/top-books` | Top 5 sách hot | month, year |

T?t c? có th? l?c theo **tháng/n?m** và m?t s? theo **tu?n**.
