# VocabMaster

Ứng dụng học từ vựng tiếng Anh với ASP.NET Core API và React.

## Cấu trúc dự án

- **VocabMaster.API**: Backend API sử dụng ASP.NET Core
- **VocabMaster.Core**: Thư viện chứa các entities, interfaces và DTOs
- **VocabMaster.Data**: Lớp truy cập dữ liệu và migrations
- **VocabMaster.Services**: Các services xử lý business logic
- **VocabMaster.Tests**: Unit tests
- **VocabMaster.Client**: Frontend React

## Cài đặt và chạy dự án

### Backend (ASP.NET Core)

1. Mở solution trong Visual Studio
2. Khôi phục các packages NuGet
3. Chạy migrations để tạo database
4. Khởi chạy project VocabMaster.API

### Frontend (React)

Dự án React sử dụng nhiều dependencies, để tối ưu việc quản lý và tránh đẩy node_modules lên Git, hãy làm theo các bước sau:

1. **Cài đặt dependencies**:
   ```bash
   cd VocabMaster.Client
   npm run install:clean
   ```

2. **Khởi chạy ứng dụng React**:
   ```bash
   npm start
   ```

3. **Build cho production**:
   ```bash
   npm run build
   ```

## Best practices cho quản lý dependencies

1. **Không commit node_modules**: Thư mục này đã được thêm vào .gitignore
2. **Không commit package-lock.json**: Đã cấu hình npm để không tạo file này
3. **Sử dụng phiên bản chính xác**: Cấu hình save-exact=true trong .npmrc
4. **Chỉ commit package.json**: Đảm bảo file này được cập nhật khi thêm dependencies mới

## Cài đặt trong môi trường CI/CD

```bash
cd VocabMaster.Client
npm run install:ci
npm run build
```

## Giải quyết vấn đề nhiều files

- Sử dụng .gitignore để loại bỏ node_modules và các file build
- Cấu hình npm để không tạo package-lock.json
- Sử dụng npm ci trong môi trường CI/CD để cài đặt dependencies nhanh hơn 